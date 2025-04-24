import os
import json
import torch
import numpy as np
import soundfile as sf
import wget
from scipy.spatial.distance import cdist
from speechbrain.inference.speaker import EncoderClassifier
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml
import torchaudio
from openai import OpenAI
from config import OPENAI_API_KEY, THRESHOLD, RATE, MODEL_CONFIG, YAML_NAME, DATA_DIR, SPEAKER_DIR
from vosk import Model, KaldiRecognizer
import queue
import threading
import pyaudio

model = Model("D:/Diploma/vosk-model-uk-v3-lgraph")

def create_vosk_recognizer():
    return KaldiRecognizer(model, RATE)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
client = OpenAI(api_key=OPENAI_API_KEY)

classifier = EncoderClassifier.from_hparams(
    source="speechbrain/spkrec-ecapa-voxceleb",
    run_opts={"device": device}
).to(device)

diar_model = SortformerEncLabelModel.from_pretrained("nvidia/diar_sortformer_4spk-v1")
diar_model.eval()

known_speakers = []
known_speaker_ids = []

def load_known_speakers():
    global known_speakers, known_speaker_ids
    known_speakers = []
    known_speaker_ids = []
    if os.path.exists(SPEAKER_DIR):
        for file in os.listdir(SPEAKER_DIR):
            if file.endswith(".wav"):
                speaker_name = os.path.splitext(file)[0]
                file_path = os.path.join(SPEAKER_DIR, file)
                waveform, sr_file = torchaudio.load(file_path)
                waveform = waveform.to(device)
                embedding = classifier.encode_batch(waveform)
                known_speakers.append(embedding.squeeze(1).cpu().numpy())
                known_speaker_ids.append(speaker_name)

def load_yaml_config():
    global MODEL_CONFIG
    if not os.path.exists(MODEL_CONFIG):
        config_url = f"https://raw.githubusercontent.com/NVIDIA/NeMo/main/examples/speaker_tasks/diarization/conf/post_processing/{YAML_NAME}"
        print("Завантаження YAML конфігурації для пост-обробки...")
        MODEL_CONFIG = wget.download(config_url, DATA_DIR)
    return load_postprocessing_from_yaml(MODEL_CONFIG)

def get_segment_embedding(segment_signal):
    audio_data = segment_signal.astype(np.float32)
    tensor = torch.from_numpy(audio_data).unsqueeze(0).to(device)
    with torch.no_grad():
        embedding = classifier.encode_batch(tensor)
    return embedding.squeeze(1).cpu().numpy()

async def receive_id(websocket):
    try:
        message = await websocket.receive()
        if "text" in message and message["text"]:
            init_message = message["text"]
        elif "bytes" in message and message["bytes"]:
            init_message = message["bytes"].decode("utf-8")
        else:
            await websocket.send_text("Error: missing id")
            await websocket.close()
            return None

        data = json.loads(init_message)

        end_time = data.get("EndTime")
        user_voice = data.get("UserVoice")
        end_phrase = data.get("EndPhrase")
        isFirstRequest = data.get("IsFirstRequest")
        previousResponseId = data.get("PreviousResponseId")
        
        await websocket.send_text("OK")# якщо все необхідне є
        return end_time, user_voice, end_phrase, isFirstRequest, previousResponseId
    except Exception as e:
        await websocket.send_text("Invalid id message")
        await websocket.close()
        return None

def listen_for_phrase(recognizer, stop_event, phrase, audio_q):
    while not stop_event.is_set():
        if not audio_q.empty():
            data = audio_q.get()
            if recognizer.AcceptWaveform(data):
                result = json.loads(recognizer.Result())
                if "text" in result and phrase.lower() in result["text"].lower():
                    stop_event.set()


import time

async def receive_audio(websocket, end_time, user_voice, end_phrase, isFirstRequest):
    audio_buffer = []                  # сюди накопичуємо весь потік
    classification_buffer = []
    classification_buffer_duration = 0.0
    window_duration = 1.0
    last_me_detected_time = 0.0
    total_duration = 0.0

    # Якщо перший запит — вважати, що ми вже “вловили” голос
    speech_detected = isFirstRequest

    # 1) При isFirstRequest=False: чекаємо до 10 с на старт вашого голосу
    if not isFirstRequest:
        init_timeout = 10.0
        init_start = time.monotonic()
        init_buffer = []
        init_buffer_dur = 0.0

        while time.monotonic() - init_start < init_timeout:
            msg = await websocket.receive()
            if msg.get("type") != "websocket.receive" or "bytes" not in msg:
                continue

            data = msg["bytes"]
            chunk = np.frombuffer(data, dtype=np.int16).astype(np.float32) / 32768.0
            chunk_duration = len(chunk) / RATE
            total_duration += chunk_duration

            # 1.1) Зберігаємо весь потік
            audio_buffer.append(chunk)
            # 1.2) Окремо для первинної перевірки
            init_buffer.append(chunk)
            init_buffer_dur += chunk_duration

            # Кожен раз, коли набираємо window_duration, перевіряємо embedding
            if init_buffer_dur >= window_duration:
                seg = np.concatenate(init_buffer)
                init_buffer = []
                init_buffer_dur = 0.0

                emb = get_segment_embedding(seg)
                me_emb = np.array(user_voice)
                dist = cdist(
                    np.atleast_2d(emb),
                    np.atleast_2d(me_emb),
                    metric="cosine"
                ).min()

                if dist < THRESHOLD:
                    speech_detected = True
                    break

        if not speech_detected:
            print(f"Timeout: no speech detected in {init_timeout} s")
            return np.array([]), False

    # 2) Основний цикл: збираємо весь потік + перевіряємо end_time / end_phrase
    stop_event = threading.Event()
    if end_phrase:
        recognizer = create_vosk_recognizer()
        listener_thread = threading.Thread(
            target=lambda: listen_for_phrase(recognizer, stop_event, end_phrase, None)
        )
        listener_thread.start()

    try:
        while True:
            message = await websocket.receive()
            # якщо прийшов спеціальний “end”
            if message.get("type") == "websocket.receive" and "text" in message and message["text"] == "end":
                break

            # якщо атач із байтами
            if message.get("type") == "websocket.receive" and "bytes" in message:
                data = message["bytes"]
                chunk = np.frombuffer(data, dtype=np.int16).astype(np.float32) / 32768.0
                chunk_duration = len(chunk) / RATE
                total_duration += chunk_duration

                # 2.1) Завжди збираємо в загальний буфер
                audio_buffer.append(chunk)
                # 2.2) Класіфікаційний буфер для end_time
                classification_buffer.append(chunk)
                classification_buffer_duration += chunk_duration

                # Обробка за end_time
                if end_time is not None:
                    if classification_buffer_duration >= window_duration:
                        window_audio = np.concatenate(classification_buffer)
                        classification_buffer = []
                        classification_buffer_duration = 0.0

                        embedding = get_segment_embedding(window_audio)
                        me_embedding = np.array(user_voice)
                        distances = cdist(
                            np.atleast_2d(embedding),
                            np.atleast_2d(me_embedding),
                            metric="cosine"
                        )
                        if distances.min() < THRESHOLD:
                            last_me_detected_time = total_duration

                    if total_duration - last_me_detected_time >= int(end_time):
                        break

                # Обробка за end_phrase
                elif end_phrase is not None:
                    if recognizer.AcceptWaveform(data):
                        result = json.loads(recognizer.Result())
                        text = result.get("text", "")
                        if end_phrase.lower() in text.lower():
                            break

            # клієнт відключився
            if message.get("type") == "websocket.disconnect":
                break

    except Exception as e:
        print("WebSocket відключено або помилка:", e)
    finally:
        if end_phrase:
            stop_event.set()
            listener_thread.join()

    full_audio = np.concatenate(audio_buffer)
    return full_audio, True

def process_audio_segments(full_audio, user_voice):
    final_audio_segments = []
    temp_audio_path = "temp_stream.wav"
    sf.write(temp_audio_path, full_audio, RATE)

    if full_audio is None or full_audio.size == 0:
        return []

    pred_list_pp = diar_model.diarize(audio=temp_audio_path, postprocessing_yaml=MODEL_CONFIG)[0]
    sorted_segments = sorted(pred_list_pp, key=lambda seg: float(seg.split()[0]))

    for segment in sorted_segments:
        parts = segment.split()
        if len(parts) < 3:
            continue
        start_str, end_str, _ = parts
        start = float(start_str)
        end = float(end_str)
        start_sample = int(start * RATE)
        end_sample = int(end * RATE)
        segment_signal = full_audio[start_sample:end_sample]

        seg_embedding = get_segment_embedding(segment_signal)
        me_embedding = np.array(user_voice)
        
        distances = cdist(np.atleast_2d(seg_embedding), np.atleast_2d(me_embedding), metric="cosine")
        if distances.min() < THRESHOLD:
            final_audio_segments.append(segment_signal)

    os.remove(temp_audio_path)
    return final_audio_segments

def transcribe_audio(final_audio_segments):
    transcription = "none"
    if final_audio_segments:
        combined_audio = np.concatenate(final_audio_segments)
        final_audio_file = "final_audio.wav"
        sf.write(final_audio_file, combined_audio, RATE)
        with open(final_audio_file, "rb") as audio_file:
            transcription = client.audio.transcriptions.create(
                model="whisper-1",
                file=audio_file,
                language="uk"
            )
        os.remove(final_audio_file)
    return transcription

load_known_speakers()
post_processing_params = load_yaml_config()


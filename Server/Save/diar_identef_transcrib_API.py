from fastapi import FastAPI, WebSocket, WebSocketDisconnect
import uvicorn
import os
import torch
import numpy as np
import librosa
import soundfile as sf
import wget
from scipy.spatial.distance import cdist
from speechbrain.inference.speaker import EncoderClassifier
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml
import torchaudio
from openai import OpenAI

app = FastAPI()
client = OpenAI(api_key="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")

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
RATE = 16000

classifier = EncoderClassifier.from_hparams(
    source="speechbrain/spkrec-ecapa-voxceleb",
    run_opts={"device": device}
)
classifier = classifier.to(device)

known_speakers = []
known_speaker_ids = []
speaker_files = [
    "data/SteveJobs.wav",
    "data/ElonMusk.wav",
    "data/NelsonMandela.wav",
    "data/Me.wav"
]
speaker_names = ["Steve Jobs", "Elon Musk", "Nelson Mandela", "Me"]

for file, name in zip(speaker_files, speaker_names):
    waveform, sr_file = torchaudio.load(file)
    waveform = waveform.to(device)
    embedding = classifier.encode_batch(waveform)
    known_speakers.append(embedding.squeeze(1).cpu().numpy())
    known_speaker_ids.append(name)

threshold = 0.8

diar_model = SortformerEncLabelModel.from_pretrained("nvidia/diar_sortformer_4spk-v1")
diar_model.eval()

data_dir = os.getcwd()
yaml_name = "sortformer_diar_4spk-v1_dihard3-dev.yaml"
MODEL_CONFIG = os.path.join(data_dir, yaml_name)
if not os.path.exists(MODEL_CONFIG):
    config_url = f"https://raw.githubusercontent.com/NVIDIA/NeMo/main/examples/speaker_tasks/diarization/conf/post_processing/{yaml_name}"
    print("Завантаження YAML конфігурації для пост-обробки...")
    MODEL_CONFIG = wget.download(config_url, data_dir)
post_processing_params = load_postprocessing_from_yaml(MODEL_CONFIG)

# Функція для отримання ембеддінгу сегмента
def get_segment_embedding(segment_signal):
    audio_data = segment_signal.astype(np.float32)
    tensor = torch.from_numpy(audio_data).unsqueeze(0).to(device)
    with torch.no_grad():
        embedding = classifier.encode_batch(tensor)
    return embedding.squeeze(1).cpu().numpy()

@app.websocket("/ws/audio")
async def websocket_audio(websocket: WebSocket):
    await websocket.accept()
    audio_buffer = []          
    classification_buffer = [] 
    classification_buffer_duration = 0.0
    window_duration = 1.0     
    last_me_detected_time = 0.0
    total_duration = 0.0      
    final_audio_segments = [] 

    try:
        while True:
            message = await websocket.receive()
            if message["type"] == "websocket.receive":

                if "text" in message and message["text"] == "end":
                    break
                elif "bytes" in message:
                    data = message["bytes"]
                    
                    chunk = np.frombuffer(data, dtype=np.int16).astype(np.float32) / 32768.0
                    chunk_duration = len(chunk) / RATE
                    total_duration += chunk_duration

                    audio_buffer.append(chunk)
                    classification_buffer.append(chunk)
                    classification_buffer_duration += chunk_duration

                    if classification_buffer_duration >= window_duration:
                        window_audio = np.concatenate(classification_buffer)
                        classification_buffer = []
                        classification_buffer_duration = 0.0

                        embedding = get_segment_embedding(window_audio)
                        me_embedding = known_speakers[3]
                        distances = cdist(np.atleast_2d(embedding), np.atleast_2d(me_embedding), metric="cosine")
                        if distances.min() < threshold:
                            last_me_detected_time = total_duration

                    if total_duration - last_me_detected_time >= 5.0 and total_duration > 5.0:
                        break
            elif message["type"] == "websocket.disconnect":
                break

    except WebSocketDisconnect:
        print("WebSocket відключено клієнтом.")

    full_audio = np.concatenate(audio_buffer)
    sf.write("received_audio.wav", full_audio, RATE)
    
    temp_audio_path = "temp_stream.wav"
    sf.write(temp_audio_path, full_audio, RATE)

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
        me_embedding = known_speakers[3]
        distances = cdist(np.atleast_2d(seg_embedding), np.atleast_2d(me_embedding), metric="cosine")
        if distances.min() < threshold:
            final_audio_segments.append(segment_signal)

    transcription = {"text": "No audio segments detected for transcription."}

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
        
        transcription_text = transcription.text if hasattr(transcription, "text") else str(transcription)
        
        """
        completion = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
                {
                    "role": "user",
                    "content": transcription_text
                }
            ]
        )"""

    # Clean-up
    os.remove(temp_audio_path)
    print(transcription_text)
    #print(completion.choices[0].message.content)

    await websocket.send_json({"transcripts": transcription_text})
    await websocket.close()

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8001)

import io
import numpy as np
import torch
import torchaudio
from scipy.spatial.distance import cdist
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
import uvicorn
from speechbrain.inference.speaker import EncoderClassifier

app = FastAPI()

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

classifier = EncoderClassifier.from_hparams(
    source="speechbrain/spkrec-ecapa-voxceleb",
    run_opts={"device": device}
).to(device)

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
    waveform, sr = torchaudio.load(file)
    waveform = waveform.to(device)
    with torch.no_grad():
        embedding = classifier.encode_batch(waveform)
    known_speakers.append(embedding.squeeze(1).cpu().numpy())
    known_speaker_ids.append(name)

threshold = 0.8 


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    try:
        while True:
            audio_bytes = await websocket.receive_bytes()

            audio_buffer = io.BytesIO(audio_bytes)
            waveform, sr_audio = torchaudio.load(audio_buffer)
            if sr_audio != 16000:
                        waveform = torchaudio.transforms.Resample(sr_audio, 16000)(waveform)
            waveform = waveform.to(device)

            # Обчислення embedding для отриманого аудіо
            with torch.no_grad():
                embedding = classifier.encode_batch(waveform).squeeze(1).cpu().numpy()
            embedding_2d = np.atleast_2d(embedding)

            # Порівняння з відомими спікерами
            identified = False
            for known_embed, known_name in zip(known_speakers, known_speaker_ids):
                known_embed_2d = np.atleast_2d(known_embed)
                distances = cdist(embedding_2d, known_embed_2d, metric="cosine")
                if distances.min() < threshold:
                    response = f"Ідентифіковано спікера: {known_name}"
                    identified = True
                    break
            if not identified:
                response = "Спікер не розпізнаний."

            await websocket.send_text(response)
    except WebSocketDisconnect:
        print("Клієнт відключився.")

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)

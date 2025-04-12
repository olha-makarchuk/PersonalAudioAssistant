import os
import io
import torch
import torchaudio
import numpy as np
from fastapi import APIRouter, HTTPException, UploadFile, File, Form
from speechbrain.inference.speaker import EncoderClassifier
from config import DEVICE  

router = APIRouter()

classifier = EncoderClassifier.from_hparams(
    source="speechbrain/spkrec-ecapa-voxceleb",
    run_opts={"device": DEVICE}
)
classifier = classifier.to(DEVICE)

def get_segment_embedding(waveform: torch.Tensor) -> np.ndarray:
    """Отримання ембеддінгу для заданого waveform."""
    with torch.no_grad():
        embedding = classifier.encode_batch(waveform)
    return embedding.squeeze(1).cpu().numpy()

@router.post("")
async def upload_and_get_embedding(file: UploadFile = File(...)):
    # Перевірка формату файлу
    file_extension = os.path.splitext(file.filename)[1]
    if file_extension.lower() != ".wav":
        raise HTTPException(status_code=400, detail="Непідтримуваний формат файлу. Використовуйте WAV.")
    
    # Зчитування файлу як байтів та завантаження аудіо
    contents = await file.read()
    with io.BytesIO(contents) as audio_file:
        try:
            waveform, sample_rate = torchaudio.load(audio_file)
        except Exception as e:
            raise HTTPException(status_code=400, detail=f"Не вдалося завантажити аудіо: {str(e)}")
    
    # Переміщення даних на потрібний пристрій (CPU/CUDA)
    waveform = waveform.to(DEVICE)
    
    # Отримання ембеддінгу
    embedding = get_segment_embedding(waveform)
    
    return {"embedding": embedding.tolist()}
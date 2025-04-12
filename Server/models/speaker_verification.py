import torch
import torchaudio
import numpy as np
from scipy.spatial.distance import cdist
from speechbrain.inference.speaker import EncoderClassifier
from config import DEVICE, SPEAKER_FILES, SPEAKER_NAMES, THRESHOLD, RATE

# Завантаження класифікатора для верифікації спікера
classifier = EncoderClassifier.from_hparams(
    source="speechbrain/spkrec-ecapa-voxceleb",
    run_opts={"device": DEVICE}
)
classifier = classifier.to(DEVICE)

# Підготовка відомих спікерів
known_speakers = []
known_speaker_ids = []
for file, name in zip(SPEAKER_FILES, SPEAKER_NAMES):
    waveform, sr_file = torchaudio.load(file)
    waveform = waveform.to(DEVICE)
    embedding = classifier.encode_batch(waveform)
    known_speakers.append(embedding.squeeze(1).cpu().numpy())
    known_speaker_ids.append(name)

def get_segment_embedding(segment_signal: np.ndarray) -> np.ndarray:
    """Отримання ембеддінгу для аудіосегмента."""
    audio_data = segment_signal.astype(np.float32)
    tensor = torch.from_numpy(audio_data).unsqueeze(0).to(DEVICE)
    with torch.no_grad():
        embedding = classifier.encode_batch(tensor)
    return embedding.squeeze(1).cpu().numpy()
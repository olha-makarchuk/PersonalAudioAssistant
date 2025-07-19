import os
import torch
import wget
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml

OPENAI_API_KEY = "api-key"
ELEVENLABS_API_KEY = "api-key"

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

RATE = 16000

DATA_DIR = os.getcwd()
DATA_FOLDER = os.path.join(DATA_DIR, "data")

SPEAKER_FILES = [os.path.join(DATA_FOLDER, f) for f in os.listdir(DATA_FOLDER) if f.endswith(".wav")]
SPEAKER_NAMES = [os.path.splitext(f)[0] for f in os.listdir(DATA_FOLDER) if f.endswith(".wav")]

print("Виявлено спікерів:", SPEAKER_NAMES)

THRESHOLD = 0.8

YAML_NAME = "sortformer_diar_4spk-v1_dihard3-dev.yaml"
MODEL_CONFIG = os.path.join(DATA_DIR, YAML_NAME)

diar_model = SortformerEncLabelModel.from_pretrained("nvidia/diar_sortformer_4spk-v1")
diar_model.eval()

if not os.path.exists(MODEL_CONFIG):
    config_url = f"https://raw.githubusercontent.com/NVIDIA/NeMo/main/examples/speaker_tasks/diarization/conf/post_processing/{YAML_NAME}"
    print("Завантаження YAML конфігурації для пост-обробки...")
    MODEL_CONFIG = wget.download(config_url, DATA_DIR)

post_processing_params = load_postprocessing_from_yaml(MODEL_CONFIG)

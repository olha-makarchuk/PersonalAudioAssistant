import os
import torch
import wget
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml

OPENAI_API_KEY = "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"
ELEVENLABS_API_KEY = "sk_27c32e41339106ed731a786439faaf7079426123259cdec4"

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

RATE = 16000

DATA_DIR = os.getcwd()
SPEAKER_DIR = "data/speakers"
THRESHOLD = 0.7

YAML_NAME = "sortformer_diar_4spk-v1_dihard3-dev.yaml"
MODEL_CONFIG = os.path.join(DATA_DIR, YAML_NAME)
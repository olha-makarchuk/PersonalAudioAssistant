import os
import torch
import wget
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml

OPENAI_API_KEY = "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"
ELEVENLABS_API_KEY = "sk_27c32e41339106ed731a786439faaf7079426123259cdec4"

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

RATE = 44100

DATA_DIR = os.getcwd()
SPEAKER_DIR = "data/speakers"
THRESHOLD = 0.6

YAML_NAME = "sortformer_diar_4spk-v1_dihard3-dev.yaml"
MODEL_CONFIG = os.path.join(DATA_DIR, YAML_NAME)
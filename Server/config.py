import os
import torch
import wget
from nemo.collections.asr.models import SortformerEncLabelModel
from nemo.collections.asr.parts.utils.vad_utils import load_postprocessing_from_yaml

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

RATE = 44100

DATA_DIR = os.getcwd()
SPEAKER_DIR = "data/speakers"
THRESHOLD = 0.5

YAML_NAME = "sortformer_diar_4spk-v1_dihard3-dev.yaml"
MODEL_CONFIG = os.path.join(DATA_DIR, YAML_NAME)

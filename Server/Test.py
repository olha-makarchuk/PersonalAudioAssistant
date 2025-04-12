from fastapi import APIRouter
from elevenlabs import ElevenLabs
from config import ELEVENLABS_API_KEY

elevenlabs_client = ElevenLabs(api_key="sk_27c32e41339106ed731a786439faaf7079426123259cdec4")

class ModelResult:
    def __init__(self, voiceId: str, name:str, URL: str):
        self.voiceId = voiceId
        self.name = name
        self.URL = URL


result = []
voices = elevenlabs_client.voices.get_all()
for voice in voices:
    v = voice[1]
        
    for vi in v:
        fine_tuning_state = vi.fine_tuning.state.get("eleven_flash_v2_5")
        if fine_tuning_state == "fine_tuned":
            model = ModelResult(vi.voice_id, vi.name, vi.preview_url)
            result.append(model)

print(result)


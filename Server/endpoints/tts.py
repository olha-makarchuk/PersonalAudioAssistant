from fastapi import APIRouter
from elevenlabs import ElevenLabs
from config import ELEVENLABS_API_KEY

router = APIRouter()
elevenlabs_client = ElevenLabs(api_key=ELEVENLABS_API_KEY)

@router.post("")
async def text_to_speech(text: str, voice_id: str = "onwK4e9ZLuTAKqWW03F9"):
    audio_stream = elevenlabs_client.text_to_speech.convert_as_stream(
        text=text,
        voice_id=voice_id,
        model_id="eleven_flash_v2_5"
    )
    audio_bytes = b''.join(chunk for chunk in audio_stream if isinstance(chunk, bytes))
    output_path = "output.mp3"
    with open(output_path, "wb") as f:
        f.write(audio_bytes)
    return {"message": "Audio saved", "file": output_path}

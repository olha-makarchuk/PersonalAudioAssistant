from elevenlabs.client import ElevenLabs

"""
client = ElevenLabs(
    api_key='sk_27c32e41339106ed731a786439faaf7079426123259cdec4',
)

audio_stream = client.text_to_speech.convert_as_stream(
    text="Перебування на висоті прискорює ріст волосся.",
    voice_id="onwK4e9ZLuTAKqWW03F9",
    model_id="eleven_flash_v2_5"
)

audio_bytes = b''.join(chunk for chunk in audio_stream if isinstance(chunk, bytes))

with open("output.mp3", "wb") as f:
    f.write(audio_bytes)

print("Аудіо збережено у файлі output.mp3")
"""


"""
client = ElevenLabs(
    api_key="sk_27c32e41339106ed731a786439faaf7079426123259cdec4",
)
print(client.models.get_all())
"""

"""
from elevenlabs import ElevenLabs

client = ElevenLabs(
    api_key="sk_27c32e41339106ed731a786439faaf7079426123259cdec4",
)
print(client.voices.get_all())
"""

from elevenlabs import ElevenLabs
client = ElevenLabs(
    api_key="YOUR_API_KEY",
)
client.voices.add(
    name="Alex",
    files="path"
)#getVoiceId
from fastapi import APIRouter, WebSocket
import soundfile as sf
from config import RATE
from services.AudioService import (
    receive_id,
    receive_audio,
    process_audio_segments,
    transcribe_audio,
    known_speaker_ids
)

router = APIRouter()

@router.websocket("/audio")
async def websocket_audio(websocket: WebSocket):
    await websocket.accept()

    id_user, end_time, user_voice, end_phrase = await receive_id(websocket)
    if id_user is None:
        return

    if str(id_user) in known_speaker_ids:
        idx = known_speaker_ids.index(str(id_user))
        print(f"Спікер з id {id_user} знайдений. Індекс: {idx}")
    else:
        print(f"Спікера з id {id_user} не знайдено.")
        idx = None  

    full_audio = await receive_audio(websocket, idx, end_time, user_voice, end_phrase)

    #sf.write("received_audio.wav", full_audio, RATE)

    final_audio_segments = process_audio_segments(full_audio, idx, user_voice)

    transcription = transcribe_audio(final_audio_segments)
    transcription_text = transcription.text if hasattr(transcription, "text") else str(transcription)

    print(transcription_text)
    await websocket.send_json({"transcripts": transcription_text})
    await websocket.close()

import ast
from fastapi import APIRouter, WebSocket
import soundfile as sf
from config import RATE, OPENAI_API_KEY
from openai import OpenAI
from services.AudioService import (
    receive_id,
    receive_audio,
    process_audio_segments,
    transcribe_audio,
    known_speaker_ids
)
import base64
import io

router = APIRouter()
client = OpenAI(api_key=OPENAI_API_KEY)

@router.websocket("/audio")
async def websocket_audio(websocket: WebSocket):
    await websocket.accept()

    end_time, user_voice, end_phrase, isFirstRequest, previousResponseId = await receive_id(websocket)

    full_audio, isContinuous  = await receive_audio(websocket, end_time, user_voice, end_phrase, isFirstRequest)

    if isContinuous == False:
        await websocket.send_json({
            "request": "none",
            "audioDuration": 0,
            "isContinuous": isContinuous
        })
        await websocket.close()
        return
    else:

        final_audio_segments = process_audio_segments(full_audio, user_voice)

        transcription, duration_seconds = transcribe_audio(final_audio_segments)
        transcription_text = transcription.text if hasattr(transcription, "text") else str(transcription)

        print(transcription_text)

        await websocket.send_json({
            "request": transcription_text,
            "audioDuration": duration_seconds,
            "isContinuous": True
        })
        
        await websocket.close()
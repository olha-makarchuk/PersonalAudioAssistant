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
            "transcripts": "",
            "isContinuous": isContinuous,
            "conversationId": "",
            "AIconversationId": ""
        })
        await websocket.close()
        return
    else:
        
        #sf.write("received_audio.wav", full_audio, RATE)

        final_audio_segments = process_audio_segments(full_audio, user_voice)

        transcription = transcribe_audio(final_audio_segments)
        transcription_text = transcription.text if hasattr(transcription, "text") else str(transcription)

        print(transcription_text)

        
        
        '''
        buffer = io.BytesIO()
        sf.write(buffer, full_audio, RATE, format='WAV')
        buffer.seek(0)
        audio_bytes = buffer.read()

        audio_base64 = base64.b64encode(audio_bytes).decode("utf-8")
        
        await websocket.send_bytes(audio_bytes)
    '''

        await websocket.send_json({
            "request": transcription_text,
            "transcripts": "",
            "isContinuous": True,  
            "conversationId": "",
            "AIconversationId": ""
        })
        
        await websocket.close()
        
        
        
        '''
        completion = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[{
                "role": "user",
                "content": transcription_text,
            }]
        )
        
        response_text = completion.choices[0].message.content
        print("GPT RESPONSE:", response_text)

        await websocket.send_json({
            "request": transcription_text,
            "transcripts": response_text,
            "isContinuous": True,
            "conversationId": "",
            "AIconversationId": ""
        })
        '''
from openai import OpenAI
import os
import json
import torch
import numpy as np
import soundfile as sf

OPENAI_API_KEY = "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"
client = OpenAI(api_key=OPENAI_API_KEY)

audio_file= open("data/speakers/q.mp3", "rb")

transcription = client.audio.transcriptions.create(
    model="gpt-4o-transcribe", 
    file=audio_file
)

print(transcription.text)
'''

audio_file = open("data/speakers/31.wav", "rb")

transcription = client.audio.transcriptions.create(
  model="whisper-1", 
  file=audio_file, 
  response_format="text",
  prompt="ZyntriQix, Digique Plus, CynapseFive, VortiQore V8, EchoNix Array, OrbitalLink Seven, DigiFractal Matrix, PULSE, RAPT, B.R.I.C.K., Q.U.A.R.T.Z., F.L.I.N.T."
)

print(transcription.text)

def transcribe_audio_file(path_to_wav: str):
    with open(path_to_wav, "rb") as audio_file:
        transcription = client.audio.transcriptions.create(
            model="whisper-1",
            file=audio_file,
            language="uk"
        )
    return transcription

# Виклик:
result = transcribe_audio_file("data/speakers/31.wav")
print(result["text"])
'''

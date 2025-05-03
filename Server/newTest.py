from openai import OpenAI
import os
import json
import torch
import numpy as np
import soundfile as sf

'''
OPENAI_API_KEY = "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"
client = OpenAI(api_key=OPENAI_API_KEY)

audio_file= open("data/speakers/q.mp3", "rb")

transcription = client.audio.transcriptions.create(
    model="gpt-4o-transcribe", 
    file=audio_file
)

print(transcription.text)


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

from pydantic import BaseModel
import tiktoken

enc = tiktoken.encoding_for_model("gpt-4")

class TokenRequest(BaseModel):
    text: str

def get_amount_tokens(text):
    text = text
    tokens = enc.encode(text)
    print(len(tokens))

    
print(len("Квантовий комп'ютер працює на основі законів квантової механіки. Замість бітів, які використовуються в класичних комп'ютерах (0 або 1), він використовує кубіти. Кубіт може бути одночасно 0 і 1 завдяки явищу, званому суперпозицією.\n\nОсь кілька ключових моментів, простими словами:\n\n1. **Суперпозиція**: Кубіти можуть бути в кількох станах одночасно, що дозволяє квантовим комп'ютерам виконувати багато обчислень паралельно.\n\n2. **Заплутаність**: Кубіти можуть бути \"пов'язані\" один з одним, навіть на великих відстанях. Це означає, що зміна стану одного кубіта миттєво вплине на інший.\n\n3. **Інтерференція**: Квантові комп'ютери використовують інтерференцію, щоб підсилити правильні результати обчислень і зменшити або скасувати неправильні.\n\nЗавдяки цим властивостям квантові комп'ютери можуть розв'язувати складні задачі набагато швидше, ніж класичні. Наприклад, задачі шифрування або оптимізації."))
print(len("Хмарні технології — це інфраструктура та послуги, які дозволяють зберігати й обробляти дані через Інтернет, замість локальних серверів або особистих комп'ютерів. Вони забезпечують доступ до ресурсів (як-от сервери, сховища, програми) за допомогою веб-браузера, що спрощує масштабування, знижує витрати і підвищує зручність використання."))
print(len("Робототехніка — це галузь науки та техніки, що вивчає, розробляє і використовує роботів. Вона поєднує в собі знання з механіки, електроніки, інформатики, штучного інтелекту та інших дисциплін. Робототехніка охоплює створення автономних і напівавтономних систем, призначених для виконання складних завдань у різних сферах, таких як промисловість, медицина, дослідження та побут."))
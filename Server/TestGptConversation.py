'''

from openai import OpenAI

OPENAI_API_KEY = "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"
client = OpenAI(api_key=OPENAI_API_KEY)

import tiktoken

# Використовуємо токенізатор для GPT-4o
enc = tiktoken.encoding_for_model("gpt-4")

text = "Сонце повільно сходило над горизонтом."
tokens = enc.encode(text)

print(f"Tokens: {tokens}")



response = client.responses.create(
    model="gpt-4.1",
    input=[
        {"role": "user", "content": "Сонце повільно сходило над горизонтом."},
    ],
)

print(response.output_text)


def continue_chat(user_message, prev_response_id=None):
    response_params = {
        "model": "gpt-4o-mini",
        "input": [{"role": "user", "content": user_message}],
        "store": True
    }

    if prev_response_id:
        response_params["previous_response_id"] = prev_response_id

    resp = client.responses.create(**response_params)

    print("Response ID:", resp.id)
    response_text = resp.output[0].content[0].text
    print("Response Text:", response_text)

    return resp 

continue_chat("яке було моє попереднє запитання?", "resp_680515fcaeb08192a564398e5fd913900da8b8fd6c8fe0aa")
'''


from elevenlabs.client import ElevenLabs
from elevenlabs import play

# Ініціалізація клієнта з вашим API-ключем
client = ElevenLabs(api_key="sk_b76dddc4f56ebe8383b50692c9120c637a2243fed76b371f")

# Генерація аудіо з тексту
audio = client.generate(
    text="Привіт! Це тест.",
    voice="Rachel",  # Ви можете змінити на інший доступний голос
    model="eleven_multilingual_v2"
)

# Програвання згенерованого аудіо
play(audio)

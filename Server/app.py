from fastapi import FastAPI
import uvicorn
from endpoints import audio, tts, voices, embendding

app = FastAPI(title="Audio Processing Server")

app.include_router(audio.router, prefix="/ws", tags=["Audio"])
app.include_router(tts.router, prefix="/tts", tags=["Text-to-Speech"])
app.include_router(voices.router, prefix="/voices", tags=["Voices"])
app.include_router(embendding.router, prefix="/embendding", tags=["Embendding"])

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
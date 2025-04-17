from fastapi import FastAPI
import uvicorn
from endpoints import audio, embendding, test

app = FastAPI(title="Audio Processing Server")

app.include_router(audio.router, prefix="/ws", tags=["Audio"])
app.include_router(embendding.router, prefix="/embendding", tags=["Embendding"])
app.include_router(test.router, prefix="/test", tags=["test"])

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
from fastapi import APIRouter
from pydantic import BaseModel
import tiktoken

router = APIRouter()

enc = tiktoken.encoding_for_model("gpt-4")

class TokenRequest(BaseModel):
    text: str

@router.post("")
async def get_amount_tokens(request: TokenRequest):
    text = request.text
    tokens = enc.encode(text)
    return {
        "token_count": len(tokens)
    }
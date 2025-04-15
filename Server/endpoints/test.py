from fastapi import APIRouter

router = APIRouter()

@router.post("")
async def upload_and_get_embedding():
    return [{"id":1, "name": "Olha", "test": "Test"}]
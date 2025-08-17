from fastapi import APIRouter, HTTPException
from src.application.conversation_service.reset_conversation import reset_conversation_state, reset_specific_conversation

router = APIRouter()


@router.post("/reset-memory")
async def reset_all_conversations():
    """Reset all conversation checkpoints in MongoDB"""
    try:
        result = await reset_conversation_state()
        if result["success"]:
            return result
        else:
            raise HTTPException(status_code=500, detail=result["message"])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to reset conversations: {str(e)}")


@router.delete("/reset-memory/{thread_id}")
async def reset_specific_conversation_endpoint(thread_id: str):
    """Reset a specific conversation thread in MongoDB"""
    try:
        result = await reset_specific_conversation(thread_id)
        if result["success"]:
            return result
        else:
            raise HTTPException(status_code=500, detail=result["message"])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to reset thread {thread_id}: {str(e)}")
"""
Reset conversation state functionality for clearing MongoDB checkpoints
"""

import asyncio
from motor.motor_asyncio import AsyncIOMotorClient
from src.config import settings


async def reset_conversation_state() -> dict:
    """
    Deletes all conversation checkpoints from MongoDB.
    
    Returns:
        dict: Summary of deleted data with counts
    """
    try:
        # Connect to MongoDB
        client = AsyncIOMotorClient(settings.MONGO_URI)
        db = client[settings.MONGO_DB_NAME]
        
        # Use the correct collection names from settings
        collections_to_clear = [
            settings.MONGO_STATE_CHECKPOINT_COLLECTION,  # "agent_state_checkpoints"
            settings.MONGO_STATE_WRITES_COLLECTION,      # "agent_state_writes"
            settings.MONGO_LONG_TERM_MEMORY_COLLECTION   # "agent_long_term_memory"
        ]
        
        deletion_summary = {}
        total_deleted = 0
        
        for collection_name in collections_to_clear:
            collection = db[collection_name]
            
            # Count documents before deletion
            count_before = await collection.count_documents({})
            
            # Delete all documents in the collection
            result = await collection.delete_many({})
            
            deletion_summary[collection_name] = {
                "documents_before": count_before,
                "documents_deleted": result.deleted_count
            }
            
            total_deleted += result.deleted_count
        
        # Close the connection
        client.close()
        
        return {
            "success": True,
            "message": f"Successfully deleted {total_deleted} checkpoint documents",
            "details": deletion_summary
        }
        
    except Exception as e:
        return {
            "success": False,
            "message": f"Error resetting conversation state: {str(e)}",
            "details": {}
        }


async def reset_specific_conversation(thread_id: str) -> dict:
    """
    Deletes checkpoints for a specific conversation thread.
    
    Args:
        thread_id (str): The thread ID to delete
        
    Returns:
        dict: Summary of deleted data
    """
    try:
        # Connect to MongoDB
        client = AsyncIOMotorClient(settings.MONGO_URI)
        db = client[settings.MONGO_DB_NAME]
        
        collections_to_clear = [
            settings.MONGO_STATE_CHECKPOINT_COLLECTION,
            settings.MONGO_STATE_WRITES_COLLECTION,
            settings.MONGO_LONG_TERM_MEMORY_COLLECTION
        ]
        
        deletion_summary = {}
        total_deleted = 0
        
        for collection_name in collections_to_clear:
            collection = db[collection_name]
            
            # Delete documents for this specific thread
            result = await collection.delete_many({"thread_id": thread_id})
            
            deletion_summary[collection_name] = {
                "documents_deleted": result.deleted_count
            }
            
            total_deleted += result.deleted_count
        
        # Close the connection
        client.close()
        
        return {
            "success": True,
            "message": f"Successfully deleted {total_deleted} documents for thread {thread_id}",
            "details": deletion_summary
        }
        
    except Exception as e:
        return {
            "success": False,
            "message": f"Error resetting thread {thread_id}: {str(e)}",
            "details": {}
        }

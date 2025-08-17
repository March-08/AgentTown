#delete the long term memory collection from qdrant

from qdrant_client import QdrantClient
from src.config import settings

client = QdrantClient(url=settings.QDRANT_URL)
client.delete_collection(settings.QDRANT_COLLECTION_NAME)

from langchain_qdrant import QdrantVectorStore
from qdrant_client import QdrantClient
from langchain.schema.retriever import BaseRetriever
from langchain_core.documents import Document

from src.config import settings
from .embeddings import get_embedding_model


def get_retriever(
    embedding_model_id: str,
    k: int = 3,
    device: str = "cpu",
) -> BaseRetriever:
    """
    Get a Qdrant retriever.
    """
    return get_qdrant_retriever(
        embedding_model_id=embedding_model_id,
        k=k,
        device=device
    )

def get_qdrant_retriever(
    embedding_model_id: str,
    k: int = 3,
    device: str = "cpu",
) -> BaseRetriever:
    """
    Get a Qdrant vector store retriever.
    """
    embedding_model = get_embedding_model(
        model_name=embedding_model_id,
        device=device
    )
    
    # Configure Qdrant client based on whether API key is provided
    if settings.QDRANT_API_KEY:
        # Use Qdrant Cloud
        qdrant_client = QdrantClient(
            url=settings.QDRANT_URL,
            api_key=settings.QDRANT_API_KEY,
        )
    else:
        # Use local Qdrant
        qdrant_client = QdrantClient(
            url=settings.QDRANT_URL,
        )
    
    # Check if collection exists, create if not
    try:
        qdrant_client.get_collection(settings.QDRANT_COLLECTION_NAME)
        collection_exists = True
    except Exception:
        collection_exists = False
    
    if not collection_exists:
        # Create collection using from_documents with a dummy document
        if settings.QDRANT_API_KEY:
            # Use Qdrant Cloud
            vector_store = QdrantVectorStore.from_documents(
                documents=[Document(page_content="dummy", metadata={})],
                embedding=embedding_model,
                url=settings.QDRANT_URL,
                api_key=settings.QDRANT_API_KEY,
                collection_name=settings.QDRANT_COLLECTION_NAME,
            )
        else:
            # Use local Qdrant
            vector_store = QdrantVectorStore.from_documents(
                documents=[Document(page_content="dummy", metadata={})],
                embedding=embedding_model,
                url=settings.QDRANT_URL,
                collection_name=settings.QDRANT_COLLECTION_NAME,
            )
    else:
        vector_store = QdrantVectorStore(
            client=qdrant_client,
            collection_name=settings.QDRANT_COLLECTION_NAME,
            embedding=embedding_model,
        )
    
    return vector_store.as_retriever(search_kwargs={"k": k})

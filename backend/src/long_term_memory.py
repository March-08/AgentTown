from loguru import logger
from langchain.schema.retriever import BaseRetriever
from langchain_core.documents import Document


from src.application.rag import get_retriever, get_splitter
from src.config import settings
from src.domain.adaptive_agent import AdaptiveAgentExtract, AdaptiveAgent
from langchain_text_splitters import RecursiveCharacterTextSplitter
from src.data import get_extraction_generator, deduplicate_documents

Splitter = RecursiveCharacterTextSplitter


class LongTermMemoryCreator:
    def __init__(self, retriever: BaseRetriever, splitter: Splitter)-> None:
        self.retriever = retriever
        self.splitter = splitter

    @classmethod
    def build_from_settings(cls) -> "LongTermMemoryCreator":
        retriever = get_retriever(
            embedding_model_id=settings.RAG_TEXT_EMBEDDING_MODEL_ID,
            k=settings.RAG_TOP_K,
            device=settings.RAG_DEVICE,
        )
        splitter = get_splitter(chunk_size=settings.RAG_CHUNK_SIZE)
        return cls(retriever, splitter)

    def __call__(self, agents: list[AdaptiveAgentExtract]) -> None:
        if len(agents) == 0:
            logger.warning("No agents to extract. Exiting...")
            return
        
        # First clear the long term memory collection to avoid duplicates.
        try:
            self.retriever.vectorstore.client.delete_collection(settings.QDRANT_COLLECTION_NAME)
        except Exception:
            pass  # Collection might not exist yet
        
        # Recreate the retriever after collection deletion
        from src.application.rag import get_retriever
        self.retriever = get_retriever(
            embedding_model_id=settings.RAG_TEXT_EMBEDDING_MODEL_ID,
            k=settings.RAG_TOP_K,
            device=settings.RAG_DEVICE,
        )
        
        extraction_generator = get_extraction_generator(agents)
        for _, docs in extraction_generator:
            chunked_docs = self.splitter.split_documents(docs)
            chunked_docs = deduplicate_documents(chunked_docs, threshold=0.7)
            self.retriever.vectorstore.add_documents(chunked_docs)



class LongTermMemoryRetriever:
    def __init__(self, retriever: BaseRetriever) -> None:
        self.retriever = retriever

    @classmethod
    def build_from_settings(cls) -> "LongTermMemoryRetriever":
        retriever = get_retriever(
            embedding_model_id=settings.RAG_TEXT_EMBEDDING_MODEL_ID,
            k=settings.RAG_TOP_K,
            device=settings.RAG_DEVICE,
        )

        return cls(retriever)

    def __call__(self, query: str) -> list[Document]:
        return self.retriever.invoke(query)

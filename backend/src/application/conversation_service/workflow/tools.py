from langchain.tools.retriever import create_retriever_tool

from src.application.rag.retriever import get_retriever
from src.config import settings

retriever = get_retriever(
    embedding_model_id=settings.RAG_TEXT_EMBEDDING_MODEL_ID,
    k=settings.RAG_TOP_K,
    device=settings.RAG_DEVICE,
)

retriever_tool = create_retriever_tool(
    retriever,
    "retriever_agent_context",
    "Search and return information about a specific innovator. Always use this tool when the user asks you about an innovator, their companies, innovations or technological contributions and theories.",
    )



tools = [retriever_tool]




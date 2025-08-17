from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langchain_core.runnables.base import RunnableSequence
from langchain_groq import ChatGroq

from src.config import settings
from .tools import tools
from src.domain.prompts import AGENT_CHARACTER_CARD, SUMMARY_PROMPT, CONTEXT_SUMMARY_PROMPT

def get_chat_model(temperature: float = 0.7, model_name: str = settings.GROQ_LLM_MODEL) -> ChatGroq:
    return ChatGroq(
        api_key=settings.GROQ_API_KEY,
        model_name=model_name,
        temperature=temperature,
    )

def get_agent_response_chain() -> RunnableSequence:

    model = get_chat_model()
    model = model.bind_tools(tools)
    system_message = AGENT_CHARACTER_CARD

    prompt = ChatPromptTemplate.from_messages(
        [
            ("system", system_message.prompt),
            MessagesPlaceholder(variable_name="messages"), # so it expects messages in input
        ],
        template_format="jinja2",
    )

    return RunnableSequence(
        prompt,
        model,
    )

def get_conversation_summary_chain() -> RunnableSequence:
    model = get_chat_model()
    system_message = SUMMARY_PROMPT

    prompt = ChatPromptTemplate.from_messages(
        [
            MessagesPlaceholder(variable_name="messages"),
            ("human", system_message.prompt),
            
        ],
        template_format="jinja2",
    )

    return RunnableSequence(
        prompt,
        model,
    )

def get_context_summary_chain() -> RunnableSequence:
    model = get_chat_model()
    system_message = CONTEXT_SUMMARY_PROMPT

    prompt = ChatPromptTemplate.from_messages(
        [
            ("human", system_message.prompt),
        ],
        template_format="jinja2",
    )

    return RunnableSequence(
        prompt,
        model,
    )
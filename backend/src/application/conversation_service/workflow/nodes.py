from langchain_core.runnables import RunnableConfig
from langgraph.prebuilt import ToolNode
from loguru import logger

from src.config import settings

from .state import AgentState
from .chains import get_agent_response_chain, get_conversation_summary_chain, get_context_summary_chain
from .tools import tools


def retriever_node(state):
    logger.info("🔍 RAG ACTIVATED: Retrieving knowledge from vector database")
    result = ToolNode(tools).invoke(state)
    logger.info("✅ RAG COMPLETED: Knowledge retrieved and ready for response")
    return result

def conversation_node(state: AgentState, config: RunnableConfig) -> dict:
    summary = state.get("summary", "")
    conversation_chain = get_agent_response_chain()
    response = conversation_chain.invoke(
        {
            "messages": state["messages"],
            "agent_context": state.get("agent_context", ""),
            "agent_name": state.get("agent_name", "Assistant"),
            "agent_perspective": state.get("agent_perspective", ""),
            "agent_style": state.get("agent_style", ""),
            "summary": summary,
        },
        config,
    )
    
    # Simple RAG detection logging
    if hasattr(response, 'tool_calls') and response.tool_calls:
        logger.info("🤖 AGENT DECISION: Using RAG (detected tool calls in response)")
    else:
        logger.info("🤖 AGENT DECISION: Direct response (no RAG needed)")
    
    return {"messages": [response]}


def summarize_conversation_node(state: AgentState) -> dict:
    summary = state.get("summary", "")
    summary_chain = get_conversation_summary_chain()

    response = summary_chain.invoke(
        {
            "messages": state["messages"],
            "agent_name": state.get("agent_name", "Assistant"),
            "summary": summary,
        },
    )
    
    # Remove old messages after summarizing
    messages = state["messages"][:]
    del messages[:settings.TOTAL_MESSAGES_SUMMARY_TRIGGER - settings.TOTAL_MESSAGES_AFTER_SUMMARY]

    return {"summary": response.content, "messages": messages}


def summarize_context_node(state: AgentState) -> dict:
    context_summary_chain = get_context_summary_chain()

    response = context_summary_chain.invoke(
        {
            "context": state["messages"][-1].content,
        }
    )
    
    # Create a new message list with the summarized content
    messages = state["messages"][:]
    if messages:
        # Handle different message types properly
        last_message = messages[-1]
        role = getattr(last_message, "role", None) or getattr(last_message, "type", "assistant")
        
        # Create a new message with the same type as the original
        from langchain_core.messages import HumanMessage, AIMessage, ToolMessage
        
        if isinstance(last_message, ToolMessage):
            # For ToolMessage, create a new AIMessage with the summarized content
            messages[-1] = AIMessage(content=response.content)
        elif isinstance(last_message, HumanMessage):
            messages[-1] = HumanMessage(content=response.content)
        else:
            # Default to AIMessage
            messages[-1] = AIMessage(content=response.content)
    
    return {"messages": messages}

def connector_node(state: AgentState) -> dict:
    return {}
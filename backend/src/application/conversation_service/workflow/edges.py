from typing import Literal
from langgraph.graph import END

from .state import AgentState
from ....config import settings

def should_summarize_conversation(state: AgentState) -> Literal["summarize_conversation_node", "__end__"]:
    messages = state["messages"]
    
    if len(messages) >= settings.TOTAL_MESSAGES_SUMMARY_TRIGGER:
        return "summarize_conversation_node"
    return END


    
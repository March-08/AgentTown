from functools import lru_cache

from langgraph.graph import StateGraph, START, END
from langgraph.prebuilt import tools_condition

from .state import AgentState
from .edges import should_summarize_conversation
from .nodes import (
    conversation_node, 
    summarize_conversation_node, 
    summarize_context_node, 
    connector_node,
    retriever_node,
)   

@lru_cache(maxsize=1)
def create_workflow_graph() -> StateGraph:
    graph_builder = StateGraph(AgentState)

    # add nodes
    graph_builder.add_node("conversation_node", conversation_node)
    graph_builder.add_node("retrieve_agent_context", retriever_node)
    graph_builder.add_node("summarize_conversation_node", summarize_conversation_node)
    graph_builder.add_node("summarize_context_node", summarize_context_node)
    graph_builder.add_node("connector_node", connector_node)

    # add edges
    graph_builder.add_edge(START, "conversation_node")
    graph_builder.add_conditional_edges(
        "conversation_node",
        tools_condition,
        {
            "tools": "retrieve_agent_context",
            END: "connector_node"
        }
    )
    graph_builder.add_edge("retrieve_agent_context", "summarize_context_node")
    graph_builder.add_edge("summarize_context_node", "conversation_node")
    graph_builder.add_conditional_edges("connector_node", should_summarize_conversation)
    graph_builder.add_edge("summarize_conversation_node", END)
    
    return graph_builder

graph = create_workflow_graph().compile()
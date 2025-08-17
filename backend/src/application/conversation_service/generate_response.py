import uuid

from typing import Union, Any, AsyncGenerator
from langchain_core.messages import HumanMessage, AIMessage, AIMessageChunk
from opik.integrations.langchain import OpikTracer

from .workflow import AgentState, create_workflow_graph
from src.config import settings
from langgraph.checkpoint.mongodb.aio import AsyncMongoDBSaver




async def get_response(
    messages: str | list[str] | list[dict[str, Any]],
    agent_id: str,
    agent_name: str,
    agent_perspective: str,
    agent_style: str,
    agent_context: str,
    new_thread: bool = False,
) -> tuple[str, AgentState]:
    """Run a conversation through the workflow graph.

    Args:
        message: Initial message to start the conversation.
        agent_id: Unique identifier for the agent.
        agent_name: Name of the agent.
        agent_perspective: agent's perspective on the topic.
        agent_style: Style of conversation (e.g., "Socratic").
        agent_context: Additional context about the agent.

    Returns:
        tuple[str, agentState]: A tuple containing:
            - The content of the last message in the conversation.
            - The final state after running the workflow.

    Raises:
        RuntimeError: If there's an error running the conversation workflow.
    """

    graph_builder = create_workflow_graph()

    try:
        async with AsyncMongoDBSaver.from_conn_string(
            conn_string=settings.MONGO_URI,
            db_name=settings.MONGO_DB_NAME,
            checkpoint_collection_name=settings.MONGO_STATE_CHECKPOINT_COLLECTION,
            writes_collection_name=settings.MONGO_STATE_WRITES_COLLECTION,
        ) as checkpointer:            
            graph = graph_builder.compile(checkpointer=checkpointer)
            opik_tracer = OpikTracer(graph=graph.get_graph(xray=True))

            thread_id = (
                agent_id if not new_thread else f"{agent_id}-{uuid.uuid4()}"
            )
            config = {
                "configurable": {"thread_id": thread_id},
                "callbacks": [opik_tracer],
            }
            output_state = await graph.ainvoke(
                input={
                    "messages": __format_messages(messages=messages),
                    "agent_name": agent_name,
                    "agent_perspective": agent_perspective,
                    "agent_style": agent_style,
                    "agent_context": agent_context,
                },
                config=config,
            )
        last_message = output_state["messages"][-1]
        return last_message.content, AgentState(**output_state)
    except Exception as e:
        raise RuntimeError(f"Error running conversation workflow: {str(e)}") from e
    
async def get_streaming_response(
    messages: str | list[str] | list[dict[str, Any]],
    agent_id: str,
    agent_name: str,
    agent_perspective: str,
    agent_style: str,
    agent_context: str,
    new_thread: bool = False,
) -> AsyncGenerator[str, None]:
    """Run a conversation through the workflow graph with streaming response.

    Args:
        messages: Initial message to start the conversation.
        agent_id: Unique identifier for the agent.
        agent_name: Name of the agent.
        agent_perspective: agent's perspective on the topic.
        agent_style: Style of conversation (e.g., "Socratic").
        agent_context: Additional context about the agent.
        new_thread: Whether to create a new conversation thread.

    Yields:
        Chunks of the response as they become available.

    Raises:
        RuntimeError: If there's an error running the conversation workflow.
    """
    graph_builder = create_workflow_graph()

    try:
        async with AsyncMongoDBSaver.from_conn_string(
            conn_string=settings.MONGO_URI,
            db_name=settings.MONGO_DB_NAME,
            checkpoint_collection_name=settings.MONGO_STATE_CHECKPOINT_COLLECTION,
            writes_collection_name=settings.MONGO_STATE_WRITES_COLLECTION,
        ) as checkpointer:
            # Set up the database tables if they don't exist            
            graph = graph_builder.compile(checkpointer=checkpointer)
            opik_tracer = OpikTracer(graph=graph.get_graph(xray=True))

            thread_id = (
                agent_id if not new_thread else f"{agent_id}-{uuid.uuid4()}"
            )
            config = {
                "configurable": {"thread_id": thread_id},
                "callbacks": [opik_tracer],
            }

            async for chunk in graph.astream(
                input={
                    "messages": __format_messages(messages=messages),
                    "agent_name": agent_name,
                    "agent_perspective": agent_perspective,
                    "agent_style": agent_style,
                    "agent_context": agent_context,
                },
                config=config,
                stream_mode="messages",
            ):
                if chunk[1]["langgraph_node"] == "conversation_node" and isinstance(
                    chunk[0], AIMessageChunk
                ):
                    yield chunk[0].content

    except Exception as e:
        raise RuntimeError(
            f"Error running streaming conversation workflow: {str(e)}"
        ) from e


def __format_messages(
    messages: Union[str, list[dict[str, Any]]],
) -> list[Union[HumanMessage, AIMessage]]:
    """Convert various message formats to a list of LangChain message objects.

    Args:
        messages: Can be one of:
            - A single string message
            - A list of string messages
            - A list of dictionaries with 'role' and 'content' keys

    Returns:
        List[Union[HumanMessage, AIMessage]]: A list of LangChain message objects
    """

    if isinstance(messages, str):
        return [HumanMessage(content=messages)]

    if isinstance(messages, list):
        if not messages:

            return []

        if (
            isinstance(messages[0], dict)
            and "role" in messages[0]
            and "content" in messages[0]
        ):
            result = []
            for msg in messages:
                if msg["role"] == "user":
                    result.append(HumanMessage(content=msg["content"]))
                elif msg["role"] == "assistant":
                    result.append(AIMessage(content=msg["content"]))
            return result

        return [HumanMessage(content=message) for message in messages]

    return []

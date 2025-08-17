from fastapi import APIRouter
from pydantic import BaseModel
from opik.integrations.langchain import OpikTracer
from fastapi import WebSocket, WebSocketDisconnect, HTTPException
from src.domain.agent_factory import AgentsFactory
from src.application.conversation_service.generate_response import get_response
from src.application.conversation_service.generate_response import get_streaming_response

router = APIRouter()

class ChatMessage(BaseModel):
    message: str
    agent_id: str

@router.post("/chat")
async def chat(chat_message: ChatMessage):
    try:
        agent_factory = AgentsFactory()
        agent = agent_factory.get_agent(chat_message.agent_id)

        response, _ = await get_response(
            messages=chat_message.message,
            agent_id=chat_message.agent_id,
            agent_name=agent.name,
            agent_perspective=agent.perspective,
            agent_style=agent.style,
            agent_context="",
        )
        return {"response": response}
    except Exception as e:
        opik_tracer = OpikTracer()
        opik_tracer.flush()

        raise HTTPException(status_code=500, detail=str(e))


@router.websocket("/ws/chat")
async def websocket_chat(websocket: WebSocket):
    await websocket.accept()

    try:
        while True:
            data = await websocket.receive_json()

            if "message" not in data or "agent_id" not in data:
                await websocket.send_json(
                    {
                        "error": "Invalid message format. Required fields: 'message' and 'agent_id'"
                    }
                )
                continue

            try:
                agent_factory = AgentsFactory()
                agent = agent_factory.get_agent(
                    data["agent_id"]
                )

                # Use streaming response instead of get_response
                response_stream = get_streaming_response(
                    messages=data["message"],
                    agent_id=data["agent_id"],
                    agent_name=agent.name,
                    agent_perspective=agent.perspective,
                    agent_style=agent.style,
                    agent_context="",
                )

                # Send initial message to indicate streaming has started
                await websocket.send_json({"streaming": True})

                # Stream each chunk of the response
                full_response = ""
                async for chunk in response_stream:
                    full_response += chunk
                    await websocket.send_json({"chunk": chunk})

                await websocket.send_json(
                    {"response": full_response, "streaming": False}
                )

            except Exception as e:
                opik_tracer = OpikTracer()
                opik_tracer.flush()

                await websocket.send_json({"error": str(e)})

    except WebSocketDisconnect:
        pass


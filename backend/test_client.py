"""
Client to test the websocket connection to the API
"""

from fastapi import FastAPI
from fastapi.testclient import TestClient
from fastapi.websockets import WebSocket
from src.infrastructure.api import app

def test_websocket():
    client = TestClient(app)
    with client.websocket_connect("/ws/chat") as websocket:
        # Send message to the WebSocket
        websocket.send_json({"message": "Hello, world!", "agent_id": "socrates"})
        
        # Receive the streaming indicator
        streaming_start = websocket.receive_json()
        print(f"Streaming started: {streaming_start}")
        
        # Collect all chunks
        chunks = []
        while True:
            response = websocket.receive_json()
            print(f"Received: {response}")
            
            if "chunk" in response:
                chunks.append(response["chunk"])
            elif "response" in response and not response.get("streaming", True):
                # Final response received
                print(f"Final response: {response['response']}")
                break
            elif "error" in response:
                print(f"Error: {response['error']}")
                break


if __name__ == "__main__":
    test_websocket()
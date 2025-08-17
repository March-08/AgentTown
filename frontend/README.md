# 🤖 Adaptive Agents: Multi-Agent AI Innovation Playground

A Unity 3D game featuring a dynamic multi-agent AI system where players can interact with AI-powered representations of famous innovators like Elon Musk, Steve Jobs, Larry Page, and others. Each agent is connected to a Retrieval-Augmented Generation (RAG) system, enabling contextually aware and intelligent conversations.

## 🎯 Project Overview

**Adaptive Agents** is an interactive simulation that brings together the greatest minds in technology and innovation through AI-powered NPCs. Players can walk around a virtual environment, discover and chat with these digital innovators, each equipped with their own AI agent that can draw from relevant knowledge bases to provide authentic, insightful responses.

## 🎬 Demo Video

<p align="center">
  <a href="https://youtu.be/D9e7rTNWWRc" target="_blank">
    <img src="https://img.youtube.com/vi/D9e7rTNWWRc/maxresdefault.jpg" alt="Watch the demo" width="900">
  </a>
</p>


*Watch the gameplay demo to see the multi-agent AI system in action! See how players interact with AI-powered innovators like Elon Musk, Steve Jobs, and Larry Page in real-time conversations.*

### 🌟 Key Features

- **Multi-Agent AI System**: Each innovator NPC is powered by its own AI agent with unique personality and knowledge
- **RAG Integration**: Real-time connection to Retrieval-Augmented Generation systems for contextually rich conversations
- **Interactive 3D Environment**: Fully navigable 3D world with modern city environments
- **Dynamic Chat System**: Seamless WebSocket-based communication with AI backend
- **Intelligent NPCs**: Distance-based interaction, highlighting, and smart conversation management
- **Fallback Systems**: Graceful degradation when AI services are unavailable

## 🚀 Technology Stack

- **Game Engine**: Unity 2022.3+ (Universal Render Pipeline)
- **Networking**: WebSocket (System.Net.WebSockets)
- **AI Backend**: Python-based server with FastAPI and WebSocket support
- **3D Assets**: Polygon art style with modern city environments
- **Character Animation**: Humanoid character controller with NavMesh pathfinding

## 🎮 Gameplay Features

### Player Interaction
- **WASD/Click Movement**: Navigate the 3D environment using standard FPS controls or point-and-click
- **NPC Discovery**: NPCs highlight when you're within interaction range
- **Smart Chat System**: Click on highlighted NPCs to start conversations
- **Distance Management**: Conversations auto-close if you walk too far away

### AI Conversations
- **Real-time Streaming**: AI responses stream in real-time for natural conversation flow
- **Agent Personalities**: Each innovator has unique conversation patterns and knowledge
- **RAG-Enhanced Responses**: Agents can access relevant information to provide detailed, accurate answers
- **Conversation Memory**: Chat history is maintained during active sessions

## 🛠️ Architecture

### Core Components

#### `NPCAgent.cs`
- Manages individual NPC behavior and properties
- Handles highlighting, interaction radius, and basic dialogue
- Automatically sets up name displays and visual feedback

#### `ChatManager.cs`
- Central hub for all chat interactions
- Manages UI state, message history, and AI integration
- Handles both streaming and fallback response systems

#### `WebSocketChatClient.cs`
- Real-time communication with AI backend
- Manages connection stability, reconnection logic
- Processes streaming responses and error handling

#### `Mover.cs`
- Player movement and interaction controller
- Handles NPC detection, highlighting, and click interactions
- Integrates with Unity's NavMesh system

### AI Backend Integration

The game connects to a WebSocket server (typically running on `localhost:8000`) that provides:
- **Agent Routing**: Messages are routed to specific AI agents based on NPC identity
- **RAG Processing**: Retrieval-augmented generation for contextually relevant responses
- **Streaming Responses**: Real-time text streaming for natural conversation flow
- **Error Handling**: Graceful fallbacks and connection management

## 🔧 Setup Instructions

### Prerequisites
- Unity 2022.3 LTS or newer
- Python 3.8+ (for AI backend)
- WebSocket-compatible AI server

### Unity Setup
1. Clone or download the project
2. Open in Unity Hub
3. Ensure all packages are imported (TextMeshPro, etc.)
4. Open the `Sandbox` scene in `Assets/Scenes/`

### AI Backend Setup
1. Set up your AI backend server with WebSocket support
2. Configure the server URL in `WebSocketChatClient` (default: `ws://localhost:8000/ws/chat`)
3. Ensure your backend supports the message format:
   ```json
   {
     "message": "user message",
     "agent_id": "agent_name"
   }
   ```

### Running the Game
1. Start your AI backend server
2. Play the scene in Unity
3. Use WASD or click to move around
4. Click on highlighted NPCs to start conversations

## 🎨 Customization

### Adding New Innovators
1. Create a new NPC GameObject with appropriate 3D model
2. Add the `NPCAgent` component
3. Configure the agent name and dialogue options
4. Ensure your AI backend has corresponding agent logic

### Modifying Chat Behavior
- Edit `ChatManager.cs` for UI behavior and conversation flow
- Modify `WebSocketChatClient.cs` for backend communication
- Customize `NPCAgent.cs` for individual NPC behavior

### Environment Design
- Use the included Polygon city assets to create new environments
- Modify terrain and lighting in the Sandbox scene
- Add new interaction objects and NPCs as needed

## 🔍 Debugging & Troubleshooting

### Common Issues

**WebSocket Connection Fails**
- Check if AI backend is running on correct port
- Verify firewall settings allow localhost connections
- Review Unity console for detailed connection logs

**NPCs Not Responding**
- Ensure `ChatManager` is properly set up in the scene
- Check WebSocket connection status
- Verify AI backend is processing requests correctly

**UI Not Appearing**
- Check EventSystem exists in scene
- Verify ChatUI components are properly assigned
- Look for UI canvas scaling issues

### Debug Features
- Use Context Menu options on WebSocket client for connection testing
- Enable detailed logging in `WebSocketChatClient`
- Monitor Unity console for real-time connection status

## 🤝 Contributing

This project welcomes contributions! Areas for improvement:
- Additional innovator personalities and agents
- Enhanced RAG integration and knowledge bases
- Improved 3D environments and visual assets
- Performance optimizations and mobile support
- Multiplayer functionality for shared experiences

## 📚 Documentation

### Key Scripts Documentation
- `/Assets/Scripts/` - Core game logic and AI integration
- Agent setup instructions included in script headers
- WebSocket protocol documentation in client comments

### Asset Attribution
- Polygon city assets from Synty Studios
- Character models from various asset packs (see asset folders for details)
- Audio and animation assets included with proper licensing

## 🎯 Future Roadmap

- **Enhanced AI Personalities**: More detailed agent characteristics and conversation patterns
- **Expanded Knowledge Bases**: Specialized RAG systems for each innovator
- **Multiplayer Support**: Shared sessions with multiple players
- **Mobile Platform**: iOS and Android deployment
- **VR Integration**: Virtual reality interaction support
- **Educational Features**: Guided learning experiences and historical contexts


---

**Built with ❤️ for exploring the intersection of AI, gaming, and innovation**

*For technical support or feature requests, please check the Unity console logs and WebSocket connection status first.*

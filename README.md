# 🏙️ AgentTown: Where Innovation Legends Come Alive

> **Step into a world where you can have conversations with history's greatest technology innovators**

AgentTown is a multi-agent AI simulation that brings the minds of legendary innovators into an interactive 3D environment. Walk through a virtual city, discover AI-powered NPCs, and engage in meaningful conversations with digital representations of visionaries like Elon Musk, Steve Jobs, Tim Berners-Lee, and other tech pioneers.

## 🎬 AgentTown Demo

<p align="center">
  <a href="https://youtu.be/D9e7rTNWWRc" target="_blank">
    <img src="https://github.com/March-08/AgentTown/blob/main/video.gif" alt="Watch AgentTown Demo" width="800">
  </a>
</p>

*Watch real conversations with AI-powered innovators in an immersive 3D environment*

## 🌟 What Makes AgentTown Special

### 🎭 **Meet the Legends**
Engage with carefully crafted AI agents embodying the personalities and wisdom of:

| Innovator | Known For | Conversation Style |
|-----------|-----------|-------------------|
| **Elon Musk** | First-principles thinking, space exploration | Direct, ambitious, future-focused |
| **Steve Jobs** | Design perfection, user experience | Passionate, minimalist, artistic |
| **Tim Berners-Lee** | World Wide Web, digital rights | Thoughtful, principled, democratic |
| **Bill Gates** | Software revolution, global impact | Analytical, optimistic, systematic |
| **Larry Page** | Search algorithms, moonshot projects | Data-driven, quietly confident |
| **Jack Dorsey** | Social media, financial inclusion | Contemplative, minimalist, socially conscious |
| **Jeff Bezos** | E-commerce, customer obsession | Methodical, long-term thinking |
| **Sergey Brin** | Search innovation, algorithmic thinking | Intellectually playful, technically precise |
| **Mark Zuckerberg** | Social connection, virtual experiences | Earnest, platform-thinking oriented |
| **Satya Nadella** | Cloud computing, inclusive technology | Collaborative, empathetic, transformation-focused |

### 🧠 **Intelligent Conversations**
- **Authentic Personalities**: Each agent responds with their unique perspective and communication style
- **Context-Aware Memory**: Conversations maintain context and build naturally over time
- **Knowledge Integration**: RAG-powered responses drawing from extensive knowledge bases
- **Real-Time Streaming**: Natural conversation flow with live response generation

### 🎮 **Immersive 3D Experience**
- **Interactive Environment**: Navigate a beautiful virtual city with modern architecture
- **Dynamic Discovery**: NPCs highlight when you're within conversation range
- **Seamless Integration**: Chat UI overlays smoothly integrate with 3D gameplay
- **Smart Interactions**: Distance-based conversation management and auto-discovery

## 🚀 Quick Start

### 🔧 **Backend Setup (AI Agents)**
```bash
# Navigate to backend
cd backend

# Install dependencies
make install

# Set up environment variables
# Create .env file with your API keys:
GROQ_API_KEY=your_groq_api_key_here
OPENAI_API_KEY=your_openai_api_key_here  # For evaluation features

# Optional: Use Qdrant Cloud instead of local (recommended for production)
QDRANT_API_KEY=your_qdrant_cloud_api_key
QDRANT_URL=https://your-cluster-url.qdrant.tech

# Start infrastructure and FastAPI server
make infra-up
```

**🚀 This will start:**
- MongoDB (conversations storage)
- Qdrant Vector Database (local, unless cloud credentials provided)
- FastAPI server with WebSocket support at `http://localhost:8000`
- Interactive API documentation at `http://localhost:8000/docs`

### 🎮 **Frontend Setup (Unity Game)**
```bash
# Prerequisites: Unity 2022.3+ LTS
# 1. Open the project in Unity Hub
# 2. Load the Sandbox scene from Assets/Scenes/
# 3. Ensure the backend is running (default: localhost:8000)
# 4. Press Play and start exploring!
```

## 🏗️ **Architecture**

```
AgentTown/
├── 🧠 backend/          # AI Agent System
│   ├── Multi-agent conversation engine
│   ├── RAG knowledge integration  
│   ├── WebSocket API server
│   └── Persistent memory system
│
└── 🎮 frontend/         # Unity 3D Game
    ├── Interactive 3D environment
    ├── NPC agent management
    ├── Real-time chat system
    └── Player movement & discovery
```

## 💬 **How It Works**

1. **🚶 Explore**: Walk around the virtual city using WASD or click-to-move
2. **✨ Discover**: NPCs glow when you're close enough to interact
3. **💭 Converse**: Click on any highlighted innovator to start a conversation
4. **🧠 Learn**: Ask questions and get authentic responses powered by advanced AI
5. **🔄 Continue**: Conversations maintain context and memory across sessions

## 🛠️ **Technology Powering the Magic**

### Backend AI Engine
- **🤖 AI/ML**: LangChain, LangGraph, OpenAI, Groq
- **🗃️ Data**: MongoDB, Qdrant Vector Database
- **🌐 API**: FastAPI with WebSocket streaming
- **🔍 RAG**: Wikipedia integration, semantic search

### Frontend Experience
- **🎮 Engine**: Unity 2022.3+ Universal Render Pipeline
- **🌐 Networking**: WebSocket real-time communication
- **🎨 Assets**: Polygon art style, modern city environments
- **🚶 Movement**: NavMesh pathfinding, intelligent NPC interactions

## 🙏 **Acknowledgments**

This project was heavily inspired by and builds upon the excellent work from:

- **[PhiloAgents Course](https://github.com/neural-maze/philoagents-course)** by The Neural Maze and Decoding ML: The foundational architecture, agent design patterns, and RAG implementation approach were adapted from this comprehensive open-source course on building AI-powered agent simulation engines. Special thanks to **Paul Iusztin** and **Miguel Otero Pedrido** for their exceptional educational content.


> **Note**: While this project transforms the original philosopher agents into tech innovator personalities and adds a 3D interactive environment, the core architectural patterns and implementation strategies were learned from the PhiloAgents Course. I highly recommend checking out their [comprehensive tutorial series](https://decodingml.substack.com/p/from-0-to-pro-ai-agents-roadmap) for anyone interested in building production-ready AI agent systems.

---

> **"The best way to predict the future is to invent it."** - Alan Kay

**AgentTown: Where legendary minds meet cutting-edge AI** 🚀

*Built with curiosity, powered by AI, inspired by the greatest innovators in history.*

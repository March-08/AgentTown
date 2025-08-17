# 🤖 Adaptive Agents: Conversational AI with Innovator Personalities

> **Personal Note**: This project represents my deep dive into the fascinating world of AI agents and conversational AI systems. What started as curiosity about how we could make AI conversations more engaging and personality-driven evolved into a comprehensive system that brings some of history's greatest innovators to life through AI. I'm particularly proud of the modular architecture and the seamless integration of multiple AI technologies that make this possible.

## 🎯 Overview

**Adaptive Agents** is a sophisticated conversational AI system that embodies the personalities, perspectives, and communication styles of renowned technology innovators. Rather than generic chatbots, users can engage in meaningful conversations with AI representations of visionaries like Elon Musk, Steve Jobs, Tim Berners-Lee, and others.

### 🌟 What Makes This Special

This isn't just another AI chatbot. Each agent has been carefully crafted with:
- **Authentic Personalities**: Detailed communication styles and philosophical perspectives
- **Dynamic Memory**: Persistent conversation history and context awareness
- **Knowledge Integration**: RAG-powered access to relevant information about each innovator
- **Adaptive Responses**: Context-aware conversations that evolve naturally

## 🚀 Featured Innovator Agents

| Agent | Personality Focus | Communication Style |
|-------|------------------|-------------------|
| **Elon Musk** | First-principles thinking, humanity's future | Direct, ambitious, occasionally provocative |
| **Steve Jobs** | Design perfection, user experience | Passionate, minimalist, intersection of technology and liberal arts |
| **Tim Berners-Lee** | Open systems, digital rights | Thoughtful, principled, democratically minded |
| **Bill Gates** | Global impact, systematic problem-solving | Analytical, optimistic, philanthropically focused |
| **Larry Page** | Information organization, moonshot thinking | Quietly confident, data-driven, future-focused |
| **Jack Dorsey** | Communication, financial inclusion | Contemplative, minimalist, socially conscious |
| **Jeff Bezos** | Customer obsession, long-term thinking | Methodical, customer-centric, improvement-focused |
| **Sergey Brin** | Scientific curiosity, algorithmic innovation | Intellectually playful, technically precise |
| **Mark Zuckerberg** | Connection, virtual experiences | Earnest, platform-thinking oriented |
| **Satya Nadella** | Empowerment, inclusive technology | Collaborative, empathetic, transformation-focused |

## 🏗️ System Architecture

### Core Components

```
├── 🎭 Domain Layer (Agent Personalities & Business Logic)
├── 🧠 Application Layer (Conversation Management & RAG)
├── 🔧 Infrastructure Layer (APIs, Memory, External Services)
├── 📊 Data Layer (Vector Storage, Persistence)
└── 🛠️ Tools (Memory Management, Evaluation)
```

### Technology Stack

- **🤖 AI/ML**: LangChain, LangGraph, Sentence Transformers, OpenAI, Groq
- **🗃️ Data Storage**: MongoDB (conversations), Qdrant (vector embeddings)
- **🌐 Backend**: FastAPI, WebSockets, Uvicorn
- **📡 External APIs**: Wikipedia integration, multiple LLM providers
- **🔍 Monitoring**: Opik (Comet ML) for conversation tracking
- **🐳 Deployment**: Docker, Docker Compose
- **📦 Dependencies**: UV package manager, Python 3.10+

## ✨ Key Features

### 🗣️ **Intelligent Conversation Management**
- **Persistent Memory**: Conversations maintain context across sessions
- **Dynamic Summarization**: Automatic conversation summarization for long interactions
- **State Management**: Advanced workflow orchestration with LangGraph

### 🧠 **RAG-Powered Knowledge Integration**
- **Wikipedia Integration**: Automatic extraction of relevant innovator information
- **Vector Search**: Semantic similarity matching for contextual responses
- **Document Processing**: Intelligent chunking and embedding generation

### 🎯 **Multi-Interface Access**
- **CLI Interface**: Interactive terminal-based conversations
- **REST API**: HTTP endpoints for integration
- **WebSocket Support**: Real-time streaming conversations
- **Web Dashboard**: (Future enhancement)

### 📊 **Evaluation & Analytics**
- **Conversation Metrics**: Track engagement and response quality
- **Agent Performance**: Evaluate personality consistency
- **System Monitoring**: Comprehensive logging and tracing

## 🚀 Quick Start

### Prerequisites
- Python 3.10+
- Docker & Docker Compose
- UV package manager

### Setup & Run
```bash
# 1. Clone the repository
git clone https://github.com/yourusername/adaptive-agents.git
cd adaptive-agents

# 2. Install dependencies
make install

# 3. Set up your environment variables
# Create .env file with your API keys:
GROQ_API_KEY=your_groq_api_key_here
OPENAI_API_KEY=your_openai_api_key_here  # For evaluation

# Optional: Use Qdrant Cloud instead of local
QDRANT_API_KEY=your_qdrant_cloud_api_key  # For cloud deployment
QDRANT_URL=https://your-cluster-url.qdrant.tech

# 4. Start infrastructure and API
make infra-up
```

That's it! 🎉 Your FastAPI server will be running at `http://localhost:8000`

**API Documentation**: Visit `http://localhost:8000/docs` for interactive API documentation

## 💬 API Usage

### Available Endpoints

**POST `/chat`** - Chat with an agent
```python
import requests

response = requests.post("http://localhost:8000/chat", json={
    "message": "How do you approach innovation?",
    "agent_id": "steve_jobs"
})

print(response.json()["response"])
# Steve Jobs: Innovation isn't about saying yes to a thousand things. It's about 
# saying no to all but the essential ones. We focus obsessively on simplicity...
```

**GET `/agents`** - List available agents
```python
agents = requests.get("http://localhost:8000/agents").json()
# Returns: ["elon_musk", "steve_jobs", "tim_berners_lee", ...]
```

### WebSocket Integration
```javascript
const ws = new WebSocket('ws://localhost:8000/chat/steve_jobs');

ws.onmessage = function(event) {
    const data = JSON.parse(event.data);
    console.log('Steve Jobs:', data.response);
};

ws.send(JSON.stringify({
    message: "What makes a product truly revolutionary?"
}));
```

## 🔧 Configuration Options

### Qdrant Setup
**Local (Default)**: Infrastructure automatically starts with `make infra-up`

**Cloud (Recommended for Production)**:
```bash
# Add to your .env file:
QDRANT_API_KEY=your_qdrant_cloud_api_key
QDRANT_URL=https://your-cluster-url.qdrant.tech
```

### Available Commands
```bash
make infra-up             # Start all infrastructure (MongoDB + Qdrant + API)
make infra-down           # Stop infrastructure  
make reset-conversations # Clear conversation history
make create-long-term-memory  # Initialize agent knowledge base
```

> **Note**: The system automatically creates agent knowledge from Wikipedia on first run. For production, consider using Qdrant Cloud for better performance and reliability.

## 📊 Performance & Monitoring

The system includes comprehensive monitoring through:
- **Opik Integration**: Conversation tracking and analysis
- **LangGraph Checkpoints**: State persistence and recovery
- **Custom Metrics**: Response quality and engagement tracking

## 🛠️ Development

### Project Structure
```
src/
├── domain/          # Business logic, agent definitions
├── application/     # Use cases, conversation services
├── infrastructure/  # External interfaces, APIs
└── data/           # Data processing, extraction

tools/              # Utility scripts
data/              # Training data, evaluation sets
```

### Contributing Guidelines
1. Follow existing code patterns and type hints
2. Add tests for new features
3. Update documentation for API changes
4. Ensure all agents maintain personality consistency

### Testing
```bash
# Run tests (when available)
make test

# Manual testing with different agents
python main.py --agent elon_musk --debug
```

## 🚧 Roadmap

### Near-term Enhancements
- [ ] **Web Interface**: React-based chat dashboard
- [ ] **Voice Integration**: Speech-to-text and text-to-speech
- [ ] **Multi-modal**: Image and document understanding
- [ ] **Agent Interactions**: Conversations between agents

### Long-term Vision
- [ ] **Custom Training**: Fine-tuned models for each agent
- [ ] **Real-time Learning**: Adaptive personalities based on interactions
- [ ] **Enterprise Features**: Team collaboration, knowledge management
- [ ] **Mobile App**: iOS/Android native applications

## 🤝 Personal Reflection

Working on this project has been an incredible journey into the cutting edge of conversational AI. I've particularly enjoyed:

1. **The Psychology of AI Personalities**: Crafting distinct voices that feel authentic yet respectful to these real innovators
2. **System Architecture**: Building a scalable, modular system that can handle complex conversational workflows
3. **RAG Implementation**: Integrating external knowledge seamlessly into conversations
4. **Real-world Application**: Creating something that people can actually use and learn from

The biggest technical challenge was balancing personality consistency with conversational flexibility - ensuring each agent feels authentic while remaining helpful and engaging.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

This project was heavily inspired by and builds upon the excellent work from:

- **[PhiloAgents Course](https://github.com/neural-maze/philoagents-course)** by The Neural Maze and Decoding ML: The foundational architecture, agent design patterns, and RAG implementation approach were adapted from this comprehensive open-source course on building AI-powered agent simulation engines. Special thanks to **Paul Iusztin** and **Miguel Otero Pedrido** for their exceptional educational content.

- **LangChain Team**: For the incredible framework that powers our conversational AI
- **OpenAI & Groq**: For providing the foundational language models  
- **MongoDB, Opik, and Qdrant**: For the robust infrastructure technologies
- **The Innovators**: For inspiring us with their vision and contributions to technology

> **Note**: While this project transforms the original philosopher agents into tech innovator personalities and adds custom features, the core architectural patterns and implementation strategies were learned from the PhiloAgents Course. I highly recommend checking out their [comprehensive tutorial series](https://decodingml.substack.com/p/from-0-to-pro-ai-agents-roadmap) for anyone interested in building production-ready AI agent systems.

---

> **"The best way to predict the future is to invent it."** - Alan Kay

*Built with curiosity, powered by AI, inspired by innovation.* 🚀
.PHONY: help install run run-main dev clean mongo-up mongo-down mongo-logs test reset-conversations

# Default target
help:
	@echo "Available commands:"
	@echo "  install     - Install dependencies using uv"
	@echo "  run         - Run FastAPI server with uvicorn"
	@echo "  run-main    - Run the main.py chatbot interface"
	@echo "  dev         - Run FastAPI server in development mode"
	@echo "  mongo-up    - Start MongoDB container"
	@echo "  mongo-down  - Stop MongoDB container"
	@echo "  mongo-logs  - View MongoDB container logs"
	@echo "  clean       - Stop all containers and clean up"
	@echo "  reset-conversations - Reset all conversation state in MongoDB"
	@echo "  test        - Run tests (if any)"

# Install dependencies
install:
	uv sync

# Run FastAPI server
run:
	uvicorn src.infrastructure.api:app --host 0.0.0.0 --port 8000

# Run FastAPI server in development mode with auto-reload
dev:
	uvicorn src.infrastructure.api:app --reload --host 0.0.0.0 --port 8000

# Run the main.py chatbot interface
run-main:
	python main.py

infra-up:
	docker-compose up -d

infra-down:
	docker-compose down

api-up:
	uv run uvicorn src.infrastructure.api:app --host 0.0.0.0 --port 8000

api-down:
	uv run uvicorn src.infrastructure.api:app --host 0.0.0.0 --port 8000

# Start MongoDB container
mongo-up:
	docker-compose up -d

# Stop MongoDB container
mongo-down:
	docker-compose down

# View MongoDB logs
mongo-logs:
	docker-compose logs -f db-mongo

qdrant-up:
	docker-compose up -d qdrant

# Clean up containers
clean:
	docker-compose down
	docker system prune -f

# Reset all conversation state in MongoDB
reset-conversations:
	python src/application/conversation_service/reset_conversation.py

# Run tests (placeholder)
test:
	@echo "No tests configured yet"
	# python -m pytest tests/ -v 

# todo create-long-term-memory
create-long-term-memory:
	python -m tools.create_long_term_memory



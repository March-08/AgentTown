from opik.integrations.langchain import OpikTracer
from contextlib import asynccontextmanager
from pydantic import BaseModel
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from .chat import router as chat_router
from .memory import router as memory_router
from fastapi import WebSocket
import os
from dotenv import load_dotenv

from .opik_utils import configure

configure()

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Do things before app starts e.g Load the ML model
    print("Starting up...")
    #relod env vars with reload argument
    load_dotenv(verbose=True, override=True)
    print("COMET_API_KEY: ", os.getenv('COMET_API_KEY'))
    yield
    # Do things after app stops e.g Clean up the ML models and release the resources
    print("Shutting down...")
    optik_tracer = OpikTracer()
    optik_tracer.shutdown()


app = FastAPI(lifespan=lifespan)


# include routers
app.include_router(chat_router)
app.include_router(memory_router)


app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    return {"message": "Hello World"}


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8000)





 
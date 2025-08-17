import json
from pathlib import Path
from typing import List

from pydantic import BaseModel, Field


class AdaptiveAgentExtract(BaseModel):
    """A class representing raw agent data extracted from external sources.

    This class follows the structure of the agents.json file and contains
    basic information about agents before enrichment.

    Args:
        id (str): Unique identifier for the agent.
        urls (List[str]): List of URLs with information about the agent.
    """

    id: str = Field(description="Unique identifier for the agent")
    urls: List[str] = Field(
        description="List of URLs with information about the agent"
    )

    @classmethod
    def from_json(cls, metadata_file: Path) -> list["AdaptiveAgentExtract"]:
        with open(metadata_file, "r") as f:
            agents_data = json.load(f)

        return [cls(**agent) for agent in agents_data]


class AdaptiveAgent(BaseModel):
    """A class representing a agent agent with memory capabilities.

    Args:
        id (str): Unique identifier for the agent.
        name (str): Name of the agent.
        perspective (str): Description of the agent's theoretical views
            about AI.
        style (str): Description of the agent's talking style.
    """

    id: str = Field(description="Unique identifier for the agent")
    name: str = Field(description="Name of the agent")
    perspective: str = Field(
        description="Description of the agent's theoretical views about AI"
    )
    style: str = Field(description="Description of the agent's talking style")

    def __str__(self) -> str:
        return f"agent(id={self.id}, name={self.name}, perspective={self.perspective}, style={self.style})"

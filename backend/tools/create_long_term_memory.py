
from pathlib import Path

import click

from src.long_term_memory import LongTermMemoryCreator
from src.config import settings
from src.domain.adaptive_agent import AdaptiveAgentExtract

@click.command()
@click.option(
    "--metadata-file",
    type=click.Path(exists=True, path_type=Path),
    default=settings.EXTRACTION_METADATA_FILE_PATH,
    help="Path to the innovators extraction metadata JSON file.",
)
def main(metadata_file: Path) -> None:
    """CLI command to create long-term memory for innovators.

    Args:
        metadata_file: Path to the innovators extraction metadata JSON file.
    """
    agents = AdaptiveAgentExtract.from_json(metadata_file)

    long_term_memory_creator = LongTermMemoryCreator.build_from_settings()
    long_term_memory_creator(agents)


if __name__ == "__main__":
    main()
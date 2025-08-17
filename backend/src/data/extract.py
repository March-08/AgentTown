from typing import Generator

from langchain_community.document_loaders import  WikipediaLoader
from langchain_core.documents import Document
from tqdm import tqdm

from src.domain.adaptive_agent import AdaptiveAgentExtract, AdaptiveAgent
from src.domain.agent_factory import AgentsFactory


def get_extraction_generator(
    agents: list[AdaptiveAgentExtract],
) -> Generator[tuple[AdaptiveAgent, list[Document]], None, None]:
    """Extract documents for a list of innovators, yielding one at a time.

    Args:
        agents: A list of AdaptiveAgentExtract objects containing innovator information.

    Yields:
        tuple[AdaptiveAgent, list[Document]]: A tuple containing the innovator object and a list of
            documents extracted for that innovator.
    """

    progress_bar = tqdm(
        agents,
        desc="Extracting docs",
        unit="agent",
        bar_format="{desc}: {percentage:3.0f}%|{bar}| {n_fmt}/{total_fmt} [{elapsed}<{remaining}, {rate_fmt}] {postfix}",
        ncols=100,
        position=0,
        leave=True,
    )

    agents_factory = AgentsFactory()
    for agent_extract in progress_bar:
        agent = agents_factory.get_agent(agent_extract.id)
        progress_bar.set_postfix_str(f"Agent: {agent.name}")

        agent_docs = extract(agent, agent_extract.urls)

        yield (agent, agent_docs)


def extract(agent: AdaptiveAgent, extract_urls: list[str]) -> list[Document]:
    """Extract documents for a single agent from all sources and deduplicate them.

    Args:
        agent: AdaptiveAgent object containing agent information.
        extract_urls: List of URLs to extract content from.

    Returns:
        list[Document]: List of deduplicated documents extracted for the agent.
    """

    docs = []

    docs.extend(extract_wikipedia(agent))
    #docs.extend(extract_stanford_encyclopedia_of_philosophy(agent, extract_urls))

    return docs


def extract_wikipedia(agent: AdaptiveAgent) -> list[Document]:
    """Extract documents for a single agent from Wikipedia.

    Args:
        agent: AdaptiveAgent object containing agent information.

    Returns:
        list[Document]: List of documents extracted from Wikipedia for the agent.
    """

    loader = WikipediaLoader(
        query=agent.name,
        lang="en",
        load_max_docs=1,
        doc_content_chars_max=1000000,
    )
    docs = loader.load()

    for doc in docs:
        doc.metadata["agent_id"] = agent.id
        doc.metadata["agent_name"] = agent.name

    return docs


# def extract_stanford_encyclopedia_of_philosophy(
#     agent: AdaptiveAgent, urls: list[str]
# ) -> list[Document]:
#     """Extract documents for a single innovator from Stanford Encyclopedia of Philosophy.

#     Args:
#         agent: AdaptiveAgent object containing innovator information.
#         urls: List of URLs to extract content from.

#     Returns:
#         list[Document]: List of documents extracted from Stanford Encyclopedia for the innovator.
#     """

#     def extract_paragraphs_and_headers(soup) -> str:
#         # List of class/id names specific to the Stanford Encyclopedia of Philosophy that we want to exclude.
#         excluded_sections = [
#             "bibliography",
#             "academic-tools",
#             "other-internet-resources",
#             "related-entries",
#             "acknowledgments",
#             "article-copyright",
#             "article-banner",
#             "footer",
#         ]

#         # Find and remove elements within excluded sections
#         for section_name in excluded_sections:
#             for section in soup.find_all(id=section_name):
#                 section.decompose()

#             for section in soup.find_all(class_=section_name):
#                 section.decompose()

#             for section in soup.find_all(
#                 lambda tag: tag.has_attr("id") and section_name in tag["id"].lower()
#             ):
#                 section.decompose()

#             for section in soup.find_all(
#                 lambda tag: tag.has_attr("class")
#                 and any(section_name in cls.lower() for cls in tag["class"])
#             ):
#                 section.decompose()

#         # Extract remaining paragraphs and headers
#         content = []
#         for element in soup.find_all(["p", "h1", "h2", "h3", "h4", "h5", "h6"]):
#             content.append(element.get_text())

#         return "\n\n".join(content)

#     if len(urls) == 0:
#         return []

#     loader = WebBaseLoader(show_progress=False)
#     soups = loader.scrape_all(urls)

#     documents = []
#     for url, soup in zip(urls, soups):
#         text = extract_paragraphs_and_headers(soup)
#         metadata = {
#             "source": url,
#             "agent_id": agent.id,
#             "agent_name": agent.name,
#         }

#         if title := soup.find("title"):
#             metadata["title"] = title.get_text().strip(" \n")

#         documents.append(Document(page_content=text, metadata=metadata))

#     return documents


if __name__ == "__main__":
    aristotle = AgentsFactory().get_agent("aristotle")
    docs = extract_wikipedia(agent=aristotle) 

    print(docs)

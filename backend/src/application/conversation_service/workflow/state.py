from langgraph.graph import MessagesState

class AgentState(MessagesState):

    agent_context: str
    agent_name: str
    agent_perspective: str
    agent_style: str
    # messages field is already properly defined in MessagesState - don't redefine it
    summary: str


def state_to_string(state: AgentState) -> str:
    if "summary" in state and bool(state["summary"]):
        conversation = state["summary"]
    elif "messages" in state and bool(state["messages"]):
        conversation = state["messages"]
    else:
        conversation = ""

    return f"""
agentState(agent_context={state["agent_context"]}, 
agent_name={state["agent_name"]}, 
agent_perspective={state["agent_perspective"]}, 
agent_style={state["agent_style"]}, 
conversation={conversation})
        """
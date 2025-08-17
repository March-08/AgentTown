import webbrowser
import asyncio
from pathlib import Path
from src.application.conversation_service.generate_response import get_response
from src.application.conversation_service.reset_conversation import reset_conversation_state
from src.config import settings
from src.application.conversation_service.workflow.graph import create_workflow_graph
import click

async def check_startup_requirements():
    """
    Check if all requirements are met before starting the chat interface.
    """
    print("🔍 Checking startup requirements...")
    
    # Check configuration
    try:
        print(f"✅ MongoDB URI configured: {settings.MONGO_URI[:50]}...")
        print(f"✅ Database name: {settings.MONGO_DB_NAME}")
        
        # Check if required API keys are configured
        if hasattr(settings, 'GROQ_API_KEY') and settings.GROQ_API_KEY:
            print("✅ GROQ API key configured")
        else:
            print("⚠️  GROQ API key not configured - you may need to set GROQ_API_KEY in your environment")
            
    except Exception as e:
        print(f"❌ Configuration error: {e}")
        return False
        
    print("🎯 All basic requirements checked!")
    return True

def display_graph(graph_obj, filename: str = "workflow_graph", auto_open: bool = False):
    """
    Display a LangGraph workflow graph by saving it as PNG and Mermaid files.
    
    Args:
        graph_obj: The compiled LangGraph graph object
        filename: Base filename for the saved files (without extension)
        auto_open: Whether to automatically open the PNG file
    """
    try:
        # Save as PNG file
        graph_png = graph_obj.get_graph().draw_mermaid_png()
        png_path = f"{filename}.png"
        with open(png_path, "wb") as f:
            f.write(graph_png)
        print(f"✅ Graph saved as '{png_path}'")
        
        # Save as Mermaid text file
        mermaid_code = graph_obj.get_graph().draw_mermaid()
        mmd_path = f"{filename}.mmd"
        with open(mmd_path, "w") as f:
            f.write(mermaid_code)
        print(f"✅ Mermaid code saved as '{mmd_path}'")
        
        # Optionally open the PNG file
        if auto_open:
            webbrowser.open(str(Path(png_path).absolute()))
            print(f"🚀 Opened '{png_path}' in default viewer")
            
        print(f"💡 View online: Copy contents of '{mmd_path}' to https://mermaid.live")
        
    except Exception as e:
        print(f"❌ Error displaying graph: {e}")

async def run_chatbot():
    """
    Run an async chatbot using the generate_response function to test MongoDB database.
    """
    print("🤖 Adaptive Agent Chat Interface")
    print("📊 This will test MongoDB database creation and conversation state management")
    print("💬 Type 'quit', 'exit', or 'q' to end the conversation")
    print("🔄 Type 'reset' to clear conversation history")
    print("=" * 60)
    
    # Agent configuration
    agent_id = "test-agent-002"
    agent_name = "Plato"
    agent_perspective = "I am a helpful AI assistant that adapts to user needs and maintains conversation context."
    agent_style = "Conversational, clear, and thoughtful"
    agent_context = "Testing MongoDB integration and conversation state persistence"
    
    print(f"🎯 Agent: {agent_name}")
    print(f"📝 Style: {agent_style}")
    print("=" * 60)
    
    message_count = 0
    
    while True:
        try:
            user_input = input("\n💬 You: ").strip()
            
            if user_input.lower() in ["quit", "exit", "q"]:
                print("\n👋 Goodbye! Thanks for testing the adaptive agent!")
                break
                
            if user_input.lower() == "reset":
                print("\n🔄 Resetting conversation state...")
                try:
                    result = await reset_conversation_state()
                    print(f"✅ {result['message']}")
                    message_count = 0
                    continue
                except Exception as e:
                    print(f"❌ Error resetting conversation: {e}")
                    continue
            
            if not user_input:
                print("⚠️  Please enter a message.")
                continue
            
            message_count += 1
            print(f"\n🔄 Processing message #{message_count}...")
            
            # Call the generate_response function
            try:
                response_content, agent_state = await get_response(
                    messages=user_input,
                    agent_id=agent_id,
                    agent_name=agent_name,
                    agent_perspective=agent_perspective,
                    agent_style=agent_style,
                    agent_context=agent_context,
                    new_thread=False  # Keep conversation in same thread to test state persistence
                )
                
                print(f"🤖 {agent_name}: {response_content}")
                
                # Show some debug info about the conversation state
                if hasattr(agent_state, 'messages') and agent_state.messages:
                    print(f"📊 Conversation length: {len(agent_state.messages)} messages")
                
            except Exception as e:
                print(f"❌ Error generating response: {e}")
                print("🔍 This might indicate a database connection or configuration issue.")
                
        except KeyboardInterrupt:
            print("\n\n⚠️  Interrupted by user. Goodbye!")
            break
        except Exception as e:
            print(f"❌ Unexpected error: {e}")

def create_and_display_graph():
    """Create and display the workflow graph"""
    graph_builder = create_workflow_graph()
    # We need to compile the graph to display it
    from langgraph.checkpoint.memory import MemorySaver
    graph = graph_builder.compile(checkpointer=MemorySaver())
    display_graph(graph)

@click.command()
@click.option("--display-graph", is_flag=True, help="Display the workflow graph")
def main(display_graph):
    """Main entry point for the adaptive agents application"""
    if display_graph:
        create_and_display_graph()
        return
    
    async def async_main():
        print("🚀 Adaptive Agents - MongoDB Test Interface")
        print()
        
        # Check requirements first
        startup_ok = await check_startup_requirements()
        if not startup_ok:
            print("\n❌ Startup checks failed. Please fix the issues above and try again.")
            return
        
        print("\n" + "=" * 60)
        print("💡 Tips:")
        print("   • The first message may take longer as the database is initialized")
        print("   • Conversation state is persisted across messages")
        print("   • Use 'reset' to clear conversation history and test database operations")
        print("   • Check your MongoDB logs to see database collection creation")
        print("=" * 60)
        
        # Run the async chatbot
        await run_chatbot()
    
    # Run the async function
    asyncio.run(async_main())

if __name__ == "__main__":
    main()

using UnityEngine;

/*
 * AI COMPLETE CHAT SETUP
 * 
 * This script automatically sets up the complete AI chat system including:
 * - WebSocket client for AI communication
 * - Chat UI system
 * - NPC interaction
 * 
 * Simply add this to any GameObject in your scene to get AI-powered conversations!
 * 
 * REQUIREMENTS:
 * - WebSocket server running on localhost:8000
 * - Agent endpoints matching NPC agent names
 */

public class AICompleteChatSetup : MonoBehaviour
{
    [Header("AI Server Settings")]
    [SerializeField] private string serverUrl = "ws://localhost:8000/ws/chat";
    [SerializeField] private bool autoConnect = true;

    [Header("Setup Options")]
    [SerializeField] private bool createChatSystem = true;
    [SerializeField] private bool createWebSocketClient = true;

    void Start()
    {
        SetupAIChatSystem();
    }

    private void SetupAIChatSystem()
    {
        Debug.Log("🤖 Setting up AI Complete Chat System...");

        // 1. Setup WebSocket Client
        if (createWebSocketClient)
        {
            SetupWebSocketClient();
        }

        // 2. Setup Chat System
        if (createChatSystem)
        {
            SetupChatSystem();
        }

        Debug.Log("✅ AI Complete Chat System ready!");
        Debug.Log("📋 Instructions:");
        Debug.Log("   1. Make sure your WebSocket server is running on localhost:8000");
        Debug.Log("   2. Add NPCAgent components to GameObjects you want as NPCs");
        Debug.Log("   3. Ensure NPC agent names match your server's agent_id values");
        Debug.Log("   4. Walk close to NPCs and click them to start AI conversations!");
    }

    private void SetupWebSocketClient()
    {
        WebSocketChatClient existingClient = FindObjectOfType<WebSocketChatClient>();

        if (existingClient == null)
        {
            GameObject wsClientGO = new GameObject("AI_WebSocketClient");
            WebSocketChatClient wsClient = wsClientGO.AddComponent<WebSocketChatClient>();

            // Configure the client
            wsClient.SetServerUrl(serverUrl);

            Debug.Log($"✅ WebSocket client created and configured for {serverUrl}");
        }
        else
        {
            Debug.Log("✅ WebSocket client already exists");
        }
    }

    private void SetupChatSystem()
    {
        ChatManager existingChatManager = FindObjectOfType<ChatManager>();

        if (existingChatManager == null)
        {
            // Use the SimpleChatSetup to create the chat system
            SimpleChatSetup chatSetup = FindObjectOfType<SimpleChatSetup>();

            if (chatSetup == null)
            {
                GameObject chatSetupGO = new GameObject("AI_ChatSetup");
                chatSetup = chatSetupGO.AddComponent<SimpleChatSetup>();
                Debug.Log("✅ Chat system will be created automatically");
            }
        }
        else
        {
            Debug.Log("✅ Chat system already exists");
        }
    }

    [ContextMenu("Test WebSocket Connection")]
    public void TestWebSocketConnection()
    {
        WebSocketChatClient client = FindObjectOfType<WebSocketChatClient>();

        if (client != null)
        {
            Debug.Log($"🔌 WebSocket Status: {client.GetConnectionStatus()}");

            if (!client.IsConnected)
            {
                Debug.Log("🔄 Attempting to connect...");
                client.StartConnection();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No WebSocket client found. Run the setup first.");
        }
    }

    [ContextMenu("Show AI Chat Status")]
    public void ShowAIChatStatus()
    {
        WebSocketChatClient client = FindObjectOfType<WebSocketChatClient>();
        ChatManager chatManager = FindObjectOfType<ChatManager>();
        NPCAgent[] agents = FindObjectsOfType<NPCAgent>();

        Debug.Log("=== AI CHAT SYSTEM STATUS ===");
        Debug.Log($"🔌 WebSocket Client: {(client != null ? "Found" : "Missing")}");
        if (client != null)
        {
            Debug.Log($"   Status: {client.GetConnectionStatus()}");
        }

        Debug.Log($"💬 Chat Manager: {(chatManager != null ? "Found" : "Missing")}");
        Debug.Log($"🤖 NPCAgents Found: {agents.Length}");

        foreach (NPCAgent agent in agents)
        {
            Debug.Log($"   - {agent.agentName} (Range: {agent.interactionRange})");
        }

        if (client != null && client.IsConnected && chatManager != null && agents.Length > 0)
        {
            Debug.Log("✅ AI Chat System is fully operational!");
        }
        else
        {
            Debug.Log("⚠️ AI Chat System needs attention. Check the missing components above.");
        }
    }
}

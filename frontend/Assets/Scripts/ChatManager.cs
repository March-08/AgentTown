using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    [SerializeField] private GameObject chatUI;
    [SerializeField] private ChatUI chatUIComponent;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text chatText;
    [SerializeField] private InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject messagePrefab; // For more advanced chat display

    [Header("AI Chat Integration")]
    [SerializeField] private WebSocketChatClient webSocketClient;
    [SerializeField] private bool useAIChat = true;
    [SerializeField] private string fallbackResponse = "I'm having trouble connecting to my AI brain right now. Please try again in a moment!";

    private NPCAgent currentAgent;
    private bool isChatOpen = false;
    private bool isAutoClosing = false; // Prevent multiple auto-close attempts
    private bool isWaitingForAIResponse = false;
    private List<string> chatHistory = new List<string>();
    private StringBuilder streamingResponse = new StringBuilder();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("🚀 ChatManager Start() called");
        SetupUIListeners();
        SetupWebSocketClient();

        Debug.Log($"🔗 ChatManager instance: {GetInstanceID()}");
        Debug.Log($"🔗 WebSocketClient instance: {(webSocketClient != null ? webSocketClient.GetInstanceID().ToString() : "NULL")}");
    }

    public void SetupUIComponents(GameObject chatUIGO, ChatUI chatUIComp, Text npcName, Text chat,
                                 InputField input, Button send, Button close, ScrollRect scroll)
    {
        chatUI = chatUIGO;
        chatUIComponent = chatUIComp;
        npcNameText = npcName;
        chatText = chat;
        playerInputField = input;
        sendButton = send;
        closeButton = close;
        chatScrollRect = scroll;

        SetupUIListeners();

        if (chatUI != null)
            chatUI.SetActive(false);
    }

    private void SetupUIListeners()
    {
        Debug.Log("🔧 Setting up UI listeners...");

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(SendMessage);
            Debug.Log("✅ Send button listener added");
        }
        else
        {
            Debug.LogWarning("⚠️ Send button is null!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseChat);
            Debug.Log("✅ Close button listener added");
        }
        else
        {
            Debug.LogWarning("⚠️ Close button is null!");
        }

        if (playerInputField != null)
        {
            playerInputField.onEndEdit.RemoveAllListeners();
            playerInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            Debug.Log("✅ Input field listener added");
        }
        else
        {
            Debug.LogWarning("⚠️ Player input field is null!");
        }

        Debug.Log("🔧 UI listeners setup complete");
    }

    private void SetupWebSocketClient()
    {
        if (webSocketClient == null)
        {
            // Try to find WebSocketClient in the scene
            webSocketClient = FindObjectOfType<WebSocketChatClient>();

            if (webSocketClient == null)
            {
                Debug.LogWarning("⚠️ WebSocketChatClient not found. Creating one automatically...");
                GameObject wsClientGO = new GameObject("WebSocketChatClient");
                webSocketClient = wsClientGO.AddComponent<WebSocketChatClient>();
            }
        }

        if (webSocketClient != null)
        {
            Debug.Log("🔗 Setting up WebSocket event handlers...");

            // Unsubscribe first to avoid duplicates
            webSocketClient.OnStreamingStart -= OnAIStreamingStart;
            webSocketClient.OnResponseChunk -= OnAIResponseChunk;
            webSocketClient.OnResponseComplete -= OnAIResponseComplete;
            webSocketClient.OnError -= OnAIError;
            webSocketClient.OnConnectionChanged -= OnConnectionChanged;

            // Subscribe to WebSocket events
            webSocketClient.OnStreamingStart += OnAIStreamingStart;
            webSocketClient.OnResponseChunk += OnAIResponseChunk;
            webSocketClient.OnResponseComplete += OnAIResponseComplete;
            webSocketClient.OnError += OnAIError;
            webSocketClient.OnConnectionChanged += OnConnectionChanged;

            Debug.Log("✅ WebSocket event handlers connected");
            Debug.Log("🔗 Event handlers subscribed to WebSocket client");

            // Test event by triggering manually (this will work if events are connected)
            Debug.Log("🧪 Testing event connection...");
            webSocketClient.TestEventHandlers();

            Debug.Log("✅ Event handler setup complete");
        }
        else
        {
            Debug.LogError("❌ Failed to setup WebSocket client");
            useAIChat = false;
        }
    }

    [ContextMenu("Force Reconnect WebSocket Events")]
    public void ForceReconnectWebSocketEvents()
    {
        Debug.Log("🔄 Manually reconnecting WebSocket events...");
        SetupWebSocketClient();
    }

    [ContextMenu("Test Event Handlers Manually")]
    public void TestEventHandlersManually()
    {
        Debug.Log("🧪 Manual event handler test from ChatManager...");
        if (webSocketClient != null)
        {
            webSocketClient.TestEventHandlers();
        }
        else
        {
            Debug.LogError("❌ WebSocketClient is null!");
        }
    }

    void OnDestroy()
    {
        // Clean up WebSocket connections
        if (webSocketClient != null)
        {
            webSocketClient.OnStreamingStart -= OnAIStreamingStart;
            webSocketClient.OnResponseChunk -= OnAIResponseChunk;
            webSocketClient.OnResponseComplete -= OnAIResponseComplete;
            webSocketClient.OnError -= OnAIError;
            webSocketClient.OnConnectionChanged -= OnConnectionChanged;
        }
    }

    void Update()
    {
        // Close chat with Escape key
        if (isChatOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }

        // Auto-close chat if player walks too far away
        if (isChatOpen && currentAgent != null && !isAutoClosing)
        {
            CheckPlayerDistance();
        }
    }

    public void OpenChat(NPCAgent agent)
    {
        if (agent == null)
        {
            Debug.LogError("❌ OpenChat called with null agent!");
            return;
        }

        Debug.Log($"🗨️ Opening chat with {agent.agentName}");
        Debug.Log($"🔍 ChatUI null? {chatUI == null}, ChatUIComponent null? {chatUIComponent == null}");
        Debug.Log($"🔍 NPCNameText null? {npcNameText == null}, ChatText null? {chatText == null}");

        // Debug WebSocket connection status
        Debug.Log($"🔗 WebSocket client: {(webSocketClient != null ? "Found" : "NULL")}");
        Debug.Log($"🔗 WebSocket connected: {(webSocketClient != null ? webSocketClient.IsConnected.ToString() : "N/A")}");
        Debug.Log($"🔗 Use AI Chat: {useAIChat}");

        // Force reconnect events to ensure they're working
        if (webSocketClient != null)
        {
            Debug.Log("🔄 Re-ensuring WebSocket event handlers are connected...");
            SetupWebSocketClient();
        }

        // Check if this is a new agent BEFORE setting currentAgent
        bool isNewAgent = (currentAgent == null || currentAgent != agent);
        currentAgent = agent;
        isChatOpen = true;
        isAutoClosing = false; // Reset auto-closing flag

        // Use ChatUI component for smooth animations if available
        if (chatUIComponent != null)
        {
            Debug.Log("✨ Using ChatUI component animations");
            chatUIComponent.ShowChatUI();
            chatUIComponent.CustomizeUIForAgent(agent);
        }
        else if (chatUI != null)
        {
            Debug.Log("📱 Activating ChatUI GameObject");
            chatUI.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No ChatUI found! Creating emergency UI...");
            CreateEmergencyUI();
        }

        if (npcNameText != null)
            npcNameText.text = agent.agentName;
        else
            Debug.LogWarning("⚠️ NPCNameText is null!");

        // Only clear chat history if opening a NEW agent (not same agent)
        if (isNewAgent)
        {
            Debug.Log($"🗑️ Opening NEW agent {agent.agentName} - clearing chat history");
            chatHistory.Clear();
        }
        else
        {
            Debug.Log($"📖 Reopening same agent {agent.agentName} - keeping chat history");
        }

        // Show greeting only for NEW agents
        if (isNewAgent)
        {
            if (useAIChat && webSocketClient != null)
            {
                if (webSocketClient.IsConnected)
                {
                    string greeting = $"Hello! I'm {agent.agentName}. ";
                    AddMessageToChat(agent.agentName, greeting);
                }
                else
                {
                    string greeting = $"Hello! I'm {agent.agentName}. I'm having trouble connecting to my AI brain, but I can still chat with simple responses.";
                    AddMessageToChat(agent.agentName, greeting);
                }
            }
            else
            {
                string greeting = "Hello! I'm " + agent.agentName + ". How can I help you?";
                AddMessageToChat(agent.agentName, greeting);
            }
        }
        else
        {
            Debug.Log($"📖 Reopening existing chat - no new greeting needed");
        }

        // Focus input field
        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerInputField is null!");
        }

        // Keep normal game speed (time slowdown removed)
        // Time.timeScale = 0.1f; // Commented out - no time slowdown needed

        Debug.Log("✅ Chat should now be visible!");
    }

    public void CloseChat()
    {
        CloseChat(false); // Manual close by default
    }

    public void CloseChat(bool autoClose)
    {
        if (autoClose)
        {
            Debug.Log("🚶 Auto-closing chat due to distance");
        }
        else
        {
            Debug.Log("🚪 CloseChat method called - Manual close");
        }

        isChatOpen = false;
        currentAgent = null;
        isAutoClosing = false; // Reset auto-closing flag

        // Use ChatUI component for smooth animations if available
        if (chatUIComponent != null)
        {
            Debug.Log("✨ Closing chat with ChatUI animations");
            chatUIComponent.HideChatUI();
        }
        else if (chatUI != null)
        {
            Debug.Log("📱 Deactivating ChatUI GameObject");
            chatUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ No ChatUI found to close!");
        }

        // Resume normal game time (always set to 1, in case it was changed elsewhere)
        Time.timeScale = 1f;

        Debug.Log("✅ Chat closed successfully");
    }

    // Test method to verify button functionality
    public void TestButtonClick()
    {
        Debug.Log("🧪 TEST: Button click detected! Buttons are working!");
    }

    private void SendMessage()
    {
        Debug.Log("📤 SendMessage method called - Button working!");

        if (playerInputField == null)
        {
            Debug.LogError("❌ PlayerInputField is null!");
            return;
        }

        if (string.IsNullOrEmpty(playerInputField.text.Trim()))
        {
            Debug.Log("⚠️ Input field is empty");
            return;
        }

        string playerMessage = playerInputField.text.Trim();
        Debug.Log($"📝 Player message: {playerMessage}");

        // Add player message to chat
        AddMessageToChat("You", playerMessage);

        // Send message to AI or generate fallback response
        if (useAIChat && webSocketClient != null && webSocketClient.IsConnected)
        {
            isWaitingForAIResponse = true;
            streamingResponse.Clear();

            // Disable send button while waiting for response
            SetSendButtonEnabled(false);

            // Use agent name as agent_id for the AI
            string agentId = currentAgent != null ? currentAgent.agentName : "default";
            webSocketClient.SendChatMessage(playerMessage, agentId);

            Debug.Log($"🤖 Sent message to AI agent: {agentId}");
        }
        else
        {
            // Fallback to simple responses
            string npcResponse = GenerateNPCResponse(playerMessage);
            Debug.Log($"🤖 Fallback response: {npcResponse}");
            StartCoroutine(DelayedNPCResponse(npcResponse));
        }

        // Clear input field
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    private void OnInputFieldEndEdit(string value)
    {
        Debug.Log("⌨️ Input field end edit triggered");
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("📨 Enter key pressed, sending message");
            SendMessage();
        }
    }

    private string GenerateNPCResponse(string playerMessage)
    {
        if (currentAgent == null) return "...";

        // Simple response system - you can make this more sophisticated
        string message = playerMessage.ToLower();

        if (message.Contains("hello") || message.Contains("hi"))
        {
            return "Hello there! Nice to meet you!";
        }
        else if (message.Contains("help"))
        {
            return "I'm here to assist you. What do you need help with?";
        }
        else if (message.Contains("bye") || message.Contains("goodbye"))
        {
            return "Goodbye! Feel free to talk to me anytime.";
        }
        else if (message.Contains("name"))
        {
            return "My name is " + currentAgent.agentName + ". What's yours?";
        }
        else
        {
            // Return random dialogue from agent
            return currentAgent.GetRandomDialogue();
        }
    }

    private IEnumerator DelayedNPCResponse(string response)
    {
        yield return new WaitForSecondsRealtime(1f); // Wait 1 second in real time
        AddMessageToChat(currentAgent.agentName, response);
    }

    // WebSocket Event Handlers
    private void OnAIStreamingStart(string agentId)
    {
        Debug.Log($"🎬 ChatManager: AI streaming started for agent: {agentId}");
        Debug.Log($"🎬 ChatManager: Current agent is: {(currentAgent != null ? currentAgent.agentName : "NULL")}");
        streamingResponse.Clear();

        // TEMPORARILY DISABLED: No streaming message to avoid interference
        Debug.Log($"🚫 Streaming message disabled to prevent interference with player messages");

        // Add agent name with typing indicator
        // if (currentAgent != null)
        // {
        //     Debug.Log($"🎬 ChatManager: Adding streaming message for {currentAgent.agentName}");
        //     AddStreamingMessage(currentAgent.agentName, "");
        // }
        // else
        // {
        //     Debug.LogError("🎬 ChatManager: currentAgent is null!");
        // }
    }

    private void OnAIResponseChunk(string chunk)
    {
        Debug.Log($"📝 ChatManager: Received AI chunk: '{chunk}'");
        streamingResponse.Append(chunk);

        Debug.Log($"📝 ChatManager: Total response so far: '{streamingResponse.ToString()}'");

        // DISABLED: UpdateStreamingMessage modifies existing messages 
        // Now we only add messages when complete, never modify existing ones
        Debug.Log($"📝 DISABLED: Not updating existing messages - will add when complete");

        if (currentAgent == null)
        {
            Debug.LogError("📝 ChatManager: currentAgent is null!");
        }
    }

    private void OnAIResponseComplete(string fullResponse)
    {
        Debug.Log($"🎯🏁 ChatManager: AI response complete EVENT RECEIVED!");
        Debug.Log($"🏁 ChatManager: Response content: '{fullResponse}'");

        isWaitingForAIResponse = false;

        // Re-enable send button
        SetSendButtonEnabled(true);

        // DIRECT ADD: Just add the AI response to new position
        if (currentAgent != null)
        {
            Debug.Log($"🏁 ChatManager: DIRECTLY adding AI response");
            string aiMessage = currentAgent.agentName + ": " + fullResponse;
            chatHistory.Add(aiMessage);
            Debug.Log($"💬 AI response added: '{aiMessage}' Total messages: {chatHistory.Count}");

            // Update display
            UpdateChatDisplay();
        }
        else
        {
            Debug.LogError("❌ ChatManager: Cannot add response - currentAgent is null!");
        }
    }

    private void OnAIError(string error)
    {
        Debug.LogError($"❌ AI Error: {error}");
        isWaitingForAIResponse = false;

        // Re-enable send button
        SetSendButtonEnabled(true);

        // Show error message to user
        if (currentAgent != null)
        {
            AddMessageToChat(currentAgent.agentName, fallbackResponse);
        }
    }

    private void OnConnectionChanged()
    {
        bool connected = webSocketClient != null && webSocketClient.IsConnected;
        Debug.Log($"🔌 WebSocket connection changed: {(connected ? "Connected" : "Disconnected")}");

        if (!connected && isWaitingForAIResponse)
        {
            // Handle disconnection during conversation
            isWaitingForAIResponse = false;
            SetSendButtonEnabled(true);

            if (currentAgent != null)
            {
                AddMessageToChat(currentAgent.agentName, "Sorry, I lost connection to my AI brain. Please try again!");
            }
        }
    }

    private void SetSendButtonEnabled(bool enabled)
    {
        if (sendButton != null)
        {
            sendButton.interactable = enabled;

            // Visual feedback for disabled state
            if (enabled)
            {
                Debug.Log("✅ Send button enabled");
            }
            else
            {
                Debug.Log("⏸️ Send button disabled (waiting for AI response)");
            }
        }
    }

    private void AddMessageToChat(string speaker, string message)
    {
        string chatMessage = speaker + ": " + message;
        chatHistory.Add(chatMessage);

        Debug.Log($"💬 Added '{chatMessage}' to history. Total messages: {chatHistory.Count}");

        // Update the display
        UpdateChatDisplay();
    }

    // Streaming Message Methods
    private void AddStreamingMessage(string speaker, string message)
    {
        string chatMessage = speaker + ": " + message + " ●"; // Add typing indicator

        Debug.Log($"🔵 BEFORE AddStreamingMessage - History count: {chatHistory.Count}");
        for (int i = 0; i < chatHistory.Count; i++)
        {
            Debug.Log($"🔵 BEFORE [{i}]: {chatHistory[i]}");
        }

        // DISABLED: Using only AddMessageToChat now to avoid multiple add methods
        // chatHistory.Add(chatMessage);

        Debug.Log($"🎬 UI: DISABLED streaming message add: '{chatMessage}'");
        Debug.Log($"🟢 AFTER AddStreamingMessage - History count: {chatHistory.Count}");
        for (int i = 0; i < chatHistory.Count; i++)
        {
            Debug.Log($"🟢 AFTER [{i}]: {chatHistory[i]}");
        }

        if (chatText != null)
        {
            Debug.Log("🎬 UI: Updating chat display");
            UpdateChatDisplay();
        }
        else
        {
            Debug.LogError("🎬 UI: chatText is null!");
        }
    }

    private void UpdateStreamingMessage(string speaker, string message)
    {
        Debug.Log($"📝 UI: Updating streaming message: '{speaker}: {message} ●'");

        if (chatHistory.Count > 0)
        {
            // Update the last message (which should be the streaming one)
            string chatMessage = speaker + ": " + message + " ●";
            // DISABLED: Never modify existing messages, only add new ones
            // chatHistory[chatHistory.Count - 1] = chatMessage;

            Debug.Log($"📝 UI: Updated last message to: '{chatMessage}'");

            if (chatText != null)
            {
                UpdateChatDisplay();
            }
            else
            {
                Debug.LogError("📝 UI: chatText is null!");
            }
        }
        else
        {
            Debug.LogError("📝 UI: Chat history is empty!");
        }
    }

    private void FinalizeStreamingMessage(string speaker, string message)
    {
        // NO LONGER USED: AI responses are added directly in OnAIResponseComplete
        Debug.Log($"🚫 FinalizeStreamingMessage DISABLED - using direct add in OnAIResponseComplete");
    }

    private void UpdateChatDisplay()
    {
        if (chatText == null)
        {
            Debug.LogError("💬 chatText is null!");
            return;
        }

        // Simple: Join all messages with newlines
        string fullChat = "";
        foreach (string msg in chatHistory)
        {
            fullChat += msg + "\n";
        }

        chatText.text = fullChat;
        Debug.Log($"💬 Updated display with {chatHistory.Count} messages");

        // Auto-scroll to bottom
        if (chatScrollRect != null)
        {
            StartCoroutine(ScrollToBottomDelayed());
        }
    }

    private void CreateEmergencyUI()
    {
        Debug.Log("🚨 Creating emergency UI as fallback...");

        // Ensure EventSystem exists
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ EventSystem created for UI interaction");
        }

        // Create a simple UI on the fly
        GameObject canvas = new GameObject("Emergency_Canvas");
        Canvas canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 1000;

        // Add GraphicRaycaster for UI interaction
        UnityEngine.UI.GraphicRaycaster raycaster = canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        raycaster.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.All;

        // Create background panel
        GameObject panel = new GameObject("Emergency_ChatPanel");
        panel.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.25f);
        panelRect.anchorMax = new Vector2(0.75f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create NPC name text at top
        GameObject nameObj = new GameObject("NPC_Name");
        nameObj.transform.SetParent(panel.transform, false);

        UnityEngine.UI.Text nameText = nameObj.AddComponent<UnityEngine.UI.Text>();
        nameText.text = "NPC";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 18;
        nameText.color = Color.yellow;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.85f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // Create ScrollRect for chat
        GameObject scrollViewObj = new GameObject("Chat_ScrollView");
        scrollViewObj.transform.SetParent(panel.transform, false);

        UnityEngine.UI.Image scrollBgImage = scrollViewObj.AddComponent<UnityEngine.UI.Image>();
        scrollBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark background

        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;
        scrollRect.scrollSensitivity = 30f;

        RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.05f, 0.3f);
        scrollViewRect.anchorMax = new Vector2(0.95f, 0.8f);
        scrollViewRect.offsetMin = Vector2.zero;
        scrollViewRect.offsetMax = Vector2.zero;

        // Create Mask component for clipping
        UnityEngine.UI.Mask mask = scrollViewObj.AddComponent<UnityEngine.UI.Mask>();
        mask.showMaskGraphic = true;

        // Create Content area
        GameObject contentObj = new GameObject("Chat_Content");
        contentObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 50); // Start with small height

        // Add VerticalLayoutGroup for automatic spacing
        UnityEngine.UI.VerticalLayoutGroup layoutGroup = contentObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 5f;

        // Add ContentSizeFitter to auto-resize content
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Set up ScrollRect references
        scrollRect.content = contentRect;
        scrollRect.viewport = scrollViewRect;

        // Create chat text area inside content
        GameObject textObj = new GameObject("Chat_Text");
        textObj.transform.SetParent(contentObj.transform, false);

        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; // Enable text wrapping
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Add ContentSizeFitter to text for proper height calculation
        ContentSizeFitter textFitter = textObj.AddComponent<ContentSizeFitter>();
        textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create input field
        GameObject inputObj = new GameObject("Input_Field");
        inputObj.transform.SetParent(panel.transform, false);

        UnityEngine.UI.Image inputImage = inputObj.AddComponent<UnityEngine.UI.Image>();
        inputImage.color = Color.white;

        UnityEngine.UI.InputField inputField = inputObj.AddComponent<UnityEngine.UI.InputField>();

        // Create input text
        GameObject inputTextObj = new GameObject("Input_Text");
        inputTextObj.transform.SetParent(inputObj.transform, false);

        UnityEngine.UI.Text inputText = inputTextObj.AddComponent<UnityEngine.UI.Text>();
        inputText.text = "";
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = 14;
        inputText.color = Color.black;

        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = new Vector2(5, 0);
        inputTextRect.offsetMax = new Vector2(-5, 0);

        inputField.textComponent = inputText;

        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.05f, 0.1f);
        inputRect.anchorMax = new Vector2(0.65f, 0.25f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        // Create send button
        GameObject sendObj = new GameObject("Send_Button");
        sendObj.transform.SetParent(panel.transform, false);

        UnityEngine.UI.Image sendImage = sendObj.AddComponent<UnityEngine.UI.Image>();
        sendImage.color = new Color(0.2f, 0.6f, 1f);

        UnityEngine.UI.Button sendButton = sendObj.AddComponent<UnityEngine.UI.Button>();
        sendButton.interactable = true;

        // Add visual feedback for button interaction
        ColorBlock sendColors = sendButton.colors;
        sendColors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        sendColors.highlightedColor = new Color(0.4f, 0.7f, 1f, 1f);
        sendColors.pressedColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        sendButton.colors = sendColors;

        GameObject sendTextObj = new GameObject("Send_Text");
        sendTextObj.transform.SetParent(sendObj.transform, false);

        UnityEngine.UI.Text sendText = sendTextObj.AddComponent<UnityEngine.UI.Text>();
        sendText.text = "Send";
        sendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sendText.fontSize = 14;
        sendText.color = Color.white;
        sendText.alignment = TextAnchor.MiddleCenter;

        RectTransform sendTextRect = sendTextObj.GetComponent<RectTransform>();
        sendTextRect.anchorMin = Vector2.zero;
        sendTextRect.anchorMax = Vector2.one;
        sendTextRect.offsetMin = Vector2.zero;
        sendTextRect.offsetMax = Vector2.zero;

        RectTransform sendRect = sendObj.GetComponent<RectTransform>();
        sendRect.anchorMin = new Vector2(0.7f, 0.1f);
        sendRect.anchorMax = new Vector2(0.9f, 0.25f);
        sendRect.offsetMin = Vector2.zero;
        sendRect.offsetMax = Vector2.zero;

        // Create close button
        GameObject closeObj = new GameObject("Close_Button");
        closeObj.transform.SetParent(panel.transform, false);

        UnityEngine.UI.Image closeImage = closeObj.AddComponent<UnityEngine.UI.Image>();
        closeImage.color = new Color(1f, 0.3f, 0.3f);

        UnityEngine.UI.Button closeButton = closeObj.AddComponent<UnityEngine.UI.Button>();
        closeButton.interactable = true;

        // Add visual feedback for button interaction
        ColorBlock colors = closeButton.colors;
        colors.normalColor = new Color(1f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        closeButton.colors = colors;

        GameObject closeTextObj = new GameObject("Close_Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);

        UnityEngine.UI.Text closeText = closeTextObj.AddComponent<UnityEngine.UI.Text>();
        closeText.text = "✕";
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 16;
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.fontStyle = FontStyle.Bold;

        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.88f, 0.88f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        // Set all references
        chatUI = panel;
        npcNameText = nameText;
        chatText = text;
        playerInputField = inputField;
        this.sendButton = sendButton;
        this.closeButton = closeButton;
        chatScrollRect = scrollRect; // Reference to scroll rect for auto-scrolling

        Debug.Log("✅ Scrollable chat UI configured with all message history");

        Debug.Log($"📊 UI Components before listener setup: Send={this.sendButton != null}, Close={this.closeButton != null}, Input={inputField != null}");

        // Setup button listeners immediately 
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(() =>
            {
                Debug.Log("🟢 Send button clicked!");
                SendMessage();
            });
            // Add test listener as fallback
            sendButton.onClick.AddListener(TestButtonClick);
            Debug.Log("✅ Send button listener added directly");
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                Debug.Log("🔴 Close button clicked!");
                CloseChat();
            });
            // Add test listener as fallback
            closeButton.onClick.AddListener(TestButtonClick);
            Debug.Log("✅ Close button listener added directly");
        }

        if (inputField != null)
        {
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            Debug.Log("✅ Input field listener added directly");
        }

        Debug.Log("✅ Emergency UI created with full functionality!");
        Debug.Log($"📊 Final UI Components: Send={this.sendButton != null}, Close={this.closeButton != null}, Input={inputField != null}");
    }

    private void CheckPlayerDistance()
    {
        // Find the player (GameObject with Mover component)
        Mover playerMover = FindObjectOfType<Mover>();
        if (playerMover == null)
        {
            Debug.LogWarning("⚠️ Player (Mover component) not found for distance check");
            return;
        }

        // Calculate distance between player and current NPC
        float distance = Vector3.Distance(playerMover.transform.position, currentAgent.transform.position);

        // Get the interaction range from the NPC (with a small buffer to prevent flickering)
        float maxDistance = currentAgent.interactionRange + 1f; // 1 unit buffer

        // Debug logging (every few frames to avoid spam)
        if (Time.frameCount % 60 == 0) // Log every 60 frames (about once per second)
        {
            Debug.Log($"📏 Distance check: Player-NPC distance = {distance:F1}, Max allowed = {maxDistance:F1}");
        }

        // Auto-close if too far away
        if (distance > maxDistance)
        {
            Debug.Log($"🚶 Player walked too far from {currentAgent.agentName} (distance: {distance:F1} > {maxDistance:F1}). Auto-closing chat.");

            isAutoClosing = true; // Set flag to prevent multiple auto-close attempts

            // Add a farewell message before closing
            if (currentAgent != null)
            {
                AddMessageToChat(currentAgent.agentName, "You walked away. Talk to me again when you're closer!");

                // Brief delay before closing to show the message
                StartCoroutine(DelayedAutoClose());
            }
            else
            {
                CloseChat(true); // Auto-close immediately if no agent
            }
        }
    }

    private IEnumerator DelayedAutoClose()
    {
        // Wait 1.5 seconds to let player read the farewell message
        yield return new WaitForSecondsRealtime(1.5f);
        CloseChat(true); // Auto-close
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
            chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private IEnumerator ScrollToBottomDelayed()
    {
        // Wait a bit longer for content size fitter to calculate proper sizes
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
            Debug.Log("📜 Auto-scrolled to bottom of chat");
        }
    }
}

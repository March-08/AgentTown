using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

/*
 * WebSocket Chat Client for Unity
 * 
 * Connects to the AI chat server on localhost:8000 and handles real-time communication
 * with AI agents for dynamic conversations.
 */

public class WebSocketChatClient : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private string serverUrl = "ws://localhost:8000/ws/chat";
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private float reconnectDelay = 5f;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField]
    private string[] alternativeUrls = {
        "ws://127.0.0.1:8000/ws/chat",
        "ws://0.0.0.0:8000/ws/chat"
    };

    private ClientWebSocket webSocket;
    private CancellationTokenSource cancellationTokenSource;
    private bool isConnected = false;
    private bool isConnecting = false;
    private Queue<string> messageQueue = new Queue<string>();

    // Events for chat responses
    public event System.Action<string> OnStreamingStart;
    public event System.Action<string> OnResponseChunk;
    public event System.Action<string> OnResponseComplete;
    public event System.Action<string> OnError;
    public event System.Action OnConnectionChanged;

    // Current conversation tracking
    private string currentAgentId = "";
    private bool isReceivingResponse = false;
    private StringBuilder currentResponse = new StringBuilder();
    private int connectionAttempts = 0;
    private int maxConnectionAttempts = 3;

    public bool IsConnected => isConnected;
    public bool IsReceivingResponse => isReceivingResponse;

    void Start()
    {
        if (autoConnect)
        {
            StartCoroutine(ConnectToServer());
        }
    }

    void Update()
    {
        // Process any queued messages on the main thread
        while (messageQueue.Count > 0)
        {
            ProcessQueuedMessage(messageQueue.Dequeue());
        }
    }

    void OnDestroy()
    {
        DisconnectFromServer();
    }

    public void StartConnection()
    {
        if (!isConnected && !isConnecting)
        {
            StartCoroutine(ConnectToServer());
        }
    }

    public void StopConnection()
    {
        DisconnectFromServer();
    }

    private IEnumerator ConnectToServer()
    {
        if (isConnecting || isConnected) yield break;

        isConnecting = true;
        connectionAttempts++;

        if (enableDetailedLogging)
        {
            Debug.Log($"🔌 Connection attempt #{connectionAttempts} to WebSocket server...");
            Debug.Log($"🔍 Checking server reachability...");
            Debug.Log($"📋 Connection Details:");
            Debug.Log($"   - URL: {serverUrl}");
            Debug.Log($"   - Platform: {Application.platform}");
            Debug.Log($"   - Unity Version: {Application.unityVersion}");
        }

        // Try primary URL first, then alternatives
        string[] urlsToTry = new string[1 + alternativeUrls.Length];
        urlsToTry[0] = serverUrl;
        for (int i = 0; i < alternativeUrls.Length; i++)
        {
            urlsToTry[i + 1] = alternativeUrls[i];
        }

        bool connectionSuccessful = false;
        string lastError = "";

        foreach (string urlToTry in urlsToTry)
        {
            if (enableDetailedLogging)
                Debug.Log($"🔗 Trying to connect to: {urlToTry}");

            yield return StartCoroutine(TryConnectToUrl(urlToTry));

            // Check if connection was successful after the coroutine completes
            if (isConnected)
            {
                connectionSuccessful = true;
                break;
            }

            yield return new WaitForSeconds(1f); // Small delay between attempts
        }

        isConnecting = false;

        if (connectionSuccessful)
        {
            connectionAttempts = 0; // Reset on successful connection
            Debug.Log($"✅ Connected to WebSocket server!");

            OnConnectionChanged?.Invoke();
            StartListening();
        }
        else
        {
            Debug.LogError($"❌ Failed to connect to WebSocket server after trying all URLs");
            Debug.LogError($"🔍 Troubleshooting tips:");
            Debug.LogError($"   1. Make sure your server is running: python -m uvicorn main:app --host 0.0.0.0 --port 8000");
            Debug.LogError($"   2. Check if port 8000 is accessible: netstat -an | grep 8000");
            Debug.LogError($"   3. Try accessing http://localhost:8000 in your browser");
            Debug.LogError($"   4. Check Windows Firewall/antivirus if on Windows");
            Debug.LogError($"   5. The chat system will use fallback responses for now");

            // Attempt reconnection if enabled and under max attempts
            if (autoConnect && connectionAttempts < maxConnectionAttempts)
            {
                Debug.Log($"🔄 Will retry connection in {reconnectDelay} seconds... (Attempt {connectionAttempts}/{maxConnectionAttempts})");
                yield return new WaitForSeconds(reconnectDelay);
                StartCoroutine(ConnectToServer());
            }
            else
            {
                Debug.Log("🛑 Max connection attempts reached. Chat will use fallback responses.");
                connectionAttempts = 0; // Reset for potential manual retry
            }
        }
    }

    private IEnumerator TryConnectToUrl(string url)
    {
        bool connectionSuccessful = false;
        Exception connectionError = null;

        // Setup WebSocket
        CleanupWebSocket(); // Clean up any previous connection
        webSocket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        // Start connection task
        Task connectionTask = null;
        try
        {
            connectionTask = webSocket.ConnectAsync(new Uri(url), cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            connectionError = e;
            if (enableDetailedLogging)
                Debug.LogWarning($"⚠️ Failed to start connection to {url}: {e.Message}");
        }

        if (connectionTask != null && connectionError == null)
        {
            // Wait for connection with timeout
            float timeout = 5f; // Shorter timeout for each individual attempt
            float elapsed = 0f;

            while (!connectionTask.IsCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Check connection result
            if (connectionTask.IsCompleted && connectionTask.Exception == null && webSocket.State == WebSocketState.Open)
            {
                connectionSuccessful = true;
                isConnected = true; // Set the global connection flag
                if (enableDetailedLogging)
                    Debug.Log($"✅ Successfully connected to {url}");
            }
            else if (connectionTask.Exception != null)
            {
                connectionError = connectionTask.Exception;
                if (enableDetailedLogging)
                    Debug.LogWarning($"⚠️ Connection to {url} failed: {connectionError.Message}");
            }
            else
            {
                if (enableDetailedLogging)
                    Debug.LogWarning($"⚠️ Connection to {url} timed out");
            }
        }

        if (!connectionSuccessful)
        {
            CleanupWebSocket();
            isConnected = false; // Ensure connection flag is false
        }
    }

    private async void StartListening()
    {
        try
        {
            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Queue message for main thread processing
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("🔌 WebSocket connection closed by server");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ WebSocket listening error: {e.Message}");
        }
        finally
        {
            isConnected = false;
            OnConnectionChanged?.Invoke();
        }
    }

    private void ProcessQueuedMessage(string jsonMessage)
    {
        try
        {
            Debug.Log($"📨 Raw received: {jsonMessage}");

            // Parse JSON response (simple parsing for this use case)
            if (jsonMessage.Contains("\"streaming\":true") || jsonMessage.Contains("\"streaming\": true"))
            {
                Debug.Log("🎬 Streaming started - triggering OnStreamingStart");
                isReceivingResponse = true;
                currentResponse.Clear();
                OnStreamingStart?.Invoke(currentAgentId);
                Debug.Log($"🎬 OnStreamingStart invoked with agent: {currentAgentId}");
            }
            else if (jsonMessage.Contains("\"chunk\":"))
            {
                // Extract chunk content
                string chunk = ExtractJsonValue(jsonMessage, "chunk");
                if (!string.IsNullOrEmpty(chunk))
                {
                    Debug.Log($"📝 Received chunk: '{chunk}' - triggering OnResponseChunk");
                    currentResponse.Append(chunk);
                    OnResponseChunk?.Invoke(chunk);
                    Debug.Log($"📝 OnResponseChunk invoked. Total response so far: '{currentResponse.ToString()}'");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Empty chunk extracted from: {jsonMessage}");
                }
            }
            else if (jsonMessage.Contains("\"streaming\":false") || jsonMessage.Contains("\"streaming\": false"))
            {
                Debug.Log("🏁 FIXED: Detected streaming:false - triggering OnResponseComplete");
                isReceivingResponse = false;

                // Check if there's a direct response field (complete response)
                string directResponse = ExtractJsonValue(jsonMessage, "response");
                string fullResponse;

                if (!string.IsNullOrEmpty(directResponse))
                {
                    // Server sent complete response directly
                    fullResponse = directResponse;
                    Debug.Log($"🏁 Direct response received: '{fullResponse}'");
                }
                else
                {
                    // Use accumulated chunks
                    fullResponse = currentResponse.ToString();
                    Debug.Log($"🏁 Accumulated response: '{fullResponse}'");
                }

                Debug.Log($"🎯 About to invoke OnResponseComplete with: '{fullResponse}'");
                Debug.Log($"🎯 OnResponseComplete event is: {(OnResponseComplete != null ? "NOT NULL" : "NULL")}");

                OnResponseComplete?.Invoke(fullResponse);
                currentResponse.Clear();
                Debug.Log("🏁 OnResponseComplete invoked and response cleared");
            }
            else if (jsonMessage.Contains("\"error\":"))
            {
                string error = ExtractJsonValue(jsonMessage, "error");
                Debug.LogError($"❌ Server error: {error}");
                OnError?.Invoke(error);
                isReceivingResponse = false;
            }
            else
            {
                Debug.LogWarning($"🤔 Unrecognized message format: {jsonMessage}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error processing message: {e.Message}");
            Debug.LogError($"❌ Message was: {jsonMessage}");
        }
    }

    public async void SendChatMessage(string message, string agentId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("⚠️ WebSocket not connected. Cannot send message.");
            OnError?.Invoke("Not connected to server. Please check your connection.");
            return;
        }

        try
        {
            currentAgentId = agentId;

            // Create JSON message
            string jsonMessage = $"{{\"message\": \"{EscapeJsonString(message)}\", \"agent_id\": \"{EscapeJsonString(agentId)}\"}}";

            Debug.Log($"📤 Sending: {jsonMessage}");

            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error sending message: {e.Message}");
            OnError?.Invoke($"Failed to send message: {e.Message}");
        }
    }

    private void CleanupWebSocket()
    {
        try
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            if (webSocket != null)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleanup", CancellationToken.None);
                }
                webSocket.Dispose();
                webSocket = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error during WebSocket cleanup: {e.Message}");
        }
    }

    private void DisconnectFromServer()
    {
        try
        {
            isConnected = false;
            CleanupWebSocket();
            OnConnectionChanged?.Invoke();
            Debug.Log("🔌 Disconnected from WebSocket server");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error during disconnect: {e.Message}");
        }
    }

    // Simple JSON value extraction (for basic use case)
    private string ExtractJsonValue(string json, string key)
    {
        try
        {
            // Try with space after colon first
            string searchKey = $"\"{key}\": \"";
            int startIndex = json.IndexOf(searchKey);

            // If not found, try without space
            if (startIndex == -1)
            {
                searchKey = $"\"{key}\":\"";
                startIndex = json.IndexOf(searchKey);
            }

            if (startIndex == -1)
            {
                Debug.LogWarning($"⚠️ Key '{key}' not found in JSON: {json}");
                return "";
            }

            startIndex += searchKey.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1)
            {
                Debug.LogWarning($"⚠️ End quote not found for key '{key}' in JSON: {json}");
                return "";
            }

            string result = json.Substring(startIndex, endIndex - startIndex);
            Debug.Log($"✅ Extracted '{key}': '{result}' from JSON");
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error extracting '{key}' from JSON: {e.Message}");
            return "";
        }
    }

    // Escape special characters for JSON
    private string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    // Public methods for connection status
    public void SetServerUrl(string url)
    {
        serverUrl = url;
    }

    public string GetConnectionStatus()
    {
        if (isConnected) return "Connected";
        if (isConnecting) return "Connecting...";
        return "Disconnected";
    }

    [ContextMenu("Test Connection")]
    public void TestConnection()
    {
        Debug.Log("🧪 Manual connection test initiated");
        StartConnection();
    }

    [ContextMenu("Reset Connection Attempts")]
    public void ResetConnectionAttempts()
    {
        connectionAttempts = 0;
        Debug.Log("🔄 Connection attempts counter reset");
    }

    [ContextMenu("Test Message")]
    public void TestMessage()
    {
        if (isConnected)
        {
            SendChatMessage("Hello, this is a test message from Unity!", "test");
            Debug.Log("📤 Test message sent");
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot send test message - not connected");
        }
    }

    [ContextMenu("Show Debug Info")]
    public void ShowDebugInfo()
    {
        Debug.Log("=== WEBSOCKET DEBUG INFO ===");
        Debug.Log($"🔌 Connected: {isConnected}");
        Debug.Log($"🔄 Connecting: {isConnecting}");
        Debug.Log($"📊 Connection Attempts: {connectionAttempts}/{maxConnectionAttempts}");
        Debug.Log($"🌐 Server URL: {serverUrl}");
        Debug.Log($"📱 Platform: {Application.platform}");
        Debug.Log($"🎮 Unity Version: {Application.unityVersion}");
        Debug.Log($"🔊 Detailed Logging: {enableDetailedLogging}");

        if (webSocket != null)
        {
            Debug.Log($"📡 WebSocket State: {webSocket.State}");
        }
        else
        {
            Debug.Log("📡 WebSocket: null");
        }

        Debug.Log("🔗 Alternative URLs:");
        for (int i = 0; i < alternativeUrls.Length; i++)
        {
            Debug.Log($"   {i + 1}. {alternativeUrls[i]}");
        }
    }

    [ContextMenu("Test Event Handlers")]
    public void TestEventHandlers()
    {
        Debug.Log("🧪 Testing WebSocket event handlers...");

        // Test OnStreamingStart
        Debug.Log("🎬 Testing OnStreamingStart...");
        OnStreamingStart?.Invoke("test_agent");

        // Test OnResponseChunk  
        Debug.Log("📝 Testing OnResponseChunk...");
        OnResponseChunk?.Invoke("TEST_CHUNK");

        // Test OnResponseComplete
        Debug.Log("🏁 Testing OnResponseComplete...");
        OnResponseComplete?.Invoke("TEST_RESPONSE");

        Debug.Log("✅ Event handler test complete");
    }
}

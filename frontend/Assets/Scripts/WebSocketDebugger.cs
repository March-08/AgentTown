using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Debug tool to help diagnose WebSocket connection issues
 */
public class WebSocketDebugger : MonoBehaviour
{
    [Header("Server Testing")]
    [SerializeField] private string serverBaseUrl = "http://localhost:8000";
    [SerializeField] private string websocketPath = "/ws/chat";

    [ContextMenu("Run Full Diagnostic")]
    public void RunFullDiagnostic()
    {
        StartCoroutine(FullDiagnosticSequence());
    }

    private IEnumerator FullDiagnosticSequence()
    {
        Debug.Log("🔍 === WEBSOCKET CONNECTION DIAGNOSTIC ===");

        // Test 1: Basic HTTP connectivity
        Debug.Log("📡 Test 1: Basic HTTP connectivity...");
        yield return StartCoroutine(TestHTTPConnection());

        yield return new WaitForSeconds(1f);

        // Test 2: Check available endpoints
        Debug.Log("📋 Test 2: Checking server endpoints...");
        yield return StartCoroutine(TestServerEndpoints());

        yield return new WaitForSeconds(1f);

        // Test 3: WebSocket-specific test
        Debug.Log("🔌 Test 3: WebSocket connection details...");
        TestWebSocketConnection();

        Debug.Log("🔍 === DIAGNOSTIC COMPLETE ===");
    }

    private IEnumerator TestHTTPConnection()
    {
        UnityWebRequest request = UnityWebRequest.Get(serverBaseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ HTTP connection successful!");
            Debug.Log($"📄 Server response code: {request.responseCode}");
        }
        else
        {
            Debug.LogError($"❌ HTTP connection failed: {request.error}");
            Debug.LogError("🚨 This is the primary issue - server not reachable!");
        }
    }

    private IEnumerator TestServerEndpoints()
    {
        // Test common FastAPI endpoints
        string[] endpointsToTest = {
            "/docs",           // FastAPI auto-docs
            "/openapi.json",   // OpenAPI spec
            "/ws/chat",        // Your WebSocket endpoint (will fail but shows if route exists)
            "/health",         // Common health check
            "/"               // Root
        };

        foreach (string endpoint in endpointsToTest)
        {
            string url = serverBaseUrl + endpoint;
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            Debug.Log($"📍 {endpoint}: {request.responseCode} - {request.result}");

            if (endpoint == "/ws/chat")
            {
                if (request.responseCode == 404)
                {
                    Debug.LogError("🚨 WebSocket endpoint /ws/chat NOT FOUND!");
                    Debug.LogError("   This means the route isn't registered in your FastAPI app");
                }
                else if (request.responseCode == 405)
                {
                    Debug.Log("✅ WebSocket endpoint exists (405 = Method Not Allowed for GET on WebSocket)");
                }
            }
        }
    }

    private void TestWebSocketConnection()
    {
        WebSocketChatClient wsClient = FindObjectOfType<WebSocketChatClient>();

        if (wsClient != null)
        {
            Debug.Log("🔌 WebSocket Client Status:");
            wsClient.ShowDebugInfo();

            Debug.Log("🧪 Attempting manual WebSocket connection...");
            wsClient.TestConnection();
        }
        else
        {
            Debug.LogError("❌ No WebSocketChatClient found in scene!");
        }
    }

    void Start()
    {
        // Auto-run diagnostic after a short delay
        Invoke("RunFullDiagnostic", 3f);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Simple HTTP test to verify server connectivity before WebSocket attempts
 */
public class ServerConnectionTester : MonoBehaviour
{
    [SerializeField] private string serverBaseUrl = "http://localhost:8000";

    [ContextMenu("Test HTTP Connection")]
    public void TestHTTPConnection()
    {
        StartCoroutine(TestServerReachability());
    }

    private IEnumerator TestServerReachability()
    {
        Debug.Log("🧪 Testing HTTP connection to server...");

        UnityWebRequest request = UnityWebRequest.Get(serverBaseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ HTTP connection successful! Server is reachable.");
            Debug.Log($"📄 Response: {request.downloadHandler.text}");
            Debug.Log("🔌 WebSocket should work if HTTP works. Check WebSocket-specific issues.");
        }
        else
        {
            Debug.LogError($"❌ HTTP connection failed: {request.error}");
            Debug.LogError("🔍 This means your server isn't reachable from Unity:");
            Debug.LogError($"   1. Make sure server is running on {serverBaseUrl}");
            Debug.LogError("   2. Check if you can access this URL in your browser");
            Debug.LogError("   3. Verify firewall/antivirus settings");
            Debug.LogError("   4. Try starting server with: uvicorn main:app --host 0.0.0.0 --port 8000");
        }
    }

    void Start()
    {
        // Auto-test on start
        Invoke("TestHTTPConnection", 2f);
    }
}

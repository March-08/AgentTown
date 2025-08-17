using UnityEngine;

/*
 * CHAT SYSTEM INITIALIZER
 * 
 * Drop this script on ANY GameObject in your scene to automatically set up the entire chat system!
 * 
 * USAGE:
 * 1. Add this script to any GameObject in your scene (or create an empty GameObject for it)
 * 2. Add NPCAgent components to any GameObjects you want as NPCs
 * 3. Make sure your player has the Mover component
 * 4. Play the game!
 * 
 * That's it! The entire chat UI will be created automatically.
 */

public class ChatSystemInitializer : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool initializeOnStart = true;

    [Header("Optional Customization")]
    [SerializeField] private Color chatPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Vector2 chatPanelSize = new Vector2(500, 400);

    void Start()
    {
        Debug.Log("🔧 ChatSystemInitializer Start() called");
        if (initializeOnStart)
        {
            Debug.Log("🔧 Auto-initialization enabled, starting setup...");
            InitializeChatSystem();
        }
        else
        {
            Debug.Log("🔧 Auto-initialization disabled");
        }
    }

    [ContextMenu("Initialize Chat System")]
    public void InitializeChatSystem()
    {
        Debug.Log("🔧 InitializeChatSystem() called");

        // Check if system is already initialized
        if (FindObjectOfType<ChatManager>() != null)
        {
            Debug.Log("⚠️ Chat system already initialized!");
            return;
        }

        Debug.Log("🔧 Creating temporary setup GameObject...");

        // Create the auto setup component temporarily
        GameObject tempSetup = new GameObject("TempChatSetup");
        AutoChatUISetup setupScript = tempSetup.AddComponent<AutoChatUISetup>();

        Debug.Log($"🔧 Applying customization: Color={chatPanelColor}, Size={chatPanelSize}");

        // Pass customization settings
        var setupType = typeof(AutoChatUISetup);
        var panelColorField = setupType.GetField("panelColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (panelColorField != null) panelColorField.SetValue(setupScript, chatPanelColor);

        var panelSizeField = setupType.GetField("chatPanelSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (panelSizeField != null) panelSizeField.SetValue(setupScript, chatPanelSize);

        // The AutoChatUISetup will do its work in Awake(), then we can destroy it
        StartCoroutine(CleanupTempSetup(tempSetup));

        Debug.Log("🚀 Chat system initialization started!");
    }

    private System.Collections.IEnumerator CleanupTempSetup(GameObject tempSetup)
    {
        yield return new WaitForEndOfFrame();
        if (tempSetup != null)
        {
            Destroy(tempSetup);
        }

        Debug.Log("✅ Chat system fully initialized and ready!");
    }
}

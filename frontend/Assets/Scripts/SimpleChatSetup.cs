using UnityEngine;
using UnityEngine.UI;

/*
 * SIMPLE CHAT SETUP
 * 
 * A minimal, guaranteed-to-work chat system setup.
 * Add this to any GameObject in your scene for instant chat functionality.
 * 
 * This is a simplified version that focuses on reliability over fancy features.
 */

public class SimpleChatSetup : MonoBehaviour
{
    void Start()
    {
        // Only create if no ChatManager exists
        if (FindObjectOfType<ChatManager>() == null)
        {
            CreateSimpleChatSystem();
        }
        else
        {
            Debug.Log("Chat system already exists, skipping SimpleChatSetup");
        }
    }

    void CreateSimpleChatSystem()
    {
        Debug.Log("🚀 Creating simple chat system...");

        // Create ChatManager first
        GameObject chatManagerGO = new GameObject("SimpleChatManager");
        ChatManager chatManager = chatManagerGO.AddComponent<ChatManager>();

        // Create Canvas
        GameObject canvasGO = new GameObject("SimpleChatCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create main chat panel
        GameObject panelGO = new GameObject("SimpleChatPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.25f);
        panelRect.anchorMax = new Vector2(0.75f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create NPC name text
        GameObject nameTextGO = new GameObject("NPCName");
        nameTextGO.transform.SetParent(panelGO.transform, false);

        Text nameText = nameTextGO.AddComponent<Text>();
        nameText.text = "NPC";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 16;
        nameText.color = Color.yellow;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;

        RectTransform nameRect = nameTextGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.85f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // Create chat text area
        GameObject chatTextGO = new GameObject("ChatText");
        chatTextGO.transform.SetParent(panelGO.transform, false);

        Text chatText = chatTextGO.AddComponent<Text>();
        chatText.text = "";
        chatText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        chatText.fontSize = 14;
        chatText.color = Color.white;
        chatText.alignment = TextAnchor.UpperLeft;

        RectTransform chatRect = chatTextGO.GetComponent<RectTransform>();
        chatRect.anchorMin = new Vector2(0.05f, 0.3f);
        chatRect.anchorMax = new Vector2(0.95f, 0.8f);
        chatRect.offsetMin = Vector2.zero;
        chatRect.offsetMax = Vector2.zero;

        // Create input field
        GameObject inputFieldGO = new GameObject("InputField");
        inputFieldGO.transform.SetParent(panelGO.transform, false);

        Image inputImage = inputFieldGO.AddComponent<Image>();
        inputImage.color = Color.white;

        InputField inputField = inputFieldGO.AddComponent<InputField>();

        // Input field text
        GameObject inputTextGO = new GameObject("Text");
        inputTextGO.transform.SetParent(inputFieldGO.transform, false);

        Text inputText = inputTextGO.AddComponent<Text>();
        inputText.text = "";
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = 14;
        inputText.color = Color.black;

        RectTransform inputTextRect = inputTextGO.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = new Vector2(5, 0);
        inputTextRect.offsetMax = new Vector2(-5, 0);

        inputField.textComponent = inputText;

        RectTransform inputRect = inputFieldGO.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.05f, 0.1f);
        inputRect.anchorMax = new Vector2(0.7f, 0.25f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        // Create send button
        GameObject sendButtonGO = new GameObject("SendButton");
        sendButtonGO.transform.SetParent(panelGO.transform, false);

        Image sendImage = sendButtonGO.AddComponent<Image>();
        sendImage.color = new Color(0.2f, 0.6f, 1f);

        Button sendButton = sendButtonGO.AddComponent<Button>();

        GameObject sendTextGO = new GameObject("Text");
        sendTextGO.transform.SetParent(sendButtonGO.transform, false);

        Text sendText = sendTextGO.AddComponent<Text>();
        sendText.text = "Send";
        sendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sendText.fontSize = 14;
        sendText.color = Color.white;
        sendText.alignment = TextAnchor.MiddleCenter;

        RectTransform sendTextRect = sendTextGO.GetComponent<RectTransform>();
        sendTextRect.anchorMin = Vector2.zero;
        sendTextRect.anchorMax = Vector2.one;
        sendTextRect.offsetMin = Vector2.zero;
        sendTextRect.offsetMax = Vector2.zero;

        RectTransform sendRect = sendButtonGO.GetComponent<RectTransform>();
        sendRect.anchorMin = new Vector2(0.75f, 0.1f);
        sendRect.anchorMax = new Vector2(0.95f, 0.25f);
        sendRect.offsetMin = Vector2.zero;
        sendRect.offsetMax = Vector2.zero;

        // Create close button
        GameObject closeButtonGO = new GameObject("CloseButton");
        closeButtonGO.transform.SetParent(panelGO.transform, false);

        Image closeImage = closeButtonGO.AddComponent<Image>();
        closeImage.color = new Color(1f, 0.3f, 0.3f);

        Button closeButton = closeButtonGO.AddComponent<Button>();

        GameObject closeTextGO = new GameObject("Text");
        closeTextGO.transform.SetParent(closeButtonGO.transform, false);

        Text closeText = closeTextGO.AddComponent<Text>();
        closeText.text = "X";
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 16;
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.fontStyle = FontStyle.Bold;

        RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        RectTransform closeRect = closeButtonGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.9f, 0.9f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.offsetMin = new Vector2(-30, -30);
        closeRect.offsetMax = Vector2.zero;

        // Setup ChatManager with all components
        chatManager.SetupUIComponents(
            panelGO,
            null, // No ChatUI component for simplicity
            nameText,
            chatText,
            inputField,
            sendButton,
            closeButton,
            null // No scroll rect for simplicity
        );

        // Start with UI hidden
        panelGO.SetActive(false);

        Debug.Log("✅ Simple chat system created successfully!");
        Debug.Log($"📊 Components: Panel={panelGO != null}, ChatManager={chatManager != null}, Text={chatText != null}");
    }
}

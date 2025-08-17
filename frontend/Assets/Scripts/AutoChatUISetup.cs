using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * AUTO CHAT UI SETUP
 * 
 * This script automatically creates the entire chat UI system at runtime.
 * Simply add this component to any GameObject in your scene (or it will auto-create one)
 * and everything will be set up automatically when the game starts.
 * 
 * No manual Unity UI setup required!
 */

public class AutoChatUISetup : MonoBehaviour
{
    [Header("UI Styling")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.4f, 0.8f, 1f);
    [SerializeField] private Color inputFieldColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Font customFont = null; // Optional custom font

    [Header("UI Dimensions")]
    [SerializeField] private Vector2 chatPanelSize = new Vector2(800, 600);
    [SerializeField] private int fontSize = 14;
    [SerializeField] private int titleFontSize = 18;

    void Awake()
    {
        Debug.Log("🔧 AutoChatUISetup Awake() - Creating UI system...");
        // Create the UI on awake so it's ready before any NPCs try to use it
        CreateChatUISystem();
    }

    void CreateChatUISystem()
    {
        Debug.Log("🔧 Starting CreateChatUISystem...");

        // Create Canvas
        Debug.Log("🔧 Creating Canvas...");
        GameObject canvasGO = CreateCanvas();

        // Create Chat Panel
        Debug.Log("🔧 Creating Chat Panel...");
        GameObject chatPanelGO = CreateChatPanel(canvasGO);

        // Create UI Elements
        Debug.Log("🔧 Creating UI Elements...");
        GameObject npcNameText = CreateNPCNameText(chatPanelGO);
        GameObject chatScrollView = CreateChatScrollView(chatPanelGO);
        GameObject chatText = GetChatTextFromScrollView(chatScrollView);
        GameObject inputField = CreateInputField(chatPanelGO);
        GameObject sendButton = CreateSendButton(chatPanelGO);
        GameObject closeButton = CreateCloseButton(chatPanelGO);

        Debug.Log($"🔧 UI Elements created: Canvas={canvasGO != null}, Panel={chatPanelGO != null}, Text={chatText != null}");

        // Create and Setup ChatManager
        Debug.Log("🔧 Setting up ChatManager...");
        SetupChatManager(chatPanelGO, npcNameText, chatText, inputField, sendButton, closeButton, chatScrollView);

        // Initially hide the chat panel
        chatPanelGO.SetActive(false);
        Debug.Log("🔧 Chat panel initially set to inactive");

        Debug.Log("✅ Auto Chat UI Setup Complete! Chat system ready to use.");
    }

    GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Auto_ChatCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to appear on top

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        return canvasGO;
    }

    GameObject CreateChatPanel(GameObject parent)
    {
        GameObject panelGO = new GameObject("ChatPanel");
        panelGO.transform.SetParent(parent.transform, false);

        // Add Image component for background
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = panelColor;

        // Add CanvasGroup for animations
        CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();

        // Setup RectTransform (centered)
        RectTransform rectTransform = panelGO.GetComponent<RectTransform>();

        // Add ChatUI component and set up its references
        ChatUI chatUIComponent = panelGO.AddComponent<ChatUI>();
        chatUIComponent.SetupUIReferences(canvasGroup, rectTransform);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = chatPanelSize;

        return panelGO;
    }

    GameObject CreateNPCNameText(GameObject parent)
    {
        GameObject textGO = new GameObject("NPCNameText");
        textGO.transform.SetParent(parent.transform, false);

        Text text = textGO.AddComponent<Text>();
        text.text = "NPC Name";
        text.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = titleFontSize;
        text.color = textColor;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;

        // Position at top
        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(0, -25);
        rectTransform.sizeDelta = new Vector2(0, 50);

        return textGO;
    }

    GameObject CreateChatScrollView(GameObject parent)
    {
        GameObject scrollViewGO = new GameObject("ChatScrollView");
        scrollViewGO.transform.SetParent(parent.transform, false);

        // Add ScrollRect
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 20f;

        // Add Image for background
        Image scrollImage = scrollViewGO.AddComponent<Image>();
        scrollImage.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);

        // Position in middle area
        RectTransform scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.05f, 0.25f);
        scrollRectTransform.anchorMax = new Vector2(0.95f, 0.85f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        // Create Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);

        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = Color.clear;

        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        scrollRect.viewport = viewportRect;

        // Create Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 200);

        scrollRect.content = contentRect;

        // Create Chat Text inside Content
        GameObject chatTextGO = new GameObject("ChatText");
        chatTextGO.transform.SetParent(contentGO.transform, false);

        Text chatText = chatTextGO.AddComponent<Text>();
        chatText.text = "";
        chatText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        chatText.fontSize = fontSize;
        chatText.color = textColor;
        chatText.alignment = TextAnchor.UpperLeft;

        RectTransform chatTextRect = chatTextGO.GetComponent<RectTransform>();
        chatTextRect.anchorMin = Vector2.zero;
        chatTextRect.anchorMax = Vector2.one;
        chatTextRect.offsetMin = new Vector2(10, 0);
        chatTextRect.offsetMax = new Vector2(-10, 0);

        // Add ContentSizeFitter for auto-sizing
        ContentSizeFitter fitter = chatTextGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return scrollViewGO;
    }

    GameObject GetChatTextFromScrollView(GameObject scrollView)
    {
        return scrollView.transform.Find("Viewport/Content/ChatText").gameObject;
    }

    GameObject CreateInputField(GameObject parent)
    {
        GameObject inputFieldGO = new GameObject("PlayerInputField");
        inputFieldGO.transform.SetParent(parent.transform, false);

        // Background Image
        Image inputImage = inputFieldGO.AddComponent<Image>();
        inputImage.color = inputFieldColor;

        InputField inputField = inputFieldGO.AddComponent<InputField>();

        // Create Text component for InputField
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputFieldGO.transform, false);

        Text text = textGO.AddComponent<Text>();
        text.text = "";
        text.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.black;
        text.supportRichText = false;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        inputField.textComponent = text;

        // Position at bottom
        RectTransform inputRect = inputFieldGO.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.05f, 0.05f);
        inputRect.anchorMax = new Vector2(0.7f, 0.2f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        return inputFieldGO;
    }

    GameObject CreateSendButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("SendButton");
        buttonGO.transform.SetParent(parent.transform, false);

        // Button component
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = buttonColor;

        Button button = buttonGO.AddComponent<Button>();

        // Button Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        Text text = textGO.AddComponent<Text>();
        text.text = "Send";
        text.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Position next to input field
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.75f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.95f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        return buttonGO;
    }

    GameObject CreateCloseButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("CloseButton");
        buttonGO.transform.SetParent(parent.transform, false);

        // Button component
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red close button

        Button button = buttonGO.AddComponent<Button>();

        // Button Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        Text text = textGO.AddComponent<Text>();
        text.text = "✕";
        text.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize + 2;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Position at top-right
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.9f, 0.9f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.offsetMin = new Vector2(-30, -30);
        buttonRect.offsetMax = Vector2.zero;

        return buttonGO;
    }

    void SetupChatManager(GameObject chatPanel, GameObject npcNameText, GameObject chatText,
                         GameObject inputField, GameObject sendButton, GameObject closeButton, GameObject scrollView)
    {
        // Find existing ChatManager or create one
        ChatManager chatManager = FindObjectOfType<ChatManager>();
        if (chatManager == null)
        {
            GameObject chatManagerGO = new GameObject("Auto_ChatManager");
            chatManager = chatManagerGO.AddComponent<ChatManager>();
            DontDestroyOnLoad(chatManagerGO); // Keep it between scenes
        }

        // Setup all UI components using the public method
        chatManager.SetupUIComponents(
            chatPanel,
            chatPanel.GetComponent<ChatUI>(),
            npcNameText.GetComponent<Text>(),
            chatText.GetComponent<Text>(),
            inputField.GetComponent<InputField>(),
            sendButton.GetComponent<Button>(),
            closeButton.GetComponent<Button>(),
            scrollView.GetComponent<ScrollRect>()
        );

        Debug.Log("✅ ChatManager automatically configured with runtime UI!");
    }
}

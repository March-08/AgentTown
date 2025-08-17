using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [Header("UI Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private AnimationCurve fadeAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform chatPanel;

    private bool isAnimating = false;

    void Start()
    {
        InitializeUI();
    }

    public void SetupUIReferences(CanvasGroup cg, RectTransform panel)
    {
        canvasGroup = cg;
        chatPanel = panel;
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Ensure the UI starts hidden
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (chatPanel == null)
            chatPanel = GetComponent<RectTransform>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public void ShowChatUI()
    {
        if (isAnimating) return;

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    public void HideChatUI()
    {
        if (isAnimating) return;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        isAnimating = true;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / fadeInDuration;
                float curveValue = fadeAnimationCurve.Evaluate(normalizedTime);

                canvasGroup.alpha = curveValue;

                // Optional: Scale animation
                if (chatPanel != null)
                {
                    float scale = Mathf.Lerp(0.8f, 1f, curveValue);
                    chatPanel.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (chatPanel != null)
                chatPanel.localScale = Vector3.one;
        }

        isAnimating = false;
    }

    private IEnumerator FadeOut()
    {
        isAnimating = true;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / fadeOutDuration;
                float curveValue = fadeAnimationCurve.Evaluate(1f - normalizedTime);

                canvasGroup.alpha = startAlpha * curveValue;

                // Optional: Scale animation
                if (chatPanel != null)
                {
                    float scale = Mathf.Lerp(0.8f, 1f, curveValue);
                    chatPanel.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            canvasGroup.alpha = 0f;
            if (chatPanel != null)
                chatPanel.localScale = Vector3.one * 0.8f;
        }

        gameObject.SetActive(false);
        isAnimating = false;
    }

    // Method to be called by buttons or other UI elements
    public void OnCloseButtonClicked()
    {
        ChatManager.Instance?.CloseChat();
    }

    // Method to handle UI sound effects (if you have an audio system)
    public void PlayUISound(string soundName)
    {
        // You can implement this if you have an audio manager
        // AudioManager.Instance?.PlaySound(soundName);
    }

    // Method to customize UI based on the agent
    public void CustomizeUIForAgent(NPCAgent agent)
    {
        if (agent == null) return;

        // You could change UI colors, styles, or other elements based on the agent
        // For example, different NPCs could have different chat window colors

        // Example implementation:
        // if (chatPanel != null)
        // {
        //     Image panelImage = chatPanel.GetComponent<Image>();
        //     if (panelImage != null)
        //     {
        //         // Set different colors based on agent type or name
        //         panelImage.color = agent.GetAgentColor();
        //     }
        // }
    }
}

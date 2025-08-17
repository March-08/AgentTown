using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * SETUP INSTRUCTIONS:
 * 
 * 1. Add this NPCAgent component to any GameObject you want to be an NPC
 * 2. Make sure the GameObject has a Collider component
 * 3. Set up a ChatManager in your scene:
 *    - Create an empty GameObject and add the ChatManager script
 *    - Create a Canvas with Chat UI elements (Panel, Text, InputField, Buttons)
 *    - Assign the UI elements to the ChatManager's serialized fields
 *    - Optionally add the ChatUI component to the chat panel for animations
 * 4. Make sure your player has the Mover component attached
 * 5. NPCs will highlight yellow when the player is nearby and can be clicked to chat
 * 6. Floating name displays will automatically appear above each NPC showing their agentName
 * 
 * CUSTOMIZATION:
 * - Change agentName, dialogueLines, and interactionRange in the inspector
 * - Modify the GenerateNPCResponse method in ChatManager for more complex AI responses
 * - Customize the ChatUI animations and styling as needed
 * - NPCNameDisplay component is automatically added to show floating names above NPCs
 */

public class NPCAgent : MonoBehaviour
{
    [SerializeField] public string agentName = "NPC";
    [SerializeField] public string[] dialogueLines = { "Hello!", "How can I help you?", "Nice to meet you!" };
    [SerializeField] public float interactionRange = 3f;

    private bool isHighlighted = false;
    private Renderer agentRenderer;
    private Color originalColor;

    void Start()
    {
        agentRenderer = GetComponent<Renderer>();
        if (agentRenderer != null)
        {
            originalColor = agentRenderer.material.color;
        }

        // Automatically add NPCNameDisplay component if it doesn't exist
        NPCNameDisplay nameDisplay = GetComponent<NPCNameDisplay>();
        if (nameDisplay == null)
        {
            gameObject.AddComponent<NPCNameDisplay>();
            Debug.Log($"✅ Auto-added NPCNameDisplay to {agentName}");
        }
    }

    public void HighlightAgent(bool highlight)
    {
        if (agentRenderer == null) return;

        isHighlighted = highlight;
        if (highlight)
        {
            agentRenderer.material.color = Color.yellow;
        }
        else
        {
            agentRenderer.material.color = originalColor;
        }
    }

    public bool IsInteractable(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= interactionRange;
    }

    public string GetRandomDialogue()
    {
        if (dialogueLines.Length == 0) return "...";
        int randomIndex = Random.Range(0, dialogueLines.Length);
        return dialogueLines[randomIndex];
    }
}

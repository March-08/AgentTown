using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Mover : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float interactionCheckRadius = 5f;

    private List<NPCAgent> nearbyAgents = new List<NPCAgent>();
    private NPCAgent highlightedAgent = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckNearbyAgents();

        if (Input.GetMouseButton(0))
        {
            // Check if clicking on UI elements first
            if (IsPointerOverUI())
            {
                Debug.Log("🖱️ Click on UI detected, ignoring movement");
                return; // Don't process movement if clicking on UI
            }

            if (!HandleAgentClick())
            {
                MoveToCursor();
            }
        }

        UpdateAnimator();
    }

    private void MoveToCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit)
        {
            GetComponent<NavMeshAgent>().destination = hit.point;
        }
    }

    private void UpdateAnimator()
    {
        Vector3 velocity = GetComponent<NavMeshAgent>().velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        float speed = localVelocity.z;
        GetComponent<Animator>().SetFloat("forwardSpeed", speed);
    }

    private void CheckNearbyAgents()
    {
        // Clear previous nearby agents
        foreach (NPCAgent agent in nearbyAgents)
        {
            if (agent != null)
                agent.HighlightAgent(false);
        }
        nearbyAgents.Clear();

        // Find all NPCAgent objects in the scene
        NPCAgent[] allAgents = FindObjectsOfType<NPCAgent>();

        foreach (NPCAgent agent in allAgents)
        {
            if (agent.IsInteractable(transform.position))
            {
                nearbyAgents.Add(agent);
                agent.HighlightAgent(true);

                // Show name when close
                NPCNameDisplay nameDisplay = agent.GetComponent<NPCNameDisplay>();
                if (nameDisplay != null)
                {
                    nameDisplay.ShowName();
                }
            }
            else
            {
                // Hide name when far
                NPCNameDisplay nameDisplay = agent.GetComponent<NPCNameDisplay>();
                if (nameDisplay != null)
                {
                    nameDisplay.HideName();
                }
            }
        }
    }

    private bool HandleAgentClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            NPCAgent clickedAgent = hit.collider.GetComponent<NPCAgent>();
            if (clickedAgent != null && nearbyAgents.Contains(clickedAgent))
            {
                // Open chat with this agent
                ChatManager.Instance?.OpenChat(clickedAgent);
                return true; // Consumed the click
            }
        }

        return false; // Click not consumed, allow movement
    }

    // Check if the mouse pointer is over a UI element
    private bool IsPointerOverUI()
    {
        // Check if EventSystem exists
        if (EventSystem.current == null)
            return false;

        // Check if pointer is over UI
        return EventSystem.current.IsPointerOverGameObject();
    }
}

using UnityEngine;

public class NPCNameDisplay : MonoBehaviour
{
    [Header("Simple Name Display")]
    [SerializeField] private float heightOffset = 1.5f;

    private NPCAgent npcAgent;
    private GameObject nameTextObj;
    private bool isVisible = false;
    private Camera mainCamera;

    void Start()
    {
        npcAgent = GetComponent<NPCAgent>();
        if (npcAgent == null)
        {
            Debug.LogError($"NPCNameDisplay: No NPCAgent found on {gameObject.name}");
            Destroy(this);
            return;
        }

        mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        CreateSimpleNameText();
    }

    void CreateSimpleNameText()
    {
        // Create simple 3D TextMesh
        nameTextObj = new GameObject($"{npcAgent.agentName}_NameText");
        nameTextObj.transform.SetParent(transform);
        nameTextObj.transform.localPosition = Vector3.up * heightOffset;

        // Add TextMesh component
        TextMesh textMesh = nameTextObj.AddComponent<TextMesh>();
        textMesh.text = npcAgent.agentName;
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        // Make text smaller to fit character scale
        nameTextObj.transform.localScale = Vector3.one * 0.1f;

        // Start hidden
        nameTextObj.SetActive(false);

        Debug.Log($"✅ Created simple name text for {npcAgent.agentName}");
    }

    void Update()
    {
        if (nameTextObj == null || mainCamera == null) return;

        // Always face the camera (billboard effect)
        nameTextObj.transform.LookAt(mainCamera.transform);
        nameTextObj.transform.Rotate(0, 180, 0); // Flip text to face camera correctly
    }

    public void ShowName()
    {
        if (nameTextObj != null && !isVisible)
        {
            nameTextObj.SetActive(true);
            isVisible = true;
        }
    }

    public void HideName()
    {
        if (nameTextObj != null && isVisible)
        {
            nameTextObj.SetActive(false);
            isVisible = false;
        }
    }

    void OnDestroy()
    {
        if (nameTextObj != null)
        {
            Destroy(nameTextObj);
        }
    }
}

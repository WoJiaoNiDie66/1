// Assets/Scripts/Interactable Objects/Checkpoint.cs
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string checkpointName = "Checkpoint";

    [Header("Visuals")]
    [SerializeField] private Sprite checkpointSprite;

    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 0.4f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 90f;

    public string CheckpointName => checkpointName;
    public Vector3 TeleportPosition => transform.position;

    private GameObject visualObject;
    private SpriteRenderer spriteRenderer;
    private Vector3 visualStartPosition;
    
    private bool playerInRange = false;
    private bool isActivated = false;

    private void Start()
    {
        visualObject = new GameObject("CheckpointVisuals");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = Vector3.zero;

        visualStartPosition = visualObject.transform.localPosition;

        spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
        if (checkpointSprite != null)
            spriteRenderer.sprite = checkpointSprite;
    }

    private void Update()
    {
        AnimateVisuals();

        if (!playerInRange) return;

        if (!isActivated)
        {
            isActivated = true;
            CheckpointManager.Instance.RegisterCheckpoint(this);
            Debug.Log("Saved!");
        }

        // Fix: Prevent opening if it's already open, preventing double-execution bugs
        if (Input.GetKeyDown(KeyCode.T) && !CheckpointPanel.Instance.IsOpen)
        {
            CheckpointPanel.Instance.Open(this);
        }
    }

    private void AnimateVisuals()
    {
        if (visualObject == null) return;
        float newY = visualStartPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        visualObject.transform.localPosition = new Vector3(visualStartPosition.x, newY, visualStartPosition.z);
        visualObject.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "CwcPlayer_A0")
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "CwcPlayer_A0")
            playerInRange = false;
    }

    // Fix: Allows the Manager to manually clear this state
    public void ResetRange()
    {
        playerInRange = false;
    }
}
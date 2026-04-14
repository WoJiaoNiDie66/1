// Assets/Scripts/Interactable Objects/Checkpoint.cs
using System.Collections;
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
    [SerializeField] private float saveSpinMultiplier = 8f; // How much faster it spins on save
    [SerializeField] private float spinRecoveryTime = 1f;   // Time it takes to slow back down

    public string CheckpointName => checkpointName;
    public Vector3 TeleportPosition => transform.position;

    private GameObject visualObject;
    private SpriteRenderer spriteRenderer;
    private Vector3 visualStartPosition;
    
    private bool playerInRange = false;
    private bool isActivated = false;

    // --- NEW: Spin State Variables ---
    private float currentSpinSpeed;
    private Coroutine spinRoutine;

    private void Start()
    {
        // Initialize current speed to base speed
        currentSpinSpeed = spinSpeed;

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

        // Open Checkpoint Panel
        if (Input.GetKeyDown(KeyCode.F) && !CheckpointPanel.Instance.IsOpen)
        {
            CheckpointPanel.Instance.Open(this);
        }
    }

    private void AnimateVisuals()
    {
        if (visualObject == null) return;
        float newY = visualStartPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        visualObject.transform.localPosition = new Vector3(visualStartPosition.x, newY, visualStartPosition.z);
        
        // Use currentSpinSpeed instead of fixed spinSpeed
        visualObject.transform.Rotate(Vector3.up, currentSpinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "CwcPlayer_A0")
        {
            playerInRange = true;

            // Register first-time activation
            if (!isActivated)
            {
                isActivated = true;
                CheckpointManager.Instance.RegisterCheckpoint(this);
            }

            // ALWAYS save the game the moment the player walks into the trigger
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame(this);
                TriggerSaveEffect(); // Trigger visual feedback
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "CwcPlayer_A0")
        {
            playerInRange = false;
        }
    }

    // --- NEW: Save Feedback Logic ---
    private void TriggerSaveEffect()
    {
        if (spinRoutine != null) StopCoroutine(spinRoutine);
        spinRoutine = StartCoroutine(SaveSpinRoutine());
    }

    private IEnumerator SaveSpinRoutine()
    {
        float elapsed = 0f;
        float targetSpin = spinSpeed * saveSpinMultiplier;

        while (elapsed < spinRecoveryTime)
        {
            elapsed += Time.deltaTime;
            // Smoothly transition from the fast spin back down to the normal spin speed
            currentSpinSpeed = Mathf.Lerp(targetSpin, spinSpeed, elapsed / spinRecoveryTime);
            yield return null;
        }

        // Lock it exactly back to the default
        currentSpinSpeed = spinSpeed;
    }

    public void ResetRange()
    {
        playerInRange = false;
    }

    public void SetActivatedState(bool state)
    {
        isActivated = state;
    }
}
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
    [SerializeField] private float saveSpinMultiplier = 8f; 
    [SerializeField] private float spinRecoveryTime = 1f;   

    [Header("Spawn Settings")]
    [Tooltip("玩家传送的具体位置与朝向 (位置 T)")]
    [SerializeField] private Transform spawnPointT; 

    // 在 Checkpoint.cs 的 Header("Spawn Settings") 附近添加
    [Header("Map Optimization")]
    [SerializeField] public MapNode assignedMap; // 该存档点所属的地图节点

    public string CheckpointName => checkpointName;
    
    // 【恢复】使用位置 T 逻辑，如果没设 T 则用存档点中心
    public Vector3 TeleportPosition => spawnPointT != null ? spawnPointT.position : transform.position;
    public Quaternion TeleportRotation => spawnPointT != null ? spawnPointT.rotation : transform.rotation;

    private GameObject visualObject;
    private SpriteRenderer spriteRenderer;
    private Vector3 visualStartPosition;

    private bool playerInRange = false;
    private bool isActivated = false;

    private float currentSpinSpeed;
    private Coroutine spinRoutine;

    private void Start()
    {
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

        // 【保留】组员的 UI 面板逻辑
        if (Input.GetKeyDown(KeyCode.F) && CheckpointPanel.Instance != null && !CheckpointPanel.Instance.IsOpen)
        {
            CheckpointPanel.Instance.Open(this);
        }
    }

    private void AnimateVisuals()
    {
        if (visualObject == null) return;
        float newY = visualStartPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        visualObject.transform.localPosition = new Vector3(visualStartPosition.x, newY, visualStartPosition.z);
        visualObject.transform.Rotate(Vector3.up, currentSpinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 【恢复】严谨的玩家碰撞检测 (root 检测)
        bool isPlayer = other.CompareTag("Player") || 
                        (other.transform.root != null && other.transform.root.CompareTag("Player"));

        if (isPlayer)
        {
            playerInRange = true;

            if (!isActivated)
            {
                isActivated = true;
                if (CheckpointManager.Instance != null)
                    CheckpointManager.Instance.RegisterCheckpoint(this);
            }

            if (SaveManager.Instance != null)
            {
                // 防连触判定
                if (currentSpinSpeed <= spinSpeed + 1f) 
                {
                    SaveManager.Instance.SaveGame(this);
                    TriggerSaveEffect(); 

                    // =========================================================
                    // 【致命修复】把被组员删掉的复活指令加回来！
                    // =========================================================
                    if (EnemyManager.Instance != null)
                    {
                        EnemyManager.Instance.OnCheckpointRest();
                    }
                    else
                    {
                        Debug.LogError("<color=red>[严重错误]</color> 场景中没有 EnemyManager 实例！");
                    }

                    Debug.Log($"<color=green>[Checkpoint]</color> 激活成功并触发了怪物复活：{checkpointName}");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || 
                        (other.transform.root != null && other.transform.root.CompareTag("Player"));
        if (isPlayer) playerInRange = false;
    }

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
            currentSpinSpeed = Mathf.Lerp(targetSpin, spinSpeed, elapsed / spinRecoveryTime);
            yield return null;
        }
        currentSpinSpeed = spinSpeed;
    }

    public void ResetRange() => playerInRange = false;
    public void SetActivatedState(bool state) => isActivated = state;
}
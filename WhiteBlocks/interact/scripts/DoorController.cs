using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("門的模型")]
    public Transform door06; // 拖入 Door_06 (左門)
    public Transform door05; // 拖入 Door_05 (右門)

    [Header("開啟角度 (Y軸)")]
    [Tooltip("開啟時要增加的 Y 軸角度")]
    public float door06OpenAngle = -90f; 
    public float door05OpenAngle = 90f;  // 注意：雙扇門通常是一正一負，如果兩扇都要 -90 請自行修改

    public float openSpeed = 2f;
    public KeyCode interactKey = KeyCode.F;
    public bool playerInteractable = true; // 是否允許玩家互動

    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerInRange = false;

    // 紀錄關門時的初始角度
    private Quaternion door06ClosedRot;
    private Quaternion door05ClosedRot;

    void Start()
    {
        // 抓取一開始的預設角度
        if (door06 != null) door06ClosedRot = door06.localRotation;
        if (door05 != null) door05ClosedRot = door05.localRotation;
    }

    void Update()
    {
        // 玩家在范围内、门没在动、按下交互键(F1)
        if (playerInRange && !isMoving && Input.GetKeyDown(interactKey) && playerInteractable)
        {
            // 【核心修改】：不要自己开门了！去按那个“全自动存档总闸”！
            SaveableInteractable saveSys = GetComponent<SaveableInteractable>();
            if (saveSys != null)
            {
                saveSys.TriggerInteraction(); 
            }
            else
            {
                Debug.LogError("门上没挂 SaveableInteractable 脚本！");
            }
        }
    }

    IEnumerator ToggleDoorRoutine()
    {
        isMoving = true;
        isOpen = !isOpen;

        // 計算這次要轉到的目標角度
        Quaternion target06 = isOpen ? door06ClosedRot * Quaternion.Euler(0, door06OpenAngle, 0) : door06ClosedRot;
        Quaternion target05 = isOpen ? door05ClosedRot * Quaternion.Euler(0, door05OpenAngle, 0) : door05ClosedRot;

        float elapsed = 0;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            
            // 平滑旋轉
            if (door06 != null) door06.localRotation = Quaternion.Slerp(door06.localRotation, target06, elapsed);
            if (door05 != null) door05.localRotation = Quaternion.Slerp(door05.localRotation, target05, elapsed);
            
            yield return null;
        }

        // 確保精準到位
        if (door06 != null) door06.localRotation = target06;
        if (door05 != null) door05.localRotation = target05;

        isMoving = false;
    }

    public void TriggerFromLever()
    {
        if (!isMoving)
        {
            StartCoroutine(ToggleDoorRoutine());
        }
    }

    // --- 感應區設定 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    // 这个方法对应你截图里的 On Interact 事件
    public void OpenDoorSmoothly() 
    {
        // 调用你原本写的平滑旋转协程
        if (!isMoving) StartCoroutine(ToggleDoorRoutine());
    }

    // 这个方法对应你截图里的 On Load Already Interacted 事件
    public void InstantSnapToOpen()
    {
        // 瞬间归位，不播动画 (这里需要你自己之前的 closedRot 变量)
        if (door06 != null) door06.localRotation = door06ClosedRot * Quaternion.Euler(0, door06OpenAngle, 0);
        if (door05 != null) door05.localRotation = door05ClosedRot * Quaternion.Euler(0, door05OpenAngle, 0);
    }
}
using UnityEngine;
using System.Collections;

public class ChestController : MonoBehaviour
{
    [Header("寶箱部位")]
    [Tooltip("請拖入寶箱蓋子 (Chest_02)")]
    public Transform chestLid; 

    [Header("角度設定 (X軸)")]
    public float closedAngleX = 50f;     // 關閉時的角度
    public float openAngleX = -1.612f;   // 打開時的角度
    
    [Header("互動設定")]
    public float openSpeed = 3f;         // 開關的速度
    public KeyCode interactKey = KeyCode.E; // 互動按鍵

    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerInRange = false;

    // 紀錄蓋子原本的 Y 和 Z 角度，避免旋轉時歪掉
    private float defaultY;
    private float defaultZ;

    void Start()
    {
        if (chestLid != null)
        {
            // 記下模型原本的 Y 和 Z 軸角度
            defaultY = chestLid.localEulerAngles.y;
            defaultZ = chestLid.localEulerAngles.z;

            // 遊戲一開始，強制將寶箱設為「關閉」的角度
            chestLid.localRotation = Quaternion.Euler(closedAngleX, defaultY, defaultZ);
        }
        else
        {
            Debug.LogError("請記得把 Chest_02 拖入 Chest Lid 欄位！");
        }
    }

    void Update()
    {
        // 玩家在範圍內 + 蓋子沒在動 + 按下 E 鍵
        if (playerInRange && !isMoving && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(ToggleChest());
        }
    }

    IEnumerator ToggleChest()
    {
        isMoving = true;
        isOpen = !isOpen; // 反轉狀態

        Debug.Log(isOpen ? "【寶箱】打開" : "【寶箱】關閉");

        // 決定目標角度
        float targetAngleX = isOpen ? openAngleX : closedAngleX;
        
        // 設定起點與終點的四元數 (Quaternion)
        Quaternion startRot = chestLid.localRotation;
        Quaternion targetRot = Quaternion.Euler(targetAngleX, defaultY, defaultZ);

        float elapsed = 0f;

        // 執行平滑旋轉動畫
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            chestLid.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            yield return null;
        }

        // 確保最終角度精準到位
        chestLid.localRotation = targetRot;
        isMoving = false;
    }

    // --- 感應區設定 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("靠近寶箱，按 E 互動");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
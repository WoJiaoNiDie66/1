using UnityEngine;

public class CandleToggle : MonoBehaviour
{
    [Header("預設狀態")]
    [Tooltip("打勾代表一開始是亮的，取消打勾代表一開始是滅的")]
    public bool isOn = true; 

    [Header("要控制的特效物件")]
    public GameObject fireParticlesParent; // 放子物件的 candle_fire
    public GameObject leftEye;             // 放 left_eye
    public GameObject rightEye;            // 放 right_eye
    public Light fireLight;                // 放 Point Light

    private bool playerInRange = false;

    void Start()
    {
        // 遊戲一開始，根據 isOn 的預設值來決定要不要亮
        UpdateEffects();
    }

    void Update()
    {
        // 當玩家在範圍內，並且按下 E 鍵
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // 反轉狀態（開變關，關變開）
            isOn = !isOn; 
            
            // 更新畫面上的特效
            UpdateEffects();
            
            Debug.Log("蠟燭狀態切換：" + (isOn ? "點燃" : "熄滅"));
        }
    }

    // 這個方法專門用來統一開啟/關閉所有特效
    void UpdateEffects()
    {
        // SetActive 用來開啟/關閉整個 GameObject（包含它身上的粒子系統）
        if (fireParticlesParent != null) fireParticlesParent.SetActive(isOn);
        if (leftEye != null) leftEye.SetActive(isOn);
        if (rightEye != null) rightEye.SetActive(isOn);
        
        // enabled 用來開啟/關閉組件（這樣燈光就會直接亮起或熄滅）
        if (fireLight != null) fireLight.enabled = isOn;
    }

    // --- 玩家感應偵測 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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
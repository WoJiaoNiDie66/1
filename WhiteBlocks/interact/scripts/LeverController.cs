using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    [Header("組件設定")]
    private Animator anim;
    public UnityEvent MyEvent; // 這裡可以在 Inspector 中綁定任何方法
    
    [Header("狀態監測")]
    private bool isPulled = false; // 紀錄拉桿目前是在左(false)還是右(true)
    private bool playerInRange = false;

    [Header("CUSTOMIZE")]
    public float leverResetDelay = 2f; // 拉桿自動回復的延遲時間（秒）
    private float resetTimer = -1f;
    private bool isReady = false;

    void Start()
    {
        // 自動抓取掛在同一個物件上的 Animator
        anim = GetComponent<Animator>();

        if (anim == null)
        {
            Debug.LogError("錯誤：物件上找不到 Animator 組件！請確認腳本掛在 black lever 上。");
        }
        
        // 初始化拉桿狀態
        anim.SetBool("isPressB1", isPulled);

        isReady = true; // 確保在 Start 結束後才允許互動
    }

    void Update()
    {
        // 只有在「玩家在範圍內」且「按下 E 鍵」時觸發
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLever();
        }
    }

    void LateUpdate()
    {
        // 如果拉桿已經被拉下，開始倒數自動回復
        if (isPulled && resetTimer > 0f)
        {
            resetTimer -= Time.deltaTime; // 倒數計時

            if (resetTimer <= 0f)
            {
                // 時間到，重置拉桿狀態
                isPulled = false;
                anim.SetBool("isPressB1", isPulled); // 更新 Animator 參數
                Debug.Log("【拉桿自動回復】目前的 isPressB1 狀態為: " + isPulled);

                resetTimer = -1f; // 停止計時
            }
        }
    }

    void ToggleLever()
    {
        if (anim != null && isPulled == false) // 只有當拉桿在左邊時才允許切換
        {
            // 1. 切換拉桿狀態
            isPulled = true;
            resetTimer = leverResetDelay; // 開始倒數自動回復的計時器
            MyEvent?.Invoke(); // 觸發綁定的事件

            // 2. 將新狀態傳送給 Animator 裡的 isPressB1 參數
            anim.SetBool("isPressB1", isPulled);

            // 3. 在 Console 印出訊息方便除錯
            Debug.Log("【拉桿切換】目前的 isPressB1 狀態為: " + isPulled);
        }
    }

    // --- 玩家感應偵測 ---

    private void OnTriggerEnter(Collider other)
    {
        // 確保你的玩家角色 Tag 設為 "Player"
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("靠近拉桿：按 [E] 進行互動");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("離開拉桿範圍");
        }
    }
}
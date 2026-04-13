using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("互動設定")]
    public KeyCode interactKey = KeyCode.F; // 互動按鍵
    public bool canPickup = true; // 是否可以被拾取（可以在 Inspector 中控制）

    private bool playerInRange = false;

    void Update()
    {
        // 當玩家在範圍內，並且按下了 E 鍵
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            InteractAndVanish();
        }
    }

    // 核心邏輯：執行互動並消失
    void InteractAndVanish()
    {
        Debug.Log("【物品】拾取完成！物品即將消失。");

        // --- 這裡可以加入其他的拾取邏輯，例如： ---
        // 1. 加到玩家背包 ( InventoryManager.Instance.AddItem(...) )
        // 2. 增加金幣或血量
        // 3. 播放一個「獲得物品」的音效

        // 🌟 最終指令：銷毀整個 Game Object
        // 這會同時銷毀它底下的子物件（例如那個 Particle System）
        Destroy(gameObject);
    }

    public void TruePickup()
    {
        canPickup = true;
    }

    // Set Pickable
    public void SetPickable(bool value)
    {
        canPickup = value;
    }

    // --- 玩家感應偵測 ---

    private void OnTriggerEnter(Collider other)
    {
        // 確認碰到的是標籤為 "Player" 的物件
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("靠近物品：按 [E] 拾取");
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
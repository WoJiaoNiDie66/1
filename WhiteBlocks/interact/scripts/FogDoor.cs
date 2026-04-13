using UnityEngine;

public class FogDoor : MonoBehaviour
{
    [Header("霧門物件綁定")]
    [Tooltip("拖入負責顯示霧氣的子物件 (fog)")]
    public GameObject fogVisual; 
    
    [Tooltip("拖入負責擋人的實體 Collider (隱形牆)")]
    public BoxCollider invisibleWall; 

    [Header("互動設定")]
    public KeyCode interactKey = KeyCode.F;

    private bool playerInRange = false;
    private bool isOpened = false; // 防止重複觸發

    void Update()
    {
        // 如果玩家在範圍內、按下 E 鍵，且霧門還沒開過
        if (playerInRange && !isOpened && Input.GetKeyDown(interactKey))
        {
            PassThroughFog();
        }
    }

    void PassThroughFog()
    {
        isOpened = true;
        Debug.Log("【霧門】消散，玩家進入 Boss 戰區域！");

        // 1. 關閉霧氣的視覺特效 (讓 fog 物件消失)
        if (fogVisual != null) 
        {
            fogVisual.SetActive(false);
        }

        // 2. 關閉隱形牆 (讓玩家可以走過去)
        if (invisibleWall != null) 
        {
            invisibleWall.enabled = false;
        }

        // 如果你之後想加個「穿過霧門」的無敵動畫，可以寫在這裡！
    }

    // --- 玩家感應偵測 ---
    private void OnTriggerEnter(Collider other)
    {
        // 記得確認玩家的 Tag 是 "Player"
        if (!isOpened && other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("站在霧門前，按 E 穿越");
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
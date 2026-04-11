using UnityEngine;
using StarterAssets; // 確保能抓到你的玩家控制器
using System.Collections; // 為了使用協程

public class WaterfallClimb : MonoBehaviour
{
    [Header("瀑布攀爬設定")]
    [Tooltip("攀爬速度 (建議比梯子慢一點)")]
    public float climbSpeed = 2.0f; 
    public KeyCode interactKey = KeyCode.E;

    [Header("邊界設定 (必填)")]
    [Tooltip("請拖入代表瀑布最頂端的空物件")]
    public Transform topPoint;    
    [Tooltip("請拖入代表瀑布最底端(水面)的空物件")]
    public Transform bottomPoint; 

    [Header("🌟 上岸設定 (解決掉落問題) 🌟")]
    [Tooltip("請建立一個空物件放在上方安全的平台上，拖入這裡。玩家到達頂部會自動滑到這裡。")]
    public Transform onshorePoint; 
    [Tooltip("上岸翻爬動作的速度 (數字越大滑得越快)")]
    public float onshoreTransitionSpeed = 4.0f;

    private bool playerInRange = false;
    private bool isClimbing = false;
    private bool isTransitioningOnshore = false; // 紀錄是否正在執行上岸動作

    // 玩家組件
    private Transform playerTransform;
    private ThirdPersonController playerController;
    private CharacterController charController;

    // 用來鎖定玩家開始攀爬時的水平位置
    private float lockedX;
    private float lockedZ;

    void Update()
    {
        // 🛡️ 防呆：正在執行上岸翻爬時，禁用所有按鍵，防止玩家亂按掉下去
        if (isTransitioningOnshore) return;

        // 1. 偵測按鍵：切換攀爬狀態
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (isClimbing) StopClimbing();
            else StartClimbing();
        }

        // 2. 攀爬時的移動邏輯
        if (isClimbing && playerTransform != null)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            
            // 計算玩家預期的下一步高度
            float nextY = playerTransform.position.y + (verticalInput * climbSpeed * Time.deltaTime);

            // 🛡️ 邊界防呆 🛡️
            if (topPoint != null && bottomPoint != null)
            {
                // 強制鎖死在最高點與最低點之間
                nextY = Mathf.Clamp(nextY, bottomPoint.position.y, topPoint.position.y);

                // 🌟 核心修改：到達頂部時，自動啟動「上岸協程」 🌟
                if (nextY >= topPoint.position.y && verticalInput > 0)
                {
                    if (onshorePoint != null)
                    {
                        Debug.Log("【瀑布】已達頂部，開始安全上岸程序...");
                        // 強制將 Y 軸設為頂點 Y 軸，防止 Y 軸歪掉
                        playerTransform.position = new Vector3(lockedX, topPoint.position.y, lockedZ);
                        // 啟動翻爬協程
                        StartCoroutine(OnshoreTransitionRoutine());
                    }
                    else
                    {
                        Debug.LogWarning("【瀑布】已達頂部，但沒有設定 OnshorePoint，玩家直接釋放，可能會掉落！");
                        StopClimbing();
                    }
                    return; 
                }
                // 如果降到最下面，自動解除攀爬
                else if (nextY <= bottomPoint.position.y && verticalInput < 0)
                {
                    Debug.Log("【瀑布】已達底部，進入水中。");
                    StopClimbing();
                    return;
                }
            }

            // 更新位置。Y 軸跟著按鍵動，X 和 Z 軸死死鎖定在按 E 的那一瞬間！
            playerTransform.position = new Vector3(lockedX, nextY, lockedZ);
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        Debug.Log("【瀑布】開始逆流而上！");

        // 關閉玩家原本的移動與重力控制
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

        // 🌟 紀錄玩家按下 E 瞬間的世界水平座標
        if (playerTransform != null)
        {
            lockedX = playerTransform.position.x;
            lockedZ = playerTransform.position.z;
        }
    }

    // 將 StopClimbing 設為可以被協程呼叫
    void StopClimbing()
    {
        isClimbing = false;
        
        // 恢復玩家的移動與重力控制
        if (playerController != null) playerController.enabled = true;
        if (charController != null) charController.enabled = true;
    }

    // 🌟 核心：上岸翻爬協程 (平滑移動玩家到安全地板) 🌟
    IEnumerator OnshoreTransitionRoutine()
    {
        isTransitioningOnshore = true; // 鎖定玩家按鍵

        // 在整個協程執行期間，ThirdPersonController 和 CharacterController 依然是關閉狀態 (無重力)

        // 目標位置 (上岸點的世界座標)
        Vector3 targetOnshorePos = onshorePoint.position;

        // 為了確保玩家腳部能對齊 onshorePoint (有些模型中心點在腰部)，我們可能需要微調 onshorePoint 的 Y 軸高度在 Unity Inspector 裡
        
        // 使用 MoveTowards 平滑地移動玩家 Transform
        while (Vector3.Distance(playerTransform.position, targetOnshorePos) > 0.05f)
        {
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position, 
                targetOnshorePos, 
                onshoreTransitionSpeed * Time.deltaTime
            );
            yield return null; // 等待下一幀
        }

        // 確保精準到位
        playerTransform.position = targetOnshorePos;

        Debug.Log("【瀑布】安全上岸完成！");

        // 翻爬完成，釋放玩家
        StopClimbing();
        isTransitioningOnshore = false; // 解鎖玩家按鍵
    }

    // --- 瀑布感應區偵測 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
            playerController = other.GetComponent<ThirdPersonController>();
            charController = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // 如果意外離開感應區，強制作廢攀爬狀態 (上岸協程中有保護機制)
            if (isClimbing && !isTransitioningOnshore) StopClimbing();
            
            playerTransform = null;
            playerController = null;
            charController = null;
        }
    }
}
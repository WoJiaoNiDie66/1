using UnityEngine;
using StarterAssets; 

public class LadderClimb : MonoBehaviour
{
    [Header("攀爬設定")]
    public float climbSpeed = 3f;      
    public KeyCode interactKey = KeyCode.F; 

    [Header("對齊與邊界設定")]
    public bool snapToCenter = true; 
    
    [Tooltip("請拖入代表梯子最頂端的空物件")]
    public Transform topPoint;    
    [Tooltip("請拖入代表梯子最底端的空物件")]
    public Transform bottomPoint; 

    private bool playerInRange = false;
    private bool isClimbing = false;

    private Transform playerTransform;
    private ThirdPersonController playerController;
    private CharacterController charController;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (isClimbing) StopClimbing();
            else StartClimbing();
        }

        if (isClimbing && playerTransform != null)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            
            // 計算玩家預期的下一步高度
            float nextY = playerTransform.position.y + (verticalInput * climbSpeed * Time.deltaTime);

            // 🛡️ 邊界防呆機制 🛡️
            if (topPoint != null && bottomPoint != null)
            {
                // 1. 強制鎖死：不准超過最高點，也不准低於最低點 (防飛天遁地)
                nextY = Mathf.Clamp(nextY, bottomPoint.position.y, topPoint.position.y);

                // 2. 自動離開機制：如果已經推到最上面還繼續按 W，或推到最下面還繼續按 S
                if (nextY >= topPoint.position.y && verticalInput > 0)
                {
                    Debug.Log("【梯子】已達頂部，自動離開");
                    StopClimbing();
                    // 為了確保玩家能踩在上層地板，可以微調一點向前的位移
                    playerTransform.position += transform.forward * 0.8f; 
                    return; // 結束這一幀的計算
                }
                else if (nextY <= bottomPoint.position.y && verticalInput < 0)
                {
                    Debug.Log("【梯子】已達底部，自動離開");
                    StopClimbing();
                    return;
                }
            }

            // 更新玩家位置 (只改變 Y 軸，X 和 Z 保持吸附在梯子上)
            playerTransform.position = new Vector3(playerTransform.position.x, nextY, playerTransform.position.z);
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

        if (snapToCenter && playerTransform != null)
        {
            Vector3 newPos = playerTransform.position;
            newPos.x = transform.position.x; 
            newPos.z = transform.position.z;
            playerTransform.position = newPos;
        }
    }

    void StopClimbing()
    {
        isClimbing = false;
        if (playerController != null) playerController.enabled = true;
        if (charController != null) charController.enabled = true;
    }

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
            if (isClimbing) StopClimbing();
            playerTransform = null;
            playerController = null;
            charController = null;
        }
    }
}
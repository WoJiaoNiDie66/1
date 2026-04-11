using UnityEngine;

public class ElevatorPressurePlate : MonoBehaviour
{
    [Header("按鈕設定")]
    public Transform redButton; // 拖入 Button_Red
    [Tooltip("按鈕被踩下時，往下凹陷的距離")]
    public float buttonPressDepth = 0.05f; 
    public float buttonSpeed = 5f;

    [Header("電梯設定")]
    [Tooltip("電梯要上升的總高度")]
    public float liftRiseHeight = 10f; 
    public float liftSpeed = 2f;
    
    [Tooltip("離開壓力板時，電梯會自動降下來嗎？")]
    public bool autoReturnToBottom = false;

    // 紀錄座標
    private Vector3 buttonStartLocalPos;
    private Vector3 buttonPressedLocalPos;
    private Vector3 liftStartPos;
    private Vector3 liftTargetPos;

    // 狀態開關
    private bool isPressed = false;
    private bool moveToTop = false;

    void Start()
    {
        // 1. 初始化按鈕的位置 (使用 localPosition，這樣按鈕才會相對電梯移動)
        if (redButton != null)
        {
            buttonStartLocalPos = redButton.localPosition;
            buttonPressedLocalPos = buttonStartLocalPos - new Vector3(0, buttonPressDepth, 0);
        }

        // 2. 初始化電梯的位置
        liftStartPos = transform.position;
        liftTargetPos = liftStartPos + new Vector3(0, liftRiseHeight, 0);
    }

    void Update()
    {
        // --- 處理紅色按鈕的凹凸動畫 ---
        if (redButton != null)
        {
            Vector3 targetBtnPos = isPressed ? buttonPressedLocalPos : buttonStartLocalPos;
            redButton.localPosition = Vector3.MoveTowards(redButton.localPosition, targetBtnPos, buttonSpeed * Time.deltaTime);
        }

        // --- 處理電梯的升降動畫 ---
        Vector3 targetLiftPos = moveToTop ? liftTargetPos : liftStartPos;
        transform.position = Vector3.MoveTowards(transform.position, targetLiftPos, liftSpeed * Time.deltaTime);
    }

    // --- 玩家踩上壓力板 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPressed = true;   // 按鈕凹下
            moveToTop = true;   // 電梯開始上升
            
            // 將玩家設為電梯的子物件，防止玩家在電梯上升時掉下去或瘋狂抖動
            other.transform.SetParent(transform);
            
            Debug.Log("【電梯】踩到壓力板，電梯啟動！");
        }
    }

    // --- 玩家離開壓力板 ---
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPressed = false;  // 按鈕彈回
            
            if (autoReturnToBottom) 
            {
                moveToTop = false; // 如果開啟自動返回，玩家一離開電梯就會降下去
            }
            
            // 解除玩家與電梯的父子關係
            other.transform.SetParent(null);
        }
    }
}
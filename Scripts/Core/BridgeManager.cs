using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BridgeManager : MonoBehaviour
{
    public static BridgeManager Instance;
    [Header("阅读 UI 设置 (靠近按 F 呼出)")]
    //[Header("阅读 UI 设置 (靠近按 E 呼出)")]
    public GameObject readMessageUI;
    [SerializeField] private TextMeshProUGUI readContentText1;
    [SerializeField] private TextMeshProUGUI readContentText2;
    [SerializeField] private TextMeshProUGUI openMessageUI;
    //[SerializeField] private PlayerInput playerInput;

    private FirebaseFirestore db;

    void Awake() { Instance = this; }

    void Start()
    {
        // 游戏开始时隐藏所有 UI 并锁定鼠标
        if (readMessageUI != null) readMessageUI.SetActive(false);
        if (openMessageUI != null) openMessageUI.gameObject.SetActive(false);

        //LockCursor(true);

        // 监听输入框的回车键提交 (玩家打完字按 Enter 直接发送)

        // 初始化 Firebase
        //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        //    if (task.Result == DependencyStatus.Available)
        //    {
        //        db = FirebaseFirestore.DefaultInstance;
        //        Debug.Log("[System] Firebase connected, scanning for nearby runes...");

        //        // 确保有玩家引用后再加载区块
        //        if (playerTransform != null)
        //        {
        //            LoadRuneStonesInChunk(GetChunkId(playerTransform.position));
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Player Transform not assigned in RuneManager!");
        //        }
        //    }
        //});
    }

    public void OpenMessage(string text)
    {
        openMessageUI.text = text;
        openMessageUI.gameObject.SetActive(true);
    }

    public void CloseMessage()
    {
        openMessageUI.gameObject.SetActive(false);
    }

    // 供 RuneInteractable 靠近按 E 时调用
    public void OpenReadUI(string destinationName,int currentQuantity, int requiredQuantity)
    {
        if (readMessageUI == null) return;

        readContentText1.text = "Bridge to " + destinationName;
        readContentText2.text = "Coins: " + currentQuantity + "/" + requiredQuantity;

        readMessageUI.SetActive(true);
        LockCursor(false);
    }

    // 供阅读面板的“关闭”按钮调用
    public void CloseReadUI()
    {
        if (readMessageUI == null) return;

        readMessageUI.SetActive(false);
        LockCursor(true);
    }

    // 统一管理鼠标指针状态
    private void LockCursor(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }



}

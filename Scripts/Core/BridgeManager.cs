using Firebase.Extensions;
using Firebase.Firestore;
using Firebase;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class BridgeManager : MonoBehaviour
{
    public static BridgeManager Instance;

    public static UnityAction<int> OnCoinSubmitted;

    [Header("阅读 UI 设置 (靠近按 F 呼出)")]
    public GameObject readMessageUI;

    [SerializeField] 
    private TextMeshProUGUI readContentText1;
    
    [SerializeField] 
    private TextMeshProUGUI readContentText2;

    [SerializeField]
    private CoinSubmitUI coinSubmitUI;

    [SerializeField]
    private List<BridgeSignInteractable> signList;
    
    private Dictionary<string, GameObject> bridgeSigns = new Dictionary<string, GameObject>();

    [SerializeField] 
    private PlayerInput playerInput;

    [SerializeField]
    private PlayerMain_A0 playerMain;

    private FirebaseFirestore db;

    private BridgeSignInteractable currentBridgeSign;

    private void Awake() { 
        if(Instance == null)
            Instance = this; 
        OnCoinSubmitted += UpdateCoinsToSign;
    }

    void Start()
    {


        // 游戏开始时隐藏所有 UI 并锁定鼠标
        if (readMessageUI != null) readMessageUI.SetActive(false);
        
        if(signList == null)
        {
            Debug.LogError("BridgeManager: Bridge Signs are not assigned in the inspector.");
            return;
        }

        if(coinSubmitUI == null)
        {
            Debug.LogError("BridgeManager: CoinSubmitUI is not assigned in the inspector.");
            return;
        }

        coinSubmitUI.ClosePanel();

        signList.Sort();


        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Debug.Log(sceneName);

        // 初始化 Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                //Debug.Log("[System] Firebase connected, scanning for nearby runes...");

                for (int i = 0; i < signList.Count; i++)
                {
                    BridgeSignInteractable sign = signList[i];
                    int bridgeID = sign.BridgeID;
                    LoadBridgeSigns(bridgeID);
                }
            }
        });
    }

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

    public void SelectBridgeSign(int maxCoin, BridgeSignInteractable bridgeSign)
    {
        if (coinSubmitUI == null) return;
        this.currentBridgeSign = bridgeSign;
        maxCoin = Mathf.Min(maxCoin, playerMain.Coins);
        coinSubmitUI.OpenPanel(maxCoin);
        CoinSubmitUI.IsActive = true;
        playerInput.SwitchCurrentActionMap("Menu");
        LockCursor(false);
    }

    public void DeselectBridgeSign()
    {
        if (coinSubmitUI == null) return;
        currentBridgeSign = null;
        coinSubmitUI.ClosePanel();
        CoinSubmitUI.IsActive = false;
        playerInput.SwitchCurrentActionMap("Player");
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

    public void LoadBridgeSigns(int id)
    {
        Debug.Log("Finding bridge signs with ID: " + id);

        db.Collection("bridge").WhereEqualTo("bridgeID", id)
          .GetSnapshotAsync().ContinueWithOnMainThread((Task<QuerySnapshot> task) => {
              if (task.IsFaulted)
              {
                  Debug.LogError("Query failed: " + task.Exception);
                  return;
              }

              QuerySnapshot snapshot = task.Result;

              foreach (DocumentSnapshot doc in snapshot.Documents)
              {
                  if (bridgeSigns.ContainsKey(doc.Id)) continue;
                  Debug.Log(doc.Id);
                  //RuneStoneData data = doc.ConvertTo<RuneStoneData>();
                  BridgeData data = doc.ConvertTo<BridgeData>();
                  signList[id].Initialize(doc.Id, data);
                  //SpawnRuneStone(doc.Id, data);
              }
          });
    }

    public void UpdateCoinsToSign(int coin)
    {
        coinSubmitUI.ClosePanel();
        CoinSubmitUI.IsActive = false;

        if (currentBridgeSign == null)
        {
            Debug.LogError("BridgeManager: No bridge sign selected when trying to update coins.");
            return;
        }

        if(!currentBridgeSign.ValidateCoins(coin, out int remainder))
        {
            return;
        }

        DocumentReference docRef = db.Collection("bridge").Document(currentBridgeSign.DocumentId);
        docRef.UpdateAsync("CoinsStored", currentBridgeSign.CoinsStored);

        if (currentBridgeSign.IsBridgeReady())
        {
            currentBridgeSign.UpdateBridgeSign();
        }

        currentBridgeSign = null;
        playerInput.SwitchCurrentActionMap("Player");
        LockCursor(true);
    }

    //public void LikeRuneStone(string docId)
    //{
    //    DocumentReference docRef = db.Collection("runes").Document(docId);
    //    docRef.UpdateAsync("likes", FieldValue.Increment(1));
    //}

    //private void SpawnRuneStone(string docId, RuneStoneData data)
    //{
    //    Vector3 spawnPos = new Vector3(data.x, data.y, data.z);
    //    GameObject stone = Instantiate(runeStonePrefab, spawnPos, Quaternion.identity);

    //    stone.GetComponent<RuneInteractable>().Initialize(docId, data);
    //    spawnedStones.Add(docId, stone);
    //}

}

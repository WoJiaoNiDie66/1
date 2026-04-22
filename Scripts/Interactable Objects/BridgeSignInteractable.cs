using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BridgeSignInteractable : MonoBehaviour
{
    [SerializeField]
    private int requiredQuantity = 5;

    [SerializeField]
    private int currentQuantity = 0;
    [SerializeField]
    private string destinationName="";

    private string bridgeId;

    private bool isPlayerNearby = false;

    public void Initialize(BridgeData data)
    {
        bridgeId = data.Id;
        destinationName = data.DestinationName;
        requiredQuantity = data.RequiredQuantity;
        currentQuantity = data.CurrentQuantity;
    }

    // 玩家进入感应圈 (必须带 Player 标签)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            BridgeManager.Instance.OpenMessage("Interact (F)");
        }
    }

    // 玩家离开感应圈
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            BridgeManager.Instance.CloseMessage();

            //// 安全机制：如果玩家在阅读时直接走开，自动关闭 UI
            if (BridgeManager.Instance.readMessageUI != null && BridgeManager.Instance.readMessageUI.activeSelf)
            {
                BridgeManager.Instance.CloseReadUI();
            }
        }
    }

    void Update()
    {
        if (!isPlayerNearby || Keyboard.current == null) return;

        // 玩家按 F 键：呼出阅读 UI
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            BridgeManager.Instance.OpenReadUI(destinationName, currentQuantity, requiredQuantity);
        }


    }


}


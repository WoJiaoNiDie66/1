using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BridgeSignInteractable : MonoBehaviour, IComparable<BridgeSignInteractable>
{
    public string DocumentId;

    [SerializeField]
    private int coinsRequired = 5;

    [SerializeField]
    private int coinsStored = 0;
    [SerializeField]
    private string destinationName="";

    [SerializeField]
    private int bridgeID;

    [SerializeField]
    private GameObject bridge;

    private bool isPlayerNearby = false;

    public int BridgeID => bridgeID;

    public int CoinsRequired => coinsRequired;

    public int CoinsStored => coinsStored;

    public void Initialize(string docID,BridgeData data)
    {
        if (data == null)
        {
            Debug.LogError("BridgeSignInteractable: BridgeData is null. Initialization failed.");
            return;
        }

        Debug.Log(data.ToString());

        if(bridgeID != data.bridgeID)
        {
            Debug.LogError($"BridgeSignInteractable: Bridge ID mismatch. Expected {bridgeID}, but got {data.bridgeID}. Initialization failed.");
            return;
        }

        DocumentId = docID;
        destinationName = data.DestinationName;
        coinsRequired = data.CoinsRequired;
        coinsStored = data.CoinsStored;

        UpdateBridgeSign();
    }

    // 玩家进入感应圈 (必须带 Player 标签)
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (IsBridgeReady())
            {
                InteractableManager.Instance.OpenMessage($"Bridge is active!");
            }
            else
            {
                InteractableManager.Instance.OpenMessage("Interact (F)\r\n\r\nSend Coins (G)");
            }
        }
    }

    // 玩家离开感应圈
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            InteractableManager.Instance.CloseMessage();

            //// 安全机制：如果玩家在阅读时直接走开，自动关闭 UI
            if (BridgeManager.Instance.readMessageUI != null && BridgeManager.Instance.readMessageUI.activeSelf)
            {
                BridgeManager.Instance.CloseReadUI();
            }
        }
    }

    private void Update()
    {
        if (!isPlayerNearby || Keyboard.current == null || IsBridgeReady()) return;

        // 玩家按 F 键：呼出阅读 UI
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            BridgeManager.Instance.OpenReadUI(destinationName, coinsStored, coinsRequired);
        }

        if(Keyboard.current.gKey.wasPressedThisFrame && !CoinSubmitUI.IsActive)
        {
            BridgeManager.Instance.SelectBridgeSign(coinsRequired - coinsStored, this);
        }
        else if (Keyboard.current.gKey.wasPressedThisFrame && CoinSubmitUI.IsActive)
        {
            BridgeManager.Instance.DeselectBridgeSign();
        }


    }

    public bool IsBridgeReady()
    {
        if (coinsStored >= coinsRequired)
        {
            Debug.Log("Bridge is active! You can cross to " + destinationName);
            return true;
        }
        return false;
    }

    public bool ValidateCoins(int amountCoins,out int remainder)
    {
        remainder = 0;

        //Check if it is already enough to activate the bridge.
        if (coinsStored >= coinsRequired)
        {
            Debug.LogWarning("Bridge is now already active! You can cross to " + destinationName);
            return false;
        }

        //Check if the coins added are enough to activate the bridge.
        if (coinsStored + amountCoins >= coinsRequired)
        {
            remainder = coinsStored + amountCoins - coinsRequired;
            coinsStored = coinsRequired;
            Debug.Log("Bridge is now active! You can cross to " + destinationName);
            return true;
        }
        else
        {
            coinsStored += amountCoins;
            Debug.Log("Added " + amountCoins + " coins to the bridge. Current coins: " + coinsStored);
            return true;
        }


    }

    public void UpdateBridgeSign()
    {
        if(coinsStored >= coinsRequired)
        {
            Debug.Log("Bridge is Ready! You can cross to " + destinationName);
            bridge.SetActive(true);
        }
        else
        {
            bridge.SetActive(false);
        }
    }

    public int CompareTo(BridgeSignInteractable other)
    {
        if(other == null) return 1;
        else return bridgeID.CompareTo(other.bridgeID);
    }


}


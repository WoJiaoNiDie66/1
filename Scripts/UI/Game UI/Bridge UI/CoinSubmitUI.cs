using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinSubmitUI : MonoBehaviour
{
    public static bool IsActive = false;
    private int currentCoin = 0;
    private int maxCoin;


    [SerializeField]
    private TextMeshProUGUI coinText;

    public void OpenPanel(int maxCoin)
    {
        gameObject.SetActive(true);
        currentCoin = 0;
        this.maxCoin = maxCoin;
        coinText.text = currentCoin.ToString();
        IsActive = true;
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }

    public void IncrementCoin()
    {
        if (currentCoin < maxCoin)
        {
            currentCoin++;
        }
        else
        {
            currentCoin = 0;
        }
        coinText.text = currentCoin.ToString();
    }

    public void DecrementCoin()
    {
        if(currentCoin == 0)
        {
            currentCoin = maxCoin;
        }
        else
        {
            currentCoin--;
        }

        coinText.text = currentCoin.ToString();
    }

    public void onConfirm()
    {
        BridgeManager.OnCoinSubmitted?.Invoke(currentCoin);
    }

    public void onCancel()
    {
        BridgeManager.Instance.DeselectBridgeSign();
    }


}

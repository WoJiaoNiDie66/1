using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementUI : MonoBehaviour
{
    [SerializeField]
    private Image barImage;

    private bool isUpdating = false;

    private float remain;
    private float currentRemain;
    private float total;

    public void ResetUI()
    {
        barImage.fillAmount = 1;
        currentRemain = 1;
    }

    public void UpdateUI(float remain,float total)
    {
        this.remain = remain;
        this.total = total;
        isUpdating = true;
    }

    private void Update()
    {
        if (isUpdating)
        {
            currentRemain = Mathf.Lerp(currentRemain, remain, Time.deltaTime / 0.1f);
            barImage.fillAmount = currentRemain / total;
            if (Mathf.Approximately(currentRemain, remain))
            {
                isUpdating = false;
            }
        }
    }
}

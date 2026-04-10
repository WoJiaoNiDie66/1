using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementUI : MonoBehaviour
{
    [SerializeField]
    private Image barImage;

    public void ResetUI()
    {
        barImage.fillAmount = 1;
    }

    public void UpdateUI(float remain,float total)
    {
        barImage.fillAmount = (remain/ total);
    }
}

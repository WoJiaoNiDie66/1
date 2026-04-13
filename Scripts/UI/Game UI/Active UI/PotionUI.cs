using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionUI : MonoBehaviour
{
    /// <summary>
    /// potionSprites[0]: Empty Potion
    /// potionSprites[1]: 1/3 Potion
    /// potionSprites[2]: 2/3 Potion
    /// potionSprites[3]: Full Potion
    /// </summary>
    [SerializeField]
    private Sprite[] potionSprites;
    [SerializeField]
    private Image potionImage;

    public void UpdateUI(int count)
    {
        potionImage.sprite = potionSprites[count];
    }

    public void ResetUI()
    {
        potionImage.sprite = potionSprites[3];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : SelectionUI
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private ItemData item;

    /// <summary>
    /// There are three types of borders/slots.
    /// borderSprites[0] is the selected slot.
    /// borderSprites[1] is unselected and unlocked slot.
    /// borderSprites[2] is unselected and locked slot.
    /// </summary>
    [SerializeField]
    private Sprite[] borderSprites;
    [SerializeField]
    private bool equipped = false;

    public ItemData Item => item;
    public bool Equipped => equipped;

    protected override void Start()
    {
        base.Start();
        if (item != null)
        {
            itemImage.sprite = item.ItemSprite;
            RefreshVisual();
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
    }

    // Called every time the menu GameObject is enabled (opened)
    private void OnEnable()
    {
        if (item != null)
            RefreshVisual();
    }

    /// <summary>
    /// Reads the current unlock/equip state and applies the correct color.
    /// </summary>
    public void RefreshVisual()
    {
        if (item == null) return;

        if (!item.Unlocked)
            itemImage.color = Color.black;
        else if (equipped)
            itemImage.color = new Color(0.5f, 0.5f, 0.5f);
        else
            itemImage.color = Color.white;
    }

    public override void Highlight()
    {
        Debug.Log("Highlight");
        highlightImage.sprite = borderSprites[0];
        base.Highlight();
    }

    public override void UnHighlight()
    {
        if (item == null || !item.Unlocked)
            highlightImage.sprite = borderSprites[2];
        else
            highlightImage.sprite = borderSprites[1];

        if (!flashStopped)
        {
            flashStopped = true;
            StopFlashing();
            highlightImage.color = new Color(1, 1, 1, MAXALPHA);
        }
            
    }

    public void OnUnlock()
    {
        RefreshVisual();
    }

    public void SetEquipped(bool equipped)
    {
        this.equipped = equipped;
        RefreshVisual();
    }
}
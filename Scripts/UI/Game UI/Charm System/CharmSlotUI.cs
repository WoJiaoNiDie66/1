using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharmSlotUI : SelectionUI
{
    [SerializeField]
    private Image charmImage;
    [SerializeField]
    private Charm charm;

    [SerializeField]
    private bool equipped;

    /// <summary>
    /// There are three types of borders/slots.
    /// borderSprites[0] is the selected slot.
    /// borderSprites[1] is unselected and unlocked slot.
    /// borderSprites[2] is unselected and locked slot.
    /// </summary>
    [SerializeField]
    private Sprite[] borderSprites;

    public Charm Charm => charm;
    public bool Equipped => equipped;

    protected override void Start()
    {
        base.Start();
        if (charm != null)
        {
            charmImage.sprite = charm.CharmSprite;
            RefreshVisual();
        }
        else
        {
            charmImage.gameObject.SetActive(false);
        }
    }

    // Called every time the menu GameObject is enabled (opened)
    private void OnEnable()
    {
        if (charm != null)
            RefreshVisual();
    }

    /// <summary>
    /// Reads the current unlock/equip state and applies the correct color.
    /// </summary>
    public void RefreshVisual()
    {
        if (charm == null) return;

        if (!charm.Unlocked)
            charmImage.color = Color.black;
        else if (equipped)
            charmImage.color = new Color(0.5f, 0.5f, 0.5f);
        else
            charmImage.color = Color.white;
    }

    public override void Highlight()
    {
        highlightImage.sprite = borderSprites[0];
        base.Highlight();
    }

    public override void UnHighlight()
    {
        if (charm == null || !charm.Unlocked)
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

    public override void OnPointerEnter(PointerEventData eventData)
    {
        ParentSelector?.UIHover(this);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        ParentSelector?.UIClicked(this);
    }

    public void OnUnlock()
    {
        RefreshVisual();
    }

    public void OnEquipped()
    {
        equipped = true;
        RefreshVisual();
    }

    public void OnUnequipped()
    {
        equipped = false;
        RefreshVisual();
    }
}
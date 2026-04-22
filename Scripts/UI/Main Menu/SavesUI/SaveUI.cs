using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Displays a single game slot in the main menu.
/// </summary>
public class SaveUI : SelectionUI
{
    [SerializeField]
    private TextMeshProUGUI[] descs;

    /// <summary>
    /// borderSprites[0] = Selected Border,
    /// borderSprites[1] = Unselected Border.
    /// </summary>
    [SerializeField]
    private Sprite[] borderSprites = new Sprite[2];

    /// <summary>The slot index (0–2) this UI element represents.</summary>
    public int SlotIndex { get; private set; }

    public override void Highlight()
    {
        highlightImage.sprite = borderSprites[0];
        base.Highlight();
    }

    public override void UnHighlight()
    {
        highlightImage.sprite = borderSprites[1];
        base.UnHighlight();
    }

    /// <summary>
    /// Call this from SaveSelector after instantiation to populate slot info.
    /// </summary>
    public void InitializeUI(int slot)
    {
        SlotIndex = slot;

        if (descs == null || descs.Length == 0) return;

        SaveData data = SaveManager.PeekSlot(slot);

        if (data == null)
        {
            // Empty slot
            descs[0].text = $"Slot {slot + 1}";
            if (descs.Length > 1) descs[1].text = "New Game";
        }
        else
        {
            descs[0].text = $"Slot {slot + 1}";
            if (descs.Length > 1) descs[1].text = $"Checkpoint: {data.currentCheckpoint}";
        }
    }
}
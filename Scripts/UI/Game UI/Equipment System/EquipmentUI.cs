// Assets/Scripts/UI/Game UI/Equipment System/EquipmentUI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class is used for showing equipments details.
/// 
/// The indexes for ui are strict so DO NOT change the order of the order.
/// ui[0]: Equipment Slot for HELMENT
/// ui[1]: Equipment Slot for CHESTPLATE
/// ui[2]: Equipment Slot for LEGGINGS
/// ui[3]: Equipment Slot for BOOTS
/// ui[4]: Equipment Slot for WEAPON
/// </summary>
public class EquipmentUI : SelectorManager
{
    //This is used to indicate if you have equipped a piece of armor or not.
    [SerializeField]
    private Image[] highlightImages;

    /// <summary>
    /// highlightSprites[0-3]: Helmet sprites,
    /// highlightSprites[4-7]: Chestplate sprites,
    /// highlightSprites[8-11]: Leggings sprites,
    /// highlightSprites[12-15]: Boots sprites
    /// </summary>
    [SerializeField]
    private Sprite[] highlightSprites;

    private EquipmentSlotUI currentSlot;
    private bool pendingSync = false; // --- NEW: Sync flag

    protected override void Start()
    {
        currentSlot = uis[currentIndex] as EquipmentSlotUI;
        if (currentSlot == null) { 
            Debug.LogError("EquipmentUI can only have EquipmentSlotUI as child"); 
        }

        EquipmentManager.OnEquipmentEquipped += EquipItem;
        EquipmentManager.OnEquipmentUnequipped += UnequipItem;
        
        // --- NEW: Listen to the SaveManager ---
        InventorySaveBridge.OnInventoryStateApplied += TriggerSync;

        base.Start();

        // Run once on start in case it loaded before UI woke up
        TriggerSync();
    }

    protected virtual void OnDestroy()
    {
        EquipmentManager.OnEquipmentEquipped -= EquipItem;
        EquipmentManager.OnEquipmentUnequipped -= UnequipItem;
        InventorySaveBridge.OnInventoryStateApplied -= TriggerSync;
    }

    // --- NEW: Coroutine Sync System ---
    private void TriggerSync()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncCoroutine());
        }
        else
        {
            // If the UI is disabled when loading, we flag it to sync next time it enables
            pendingSync = true;
        }
    }

    private void OnEnable()
    {
        if (pendingSync)
        {
            StartCoroutine(SyncCoroutine());
        }
    }

    private IEnumerator SyncCoroutine()
    {
        // Wait until the end of the frame to ensure EquipmentManager is 100% awake!
        yield return new WaitForEndOfFrame();
        pendingSync = false;

        if (uis == null || uis.Length == 0) yield break;

        for (int i = 0; i < uis.Length; i++)
        {
            EquipmentSlotUI slotUI = uis[i] as EquipmentSlotUI;
            if (slotUI == null) continue;

            // Strict mapping based on your header notes
            EquipmentType type = (EquipmentType)i;
            
            // Check what the top-level EquipmentManager says the player is actually wearing
            EquippableItem currentlyEquipped = EquipmentManager.GetEquippedItem(type);

            if (currentlyEquipped != null)
            {
                if (slotUI.EquippedItem != currentlyEquipped)
                {
                    Debug.Log($"<color=cyan>[Save Sync]</color> Visually Equipping to Panel: {currentlyEquipped.name}");
                    slotUI.Equip(currentlyEquipped);
                }
            }
            else
            {
                if (slotUI.EquippedItem != null || slotUI.Equipped)
                {
                    Debug.Log($"<color=yellow>[Save Sync]</color> Visually Unequipping from Panel slot: {type}");
                    slotUI.Unequip();
                }
            }
        }

        // Refresh the highlighted borders
        UIHover();
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentSlot = uis[currentIndex] as EquipmentSlotUI;
            if(currentSlot == null)
            {
                Debug.LogError("EquipmentUI can only have EquipmentSlotUI as child");
                return;
            }
            
            if(currentSlot.EquippedItem != null)
            {
                EquipmentManager.OnEquipmentUnequipped.Invoke(currentSlot.EquippedItem);
            }
            else
            {
                Debug.Log("No item has equipped.");
            }
        }
        base.Update();
    }

    public override void UIHover()
    {
        if (uis == null || uis.Length == 0) return;
        currentSlot = uis[currentIndex] as EquipmentSlotUI;
        if (currentSlot == null) return;

        switch ((EquipmentType)currentIndex) {
            case EquipmentType.HELMET:
                if (currentSlot.Equipped) highlightImages[(int)EquipmentType.HELMET].sprite = highlightSprites[3];
                else highlightImages[(int)EquipmentType.HELMET].sprite = highlightSprites[1];
                break;
            case EquipmentType.CHESTPLATE:
                if (currentSlot.Equipped) highlightImages[(int)EquipmentType.CHESTPLATE].sprite = highlightSprites[7];
                else highlightImages[(int)EquipmentType.CHESTPLATE].sprite = highlightSprites[5];
                break;
            case EquipmentType.LEGGINGS:
                if (currentSlot.Equipped) highlightImages[(int)EquipmentType.LEGGINGS].sprite = highlightSprites[11];
                else highlightImages[(int)EquipmentType.LEGGINGS].sprite = highlightSprites[9];
                break;
            case EquipmentType.BOOTS:
                if (currentSlot.Equipped) highlightImages[(int)EquipmentType.BOOTS].sprite = highlightSprites[15];
                else highlightImages[(int)EquipmentType.BOOTS].sprite = highlightSprites[13];
                break;
        }

        for(int i = 0; i < uis.Length - 1; i++) // NOTE: Assumes Weapon is ignored for highlights based on original code loop condition
        {
            if (i == currentIndex) continue;
            EquipmentSlotUI slotUI = uis[i] as EquipmentSlotUI;
            if (slotUI == null) continue;

            switch ((EquipmentType)i)
            {
                case EquipmentType.HELMET:
                    if (slotUI.Equipped) highlightImages[(int)EquipmentType.HELMET].sprite = highlightSprites[2];
                    else highlightImages[(int)EquipmentType.HELMET].sprite = highlightSprites[0];
                    break;
                case EquipmentType.CHESTPLATE:
                    if (slotUI.Equipped) highlightImages[(int)EquipmentType.CHESTPLATE].sprite = highlightSprites[6];
                    else highlightImages[(int)EquipmentType.CHESTPLATE].sprite = highlightSprites[4];
                    break;
                case EquipmentType.LEGGINGS:
                    if (slotUI.Equipped) highlightImages[(int)EquipmentType.LEGGINGS].sprite = highlightSprites[10];
                    else highlightImages[(int)EquipmentType.LEGGINGS].sprite = highlightSprites[8];
                    break;
                case EquipmentType.BOOTS:
                    if (slotUI.Equipped) highlightImages[(int)EquipmentType.BOOTS].sprite = highlightSprites[14];
                    else highlightImages[(int)EquipmentType.BOOTS].sprite = highlightSprites[12];
                    break;
            }
        }

        base.UIHover();
    }

    public override void UIClicked(SelectionUI ui)
    {
        currentSlot = uis[currentIndex] as EquipmentSlotUI;
        if (currentSlot == null)
        {
            Debug.LogError("EquipmentUI can only have EquipmentSlotUI as child");
            return;
        }

        if (currentSlot.EquippedItem != null)
        {
            EquipmentManager.OnEquipmentUnequipped.Invoke(currentSlot.EquippedItem);
        }
        else
        {
            Debug.Log("No item has equipped.");
        }
    }

    private void EquipItem(EquippableItem item)
    {
        if (uis == null || uis.Length == 0) return;

        int id = (int)item.EquipmentType;
        EquipmentSlotUI slotUI = uis[id] as EquipmentSlotUI;
        if(slotUI == null)
        {
            Debug.LogError("EquipmentUI can only have EquipmentSlotUI as child");
            return;
        }
        if (slotUI.EquippedItem == null)
        {
            slotUI.Equip(item);
        }
        else
        {
            EquippableItem unequipItem = slotUI.EquippedItem;
            slotUI.Equip(item);
            EquipmentManager.OnEquipmentSwapped.Invoke(unequipItem);
        }
    }

    private void UnequipItem(EquippableItem item) 
    {
        if (uis == null || uis.Length == 0) return;

        int id = (int)item.EquipmentType;
        EquipmentSlotUI slotUI = uis[id] as EquipmentSlotUI;
        if (slotUI != null)
        {
            if(slotUI.EquippedItem == item)
            {
                Debug.Log("Unequipped Item");
                slotUI.Unequip();
            }
            else
            {
                slotUI.Equip(item);
                switch (item.EquipmentType)
                {
                    case EquipmentType.HELMET:
                        highlightImages[(int)EquipmentType.HELMET].sprite = highlightSprites[2];
                        break;
                    case EquipmentType.CHESTPLATE:
                        highlightImages[(int)EquipmentType.CHESTPLATE].sprite = highlightSprites[6];
                        break;
                    case EquipmentType.LEGGINGS:
                        highlightImages[(int)EquipmentType.LEGGINGS].sprite = highlightSprites[10];
                        break;
                    case EquipmentType.BOOTS:
                        highlightImages[(int)EquipmentType.BOOTS].sprite = highlightSprites[14];
                        break;
                }
            }
        }
    }
}
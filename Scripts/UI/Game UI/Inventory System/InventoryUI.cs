// Assets/Scripts/UI/Game UI/Inventory System/InventoryUI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : SelectorManager
{
    [SerializeField]
    private ItemDescriptor itemDescriptor;

    private bool pendingSync = false;

    protected override void Start()
    {
        EquipmentManager.OnEquipmentUnequipped += UnequipItem;
        EquipmentManager.OnEquipmentSwapped += UnequipItem;
        
        // --- NEW: Listen to the SaveManager ---
        InventorySaveBridge.OnInventoryStateApplied += TriggerSync;
        
        base.Start();
        UIHover();
        
        // Run it once on Start just in case it loaded before this script woke up
        TriggerSync();
    }

    protected virtual void OnDestroy()
    {
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
            var slotUI = uis[i] as ItemSlotUI;
            if (slotUI == null || slotUI.Item == null) continue;

            EquippableItem equippable = slotUI.Item as EquippableItem;
            if (equippable != null)
            {
                // Check what the top-level EquipmentManager says the player is actually wearing
                EquippableItem currentlyEquipped = EquipmentManager.GetEquippedItem(equippable.EquipmentType);
                
                if (currentlyEquipped == equippable)
                {
                    // The player is wearing this item. Force the UI to show it as Equipped!
                    if (!slotUI.Equipped)
                    {
                        Debug.Log($"<color=cyan>[Save Sync]</color> Visually Equipping Item: {equippable.name}");
                        slotUI.SetEquipped(true);
                    }
                }
                else
                {
                    // The player is NOT wearing this item. Clean it up just in case.
                    if (slotUI.Equipped)
                    {
                        Debug.Log($"<color=yellow>[Save Sync]</color> Visually Unequipping Item: {equippable.name}");
                        slotUI.SetEquipped(false);
                    }
                }
            }
        }

        // Refresh the description text to match the newly loaded state
        UIHover(); 
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var slotUI = uis[currentIndex] as ItemSlotUI;
            if (slotUI == null)
            {
                Debug.LogError("The Inventory must only have ItemSlotUI as child");
                return;
            }

            if (!slotUI.Item.Unlocked)
            {
                Debug.LogError("Item is not unlocked");
                return;
            }

            var equippableItem = slotUI.Item as EquippableItem;

            if (equippableItem != null)
            {
                // Equip Item
                if (!slotUI.Equipped)
                {
                    Debug.Log("Equipping Item");
                    EquipmentManager.OnEquipmentEquipped.Invoke(equippableItem);
                    slotUI.SetEquipped(true);
                }
                else
                {
                    Debug.Log("Unequipping Item");
                    EquipmentManager.OnEquipmentUnequipped.Invoke(equippableItem);
                }
            }

            UIHover();
        }

        base.Update();
    }

    public void OnItemUnselected()
    {
        itemDescriptor.gameObject.SetActive(false);
    }

    public override void UIHover()
    {
        if (uis == null || uis.Length == 0) return;

        for (int i = 0; i < uis.Length; i++)
        {
            if(i != currentIndex)
            {
                uis[i].UnHighlight();
            }
            else
            {
                uis[i].Highlight();
            }
        }

        var slotUI = uis[currentIndex] as ItemSlotUI;
        if (slotUI == null)
        {
            Debug.LogError("The child must be InventorySlotUI");
            return;
        }

        if (slotUI.Item == null)
        {
            itemDescriptor.ResetDescription();
        }
        else if(!slotUI.Item.Unlocked)
        {
            bool equippable = slotUI.Item is EquippableItem;
            itemDescriptor.ResetDescription(
                                        slotUI.Item.ItemType,
                                        slotUI.Item.Unlocked,
                                        equippable
                                        ); 
        }
        else if (slotUI.Item.Unlocked)
        {
            itemDescriptor.SetDescription(
                                        slotUI.Item,
                                        slotUI.Equipped
                                        );
        }
        else
        {
            Debug.LogWarning("Slot selected but Item cannot be null.");
        }
    }

    public override void UIHover(SelectionUI UI)
    {
        var slotUI = UI as ItemSlotUI;
        if (slotUI == null)
        {
            Debug.LogError("The child must be InventorySlotUI");
            return;
        }

        for(int i = 0; i < uis.Length; i++)
        {
            if (uis[i] == UI)
            {
                currentIndex = i;
                uis[i].Highlight();
            }
            else
            {
                uis[i].UnHighlight();
            }
        }

        if (slotUI.Item != null)
        {
            itemDescriptor.SetDescription(
                                        slotUI.Item,
                                        slotUI.Equipped
                                        );
        }
    }

    public override void UIClicked(SelectionUI ui)
    {
        var slotUI = ui as ItemSlotUI;
        if (slotUI == null)
        {
            Debug.LogError("The Inventory must only have ItemSlotUI as child");
            return;
        }
        if (!slotUI.Item.Unlocked)
        {
            Debug.Log("Item is not unlocked.");
            return;
        }

        var equippableItem = slotUI.Item as EquippableItem;

        if (equippableItem != null)
        {
            // Equip Item
            if (!slotUI.Equipped)
            {
                Debug.Log("Equipping Item");
                EquipmentManager.OnEquipmentEquipped.Invoke(equippableItem);
                slotUI.SetEquipped(true);
            }
            else
            {
                Debug.Log("Unequipping Item");
                EquipmentManager.OnEquipmentUnequipped.Invoke(equippableItem);
            }
        }

        UIHover();
    }

    private void UnequipItem(EquippableItem item)
    {
        if (uis == null) return;
        
        for (int i = 0; i < uis.Length; i++)
        {
            ItemSlotUI slotUI = uis[i] as ItemSlotUI;
            if (slotUI != null)
            {
                EquippableItem eItem = slotUI.Item as EquippableItem;
                if (eItem != null)
                {
                    if(eItem == item)
                    {
                        slotUI.SetEquipped(false);
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        UIHover();
    }
}
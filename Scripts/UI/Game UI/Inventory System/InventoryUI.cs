using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : SelectorManager
{
    [SerializeField]
    private ItemDescriptor itemDescriptor;

    protected override void Start()
    {
        EquipmentManager.OnEquipmentUnequipped += UnequipItem;
        EquipmentManager.OnEquipmentSwapped += UnequipItem;
        base.Start();
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
            }

            if (!slotUI.Item.Unlocked)
            {
                Debug.LogError("Item is not unlocked");
            }

            var equippableItem = slotUI.Item as EquippableItem;

            if (equippableItem != null)
            {
                //Equip Item
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
        //Debug.Log($"Item Slot ID:{currentIndex}");
        base.Update();
    }

    public void OnItemUnselected()
    {
        itemDescriptor.gameObject.SetActive(false);
    }

    public override void UIHover()
    {

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

        //Debug.Log($"Item Slot ID:{currentIndex}");

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
        else if(!slotUI.Item.Unlocked){
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
        }
        if (!slotUI.Item.Unlocked)
        {
            Debug.Log("Item is not unlocked.");
            return;
        }

        var equippableItem = slotUI.Item as EquippableItem;

        if (equippableItem != null)
        {
            //Equip Item
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

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class EquipmentManager : MonoBehaviour
{
    public static UnityAction<EquippableItem> OnEquipmentEquipped;
    public static UnityAction<EquippableItem> OnEquipmentUnequipped;
    public static UnityAction<EquippableItem> OnEquipmentSwapped;

    private static readonly Dictionary<EquipmentType, EquippableItem> equippedItems =
        new Dictionary<EquipmentType, EquippableItem>();

    public static float IncomingDamageMultiplier { get; private set; } = 1f;
    public static float OutgoingDamageMultiplier { get; private set; } = 1f;

    private void OnEnable()
    {
        OnEquipmentEquipped += HandleEquipmentEquipped;
        OnEquipmentUnequipped += HandleEquipmentUnequipped;
    }

    private void OnDisable()
    {
        OnEquipmentEquipped -= HandleEquipmentEquipped;
        OnEquipmentUnequipped -= HandleEquipmentUnequipped;
    }

    private void HandleEquipmentEquipped(EquippableItem item)
    {
        if (item == null) return;

        equippedItems[item.EquipmentType] = item;
        RecalculateEquipmentEffects();
    }

    private void HandleEquipmentUnequipped(EquippableItem item)
    {
        if (item == null) return;

        if (equippedItems.TryGetValue(item.EquipmentType, out EquippableItem equippedItem))
        {
            if (equippedItem == item)
            {
                equippedItems.Remove(item.EquipmentType);
                RecalculateEquipmentEffects();
            }
        }
    }

    private static void RecalculateEquipmentEffects()
    {
        IncomingDamageMultiplier = 1f;
        OutgoingDamageMultiplier = 1f;

        foreach (var kvp in equippedItems)
        {
            EquippableItem item = kvp.Value;
            if (item == null) continue;

            switch (item.ItemID)
            {
                // Leather set
                case 0: // Leather Helmet
                    IncomingDamageMultiplier *= 0.95f; // 5% reduction
                    break;
                case 1: // Leather Chestplate
                    IncomingDamageMultiplier *= 0.90f; // 10% reduction
                    break;
                case 2: // Leather Pants
                    IncomingDamageMultiplier *= 0.92f; // 8% reduction
                    break;
                case 3: // Leather Boots
                    IncomingDamageMultiplier *= 0.95f; // 5% reduction
                    break;

                // Iron set
                case 4: // Iron Helmet
                    IncomingDamageMultiplier *= 0.90f; // 10% reduction
                    break;
                case 5: // Iron Chestplate
                    IncomingDamageMultiplier *= 0.82f; // 18% reduction
                    break;
                case 6: // Iron Pants
                    IncomingDamageMultiplier *= 0.86f; // 14% reduction
                    break;
                case 7: // Iron Boots
                    IncomingDamageMultiplier *= 0.92f; // 8% reduction
                    break;

                // Weapons
                case 8: // Stone Sword
                    OutgoingDamageMultiplier *= 2f; // 2x more damage
                    break;
                case 9: // Iron Sword
                    OutgoingDamageMultiplier *= 3f; // 3x more damage
                    break;
            }
        }
    }

    public static EquippableItem GetEquippedItem(EquipmentType type)
    {
        if (equippedItems.TryGetValue(type, out EquippableItem item))
        {
            return item;
        }

        return null;
    }
}
// Assets/Scripts/Core/InventorySaveBridge.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySaveBridge : MonoBehaviour
{
    public static InventorySaveBridge Instance { get; private set; }
    public static event Action OnInventoryStateApplied;

    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    [SerializeField] private bool resetAllFlagsOnAwake = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (resetAllFlagsOnAwake) ResetAllRuntimeUnlockedFlags();
    }

    public List<string> GetUnlockedItemIds()
    {
        List<string> result = new List<string>();
        foreach (var item in allItems)
        {
            if (item != null && ScriptableObjectRuntimeSaveUtil.GetUnlocked(item))
            {
                string id = ScriptableObjectRuntimeSaveUtil.GetId(item);
                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
        }
        return result;
    }

    public List<string> GetEquippedItemIds()
    {
        List<string> result = new List<string>();

        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            EquippableItem equippedItem = EquipmentManager.GetEquippedItem(type);
            if (equippedItem != null)
            {
                string id = ScriptableObjectRuntimeSaveUtil.GetId(equippedItem);
                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) 
                    result.Add(id);
            }
        }
        return result;
    }

    public void LoadState(List<string> savedUnlocked, List<string> savedEquipped)
    {
        HashSet<string> unlockedSet = new HashSet<string>(savedUnlocked ?? new List<string>());
        HashSet<string> equippedSet = new HashSet<string>(savedEquipped ?? new List<string>());

        // 1. Unequip everything currently equipped to clear ghost stats from old saves
        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            EquippableItem currentItem = EquipmentManager.GetEquippedItem(type);
            if (currentItem != null)
            {
                EquipmentManager.OnEquipmentUnequipped?.Invoke(currentItem);
            }
        }

        // 2. Re-apply states
        foreach (var item in allItems)
        {
            if (item == null) continue;

            string id = ScriptableObjectRuntimeSaveUtil.GetId(item);
            
            // Set Unlocked state (Now correctly searches BaseType inheritance)
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(item, unlockedSet.Contains(id));

            if (equippedSet.Contains(id) && item is EquippableItem equippable)
            {
                EquipmentManager.OnEquipmentEquipped?.Invoke(equippable);
            }
        }
        OnInventoryStateApplied?.Invoke();
    }

    // --- FIX: Clear Static dictionary alongside ScriptableObjects ---
    public void ResetAllRuntimeUnlockedFlags()
    {
        // 1. Unequip everything from EquipmentManager to clear the static memory dictionary
        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            EquippableItem currentItem = EquipmentManager.GetEquippedItem(type);
            if (currentItem != null)
            {
                EquipmentManager.OnEquipmentUnequipped?.Invoke(currentItem);
            }
        }

        // 2. Reset the ScriptableObjects Unlocked flags (Will now successfully find the variable)
        foreach (var item in allItems)
        {
            if (item == null) continue;
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(item, false);
        }
        
        OnInventoryStateApplied?.Invoke();
    }
}
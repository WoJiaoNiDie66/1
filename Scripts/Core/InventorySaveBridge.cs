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
        foreach (var item in allItems)
        {
            if (item != null && ScriptableObjectRuntimeSaveUtil.GetEquipped(item))
            {
                string id = ScriptableObjectRuntimeSaveUtil.GetId(item);
                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
        }
        return result;
    }

    public void LoadState(List<string> savedUnlocked, List<string> savedEquipped)
    {
        HashSet<string> unlockedSet = new HashSet<string>(savedUnlocked ?? new List<string>());
        HashSet<string> equippedSet = new HashSet<string>(savedEquipped ?? new List<string>());

        foreach (var item in allItems)
        {
            if (item == null) continue;

            string id = ScriptableObjectRuntimeSaveUtil.GetId(item);
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(item, unlockedSet.Contains(id));
            
            bool isEquipped = equippedSet.Contains(id);
            bool wasEquipped = ScriptableObjectRuntimeSaveUtil.GetEquipped(item);
            ScriptableObjectRuntimeSaveUtil.SetEquipped(item, isEquipped);

            // Re-trigger the core EquipmentManager to recalculate damage multipliers
            if (item is EquippableItem equippable)
            {
                if (isEquipped) EquipmentManager.OnEquipmentEquipped?.Invoke(equippable);
                else if (wasEquipped) EquipmentManager.OnEquipmentUnequipped?.Invoke(equippable);
            }
        }
        OnInventoryStateApplied?.Invoke();
    }

    public void ResetAllRuntimeUnlockedFlags()
    {
        foreach (var item in allItems)
        {
            if (item == null) continue;
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(item, false);
            ScriptableObjectRuntimeSaveUtil.SetEquipped(item, false);
        }
    }
}
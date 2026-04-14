// Assets/Scripts/Core/CharmSaveBridge.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharmSaveBridge : MonoBehaviour
{
    public static CharmSaveBridge Instance { get; private set; }
    public static event Action OnCharmStateApplied;

    [SerializeField] private List<Charm> allCharms = new List<Charm>();
    [SerializeField] private bool resetAllFlagsOnAwake = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (resetAllFlagsOnAwake) ResetAllRuntimeUnlockedFlags();
    }

    public List<string> GetUnlockedCharmIds()
    {
        List<string> result = new List<string>();
        foreach (var charm in allCharms)
        {
            if (charm != null && ScriptableObjectRuntimeSaveUtil.GetUnlocked(charm))
            {
                string id = ScriptableObjectRuntimeSaveUtil.GetId(charm);
                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
        }
        return result;
    }

    // --- FIX: Read straight from CharmInventoryUI ---
    public List<CharmSaveData> GetEquippedCharms()
    {
        List<CharmSaveData> result = new List<CharmSaveData>();
        
        // Find the UI in the scene (even if it is currently disabled/hidden)
        CharmInventoryUI charmUI = FindObjectOfType<CharmInventoryUI>(true);

        if (charmUI != null)
        {
            List<Charm> equippedCharms = charmUI.GetEquippedCharms();
            if (equippedCharms != null)
            {
                foreach (Charm charm in equippedCharms)
                {
                    if (charm != null)
                    {
                        result.Add(new CharmSaveData 
                        { 
                            charmId = ScriptableObjectRuntimeSaveUtil.GetId(charm), 
                            slotId = charm.EquippedSlotID 
                        });
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("CharmInventoryUI could not be found in the scene! Equipped charms were not saved.");
        }

        return result;
    }

    public void LoadState(List<string> savedUnlocked, List<CharmSaveData> savedEquipped)
    {
        HashSet<string> unlockedSet = new HashSet<string>(savedUnlocked ?? new List<string>());
        
        Dictionary<string, int> equipMap = new Dictionary<string, int>();
        if (savedEquipped != null)
        {
            foreach (var data in savedEquipped)
                if (!string.IsNullOrEmpty(data.charmId)) equipMap[data.charmId] = data.slotId;
        }

        foreach (var charm in allCharms)
        {
            if (charm == null) continue;

            string id = ScriptableObjectRuntimeSaveUtil.GetId(charm);
            
            // Set Unlocked state
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(charm, unlockedSet.Contains(id));

            // Set Equipped state
            if (equipMap.TryGetValue(id, out int slotId))
            {
                charm.SetEquipped(true);
                charm.SetSlotID(slotId);
                
                // Optional: If you have a CharmManager action you want to fire on load, it would be:
                // CharmManager.OnCharmEquipped?.Invoke(charm);
            }
            else
            {
                charm.SetEquipped(false);
                charm.SetSlotID(-1);
            }
        }
        OnCharmStateApplied?.Invoke();
    }

    public void ResetAllRuntimeUnlockedFlags()
    {
        foreach (var charm in allCharms)
        {
            if (charm == null) continue;
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(charm, false);
            charm.SetEquipped(false);
            charm.SetSlotID(-1);
        }
    }
}
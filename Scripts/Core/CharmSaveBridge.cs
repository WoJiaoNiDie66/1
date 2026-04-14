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

    public List<string> GetEquippedCharmIds()
    {
        List<string> result = new List<string>();
        
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
                        result.Add(ScriptableObjectRuntimeSaveUtil.GetId(charm));
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

    public void LoadState(List<string> savedUnlocked, List<string> savedEquipped)
    {
        HashSet<string> unlockedSet = new HashSet<string>(savedUnlocked ?? new List<string>());
        HashSet<string> equippedSet = new HashSet<string>(savedEquipped ?? new List<string>());

        CharmInventoryUI charmUI = FindObjectOfType<CharmInventoryUI>(true);

        foreach (var charm in allCharms)
        {
            if (charm == null) continue;

            string id = ScriptableObjectRuntimeSaveUtil.GetId(charm);
            
            // Set Unlocked state
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(charm, unlockedSet.Contains(id));

            // Delegate equipping back to the UI Manager so abilities and costs apply correctly
            if (equippedSet.Contains(id))
            {
                charm.SetEquipped(true); // Still update the SO state
                if (charmUI != null)
                    charmUI.ForceEquipCharmFromSave(charm);
            }
            else
            {
                charm.SetEquipped(false); // Still update the SO state
                if (charmUI != null)
                    charmUI.ForceUnequipCharmFromSave(charm);
            }
        }
        OnCharmStateApplied?.Invoke();
    }

    public void ResetAllRuntimeUnlockedFlags()
    {
        CharmInventoryUI charmUI = FindObjectOfType<CharmInventoryUI>(true);

        foreach (var charm in allCharms)
        {
            if (charm == null) continue;
            
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(charm, false);
            charm.SetEquipped(false);

            // Cleanly force-unequip in UI to clear old costs/effects from previous saves
            if (charmUI != null)
                charmUI.ForceUnequipCharmFromSave(charm);
        }
    }
}
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
        
        // We only grab what the SOs currently say
        foreach (var charm in allCharms)
        {
            if (charm != null && charm.Equipped)
            {
                result.Add(ScriptableObjectRuntimeSaveUtil.GetId(charm));
            }
        }
        
        return result;
    }

    public void LoadState(List<string> savedUnlocked, List<string> savedEquipped)
    {
        HashSet<string> unlockedSet = new HashSet<string>(savedUnlocked ?? new List<string>());
        HashSet<string> equippedSet = new HashSet<string>(savedEquipped ?? new List<string>());

        foreach (var charm in allCharms)
        {
            if (charm == null) continue;

            string id = ScriptableObjectRuntimeSaveUtil.GetId(charm);
            
            // Re-apply states purely to the ScriptableObjects. The UI will pull from this when it enables.
            ScriptableObjectRuntimeSaveUtil.SetUnlocked(charm, unlockedSet.Contains(id));
            charm.SetEquipped(equippedSet.Contains(id));
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
        }
    }
}
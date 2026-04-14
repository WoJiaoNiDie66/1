// Assets/Scripts/Core/SkillSaveBridge.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSaveBridge : MonoBehaviour
{
    public static SkillSaveBridge Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadState(List<string> savedSkillIds)
    {
        if (savedSkillIds == null || savedSkillIds.Count == 0) return;
        StartCoroutine(SyncSkillsCoroutine(savedSkillIds));
    }

    private IEnumerator SyncSkillsCoroutine(List<string> savedSkillIds)
    {
        yield return new WaitForEndOfFrame();

        HashSet<string> unlockedSet = new HashSet<string>(savedSkillIds);
        SkillNodeUI[] allNodes = FindObjectsOfType<SkillNodeUI>(true);

        // 1. FORCE all nodes to pull their SO defaults right now.
        // This prevents inactive nodes from waking up later and wiping out our save data!
        foreach (SkillNodeUI node in allNodes)
        {
            node.InitializeFromSO();
        }

        // 2. Apply the saved upgrades safely
        foreach (SkillNodeUI node in allNodes)
        {
            if (node == null || node.SkillNode == null || node.SkillNode.SkillData == null) continue;

            string id = node.GetSkillID();

            if (unlockedSet.Contains(id))
            {
                if (node.SkillNode.IsLocked)
                {
                    node.SkillNode.SetUnlockable(true);
                    
                    // This cascades down and unlocks the children properly
                    node.SkillNode.ApplyUpgrade();
                    
                    if (SkillTreeManager.OnSkillUpgrade != null)
                    {
                        SkillTreeManager.OnSkillUpgrade.Invoke(node);
                    }

                    node.UnHighlight(); 
                    Debug.Log($"<color=green>[Save Sync]</color> Unlocked Skill: {node.SkillNode.SkillData.UpgradeName}");
                }
            }
        }
    }
}
// Assets/Scripts/Core/SaveManager.cs
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
    public SaveData CurrentSaveData { get; private set; } = new SaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
    }

    public void SaveGame(Checkpoint currentCheckpoint)
    {
        if (currentCheckpoint != null) CheckpointManager.Instance.SetCurrentCheckpoint(currentCheckpoint);

        CurrentSaveData.currentCheckpoint = CheckpointManager.Instance.GetCurrentCheckpointName();
        CurrentSaveData.activatedCheckpoints = CheckpointManager.Instance.GetActivatedCheckpointNames();

        if (CharmSaveBridge.Instance != null)
        {
            CurrentSaveData.unlockedCharmIds = CharmSaveBridge.Instance.GetUnlockedCharmIds();
            CurrentSaveData.equippedCharmIds = CharmSaveBridge.Instance.GetEquippedCharmIds();
        }

        if (InventorySaveBridge.Instance != null)
        {
            CurrentSaveData.unlockedItemIds = InventorySaveBridge.Instance.GetUnlockedItemIds();
            CurrentSaveData.equippedItemIds = InventorySaveBridge.Instance.GetEquippedItemIds();
        }

        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
        {
            if (chest.HasBeenOpened && !CurrentSaveData.openedChestIds.Contains(chest.ChestID))
            {
                CurrentSaveData.openedChestIds.Add(chest.ChestID);
            }
        }

        SkillNodeUI[] skillNodes = FindObjectsOfType<SkillNodeUI>(true);
        foreach (var node in skillNodes)
        {
            if (node.IsNodeUnlocked())
            {
                string id = node.GetSkillID();
                if (!string.IsNullOrEmpty(id) && !CurrentSaveData.unlockedSkillIds.Contains(id))
                {
                    CurrentSaveData.unlockedSkillIds.Add(id);
                }
            }
        }

        string json = JsonUtility.ToJson(CurrentSaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Saved game to: {saveFilePath}");
    }

    public void LoadGame(bool teleportPlayerToCurrentCheckpoint = false)
    {
        if (!File.Exists(saveFilePath)) { Debug.LogWarning("No save file found."); return; }

        string json = File.ReadAllText(saveFilePath);
        CurrentSaveData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();

        CheckpointManager.Instance.RestoreCheckpointState(CurrentSaveData.activatedCheckpoints, CurrentSaveData.currentCheckpoint);

        if (CharmSaveBridge.Instance != null)
            CharmSaveBridge.Instance.LoadState(CurrentSaveData.unlockedCharmIds, CurrentSaveData.equippedCharmIds);

        if (InventorySaveBridge.Instance != null)
            InventorySaveBridge.Instance.LoadState(CurrentSaveData.unlockedItemIds, CurrentSaveData.equippedItemIds);

        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
        {
            bool wasOpened = CurrentSaveData.openedChestIds.Contains(chest.ChestID);
            chest.LoadState(wasOpened);
        }

        // --- NEW: Load the Skills! ---
        if (SkillSaveBridge.Instance != null)
        {
            SkillSaveBridge.Instance.LoadState(CurrentSaveData.unlockedSkillIds);
        }

        if (teleportPlayerToCurrentCheckpoint)
        {
            Checkpoint current = CheckpointManager.Instance.GetCurrentCheckpoint();
            if (current != null) CheckpointManager.Instance.TeleportPlayerTo(current);
        }

        Debug.Log("Game loaded.");
    }
}
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

        // Save Charms
        if (CharmSaveBridge.Instance != null)
        {
            CurrentSaveData.unlockedCharmIds = CharmSaveBridge.Instance.GetUnlockedCharmIds();
            CurrentSaveData.equippedCharms = CharmSaveBridge.Instance.GetEquippedCharms();
        }

        // Save Items
        if (InventorySaveBridge.Instance != null)
        {
            CurrentSaveData.unlockedItemIds = InventorySaveBridge.Instance.GetUnlockedItemIds();
            CurrentSaveData.equippedItemIds = InventorySaveBridge.Instance.GetEquippedItemIds();
        }

        // Save opened chests in current scene (merged so we don't erase chests from other scenes)
        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
        {
            if (chest.HasBeenOpened && !CurrentSaveData.openedChestIds.Contains(chest.ChestID))
            {
                CurrentSaveData.openedChestIds.Add(chest.ChestID);
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
            CharmSaveBridge.Instance.LoadState(CurrentSaveData.unlockedCharmIds, CurrentSaveData.equippedCharms);

        if (InventorySaveBridge.Instance != null)
            InventorySaveBridge.Instance.LoadState(CurrentSaveData.unlockedItemIds, CurrentSaveData.equippedItemIds);

        // Instantly force-open chests that were already looted
        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
        {
            bool wasOpened = CurrentSaveData.openedChestIds.Contains(chest.ChestID);
            chest.LoadState(wasOpened);
        }

        if (teleportPlayerToCurrentCheckpoint)
        {
            Checkpoint current = CheckpointManager.Instance.GetCurrentCheckpoint();
            if (current != null) CheckpointManager.Instance.TeleportPlayerTo(current);
        }

        Debug.Log("Game loaded.");
    }
}
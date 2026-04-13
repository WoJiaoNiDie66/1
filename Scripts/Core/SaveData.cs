// Assets/Scripts/Core/SaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class CharmSaveData
{
    public string charmId;
    public int slotId;
}

[Serializable]
public class SaveData
{
    public string currentCheckpoint;
    public List<string> activatedCheckpoints = new List<string>();

    public List<string> unlockedCharmIds = new List<string>();
    public List<string> unlockedItemIds = new List<string>();

    // --- NEW: Equipment & Chests ---
    public List<string> equippedItemIds = new List<string>();
    public List<CharmSaveData> equippedCharms = new List<CharmSaveData>();
    public List<string> openedChestIds = new List<string>();
}
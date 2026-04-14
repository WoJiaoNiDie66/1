// Assets/Scripts/Core/SaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string currentCheckpoint;
    public List<string> activatedCheckpoints = new List<string>();

    public List<string> unlockedCharmIds = new List<string>();
    public List<string> unlockedItemIds = new List<string>();

    public List<string> equippedItemIds = new List<string>();
    public List<string> equippedCharmIds = new List<string>(); // <-- Simplified to just strings
    
    public List<string> openedChestIds = new List<string>();
}
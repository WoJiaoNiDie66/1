// Assets/Scripts/Core/SaveData.cs
using System;
using System.Collections.Generic;

// --- NEW: Q-Learning 数据结构 ---
[Serializable]
public class QLearningData
{
    public string enemyId;
    public List<float> skillPossibilities = new List<float>();
}

[Serializable]
public class SaveData
{
    public int id;
    public string currentCheckpoint;
    public List<string> activatedCheckpoints = new List<string>();

    public List<string> unlockedCharmIds = new List<string>();
    public List<string> unlockedItemIds = new List<string>();

    public List<string> equippedItemIds = new List<string>();
    public List<string> equippedCharmIds = new List<string>();

    public List<string> openedChestIds = new List<string>();
    
    // --- NEW: Skill Tree ---
    public List<string> unlockedSkillIds = new List<string>();

    // --- NEW: 敌人和 Boss 状态 ---
    public List<string> defeatedBossNames = new List<string>(); // 记录已死亡的 Boss 名称
    public List<QLearningData> qLearningStats = new List<QLearningData>(); // 记录 Q-Learning 强度
}
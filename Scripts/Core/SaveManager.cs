// Assets/Scripts/Core/SaveManager.cs
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
    public SaveData CurrentSaveData { get; private set; } = new SaveData();

    private bool isReloadingScene = false;
    private float lastLoadTime = -1f;

    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
    }

    private void Start()
    {
        // 【优化】游戏启动时延迟一帧加载，确保 CheckpointManager 等单例已就绪
        StartCoroutine(InitialLoadRoutine());
    }

    private IEnumerator InitialLoadRoutine()
    {
        yield return null; // 等待第一帧
        LoadGame(true); 
        Debug.Log("<color=cyan>[SaveManager]</color> 游戏启动：已自动加载存档并执行传送。");
    }

    /// <summary>
    /// 保存游戏存档
    /// </summary>
    public void SaveGame(Checkpoint currentCheckpoint)
    {
        Debug.Log("Save Game.");

        // --- Checkpoints, Charms, Inventory, Chests, Skills ---
        if (currentCheckpoint != null)
            CheckpointManager.Instance.SetCurrentCheckpoint(currentCheckpoint);

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
                CurrentSaveData.openedChestIds.Add(chest.ChestID);
        }

        SkillNodeUI[] skillNodes = FindObjectsOfType<SkillNodeUI>(true);
        foreach (var node in skillNodes)
        {
            if (node.IsNodeUnlocked())
            {
                string id = node.GetSkillID();
                if (!string.IsNullOrEmpty(id) && !CurrentSaveData.unlockedSkillIds.Contains(id))
                    CurrentSaveData.unlockedSkillIds.Add(id);
            }
        }

        // --- Bosses & Q-Learning ---
        SkeletonSwordDecision[] allEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        
        // 这里的逻辑是：如果 shouldRespawn 为 false 且已死亡，则记入存档
        foreach (var enemy in allEnemies)
        {
            if (!enemy.shouldRespawn && enemy.IsDead())
            {
                if (!CurrentSaveData.defeatedBossNames.Contains(enemy.gameObject.name))
                    CurrentSaveData.defeatedBossNames.Add(enemy.gameObject.name);
            }
        }

        CurrentSaveData.qLearningStats.Clear();
        foreach (var qEnemy in allEnemies)
        {
            if (qEnemy.applyQLearning)
            {
                QLearningData data = new QLearningData();
                data.enemyId = qEnemy.gameObject.name;
                foreach (var skill in qEnemy.battleSkills) data.skillPossibilities.Add(skill.posibility);
                CurrentSaveData.qLearningStats.Add(data);
            }
        }

        File.WriteAllText(saveFilePath, JsonUtility.ToJson(CurrentSaveData, true));
        Debug.Log($"[SaveManager] Saved game to: {saveFilePath}");
    }

    /// <summary>
    /// 加载游戏存档
    /// </summary>
    public void LoadGame(bool teleportPlayerToCurrentCheckpoint = false)
    {
        Debug.Log("Load Game.");
        if (!File.Exists(saveFilePath)) return;
        
        string json = File.ReadAllText(saveFilePath);
        CurrentSaveData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();

        // 如果不是刚进游戏（运行时间超过0.5秒），则需要重载场景
        if (Time.timeSinceLevelLoad > 0.5f)
        {
            if (!isReloadingScene) StartCoroutine(ReloadSceneAndApply(teleportPlayerToCurrentCheckpoint));
        }
        else
        {
            ApplySaveData(teleportPlayerToCurrentCheckpoint);
        }
    }

    private IEnumerator ReloadSceneAndApply(bool teleportPlayerToCurrentCheckpoint)
    {
        isReloadingScene = true;
        Time.timeScale = 1f;
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        yield return new WaitUntil(() => op.isDone);
        yield return new WaitForEndOfFrame();
        ApplySaveData(teleportPlayerToCurrentCheckpoint);
        isReloadingScene = false;
    }

    /// <summary>
    /// 将存档数据应用到场景中的对象
    /// </summary>
    private void ApplySaveData(bool teleportPlayerToCurrentCheckpoint)
    {
        // 防御性编程：防止 GameBootstrapper 和 InitialLoadRoutine 在同一帧内双重加载
        if (Time.unscaledTime - lastLoadTime < 0.1f) return;
        lastLoadTime = Time.unscaledTime;

        // --- Restore Bridges & Chests ---
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

        if (SkillSaveBridge.Instance != null)
            SkillSaveBridge.Instance.LoadState(CurrentSaveData.unlockedSkillIds);


        // --- Bosses & Q-Learning ---
        SkeletonSwordDecision[] allEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);

        foreach (var enemy in allEnemies)
        {
            // 处理 Boss (shouldRespawn 为 false 的怪)
            if (!enemy.shouldRespawn)
            {
                if (CurrentSaveData.defeatedBossNames.Contains(enemy.gameObject.name))
                    enemy.gameObject.SetActive(false); // 已击败的 Boss 隐藏
                else
                {
                    enemy.gameObject.SetActive(true);
                    enemy.ResetEnemy(); // 未击败的 Boss 重置血量和位置
                }
            }
        }

        foreach (var qEnemy in allEnemies)
        {
            if (qEnemy.applyQLearning)
            {
                var savedData = CurrentSaveData.qLearningStats.Find(d => d.enemyId == qEnemy.gameObject.name);
                if (savedData != null && savedData.skillPossibilities.Count == qEnemy.battleSkills.Length)
                {
                    for (int i = 0; i < qEnemy.battleSkills.Length; i++)
                        qEnemy.battleSkills[i].posibility = savedData.skillPossibilities[i];
                }
            }
        }

        // 【新增修复】执行传送玩家到存档点 T 位置的逻辑
        if (teleportPlayerToCurrentCheckpoint && CheckpointManager.Instance != null && !string.IsNullOrEmpty(CurrentSaveData.currentCheckpoint))
        {
            Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>(true);
            foreach (var cp in allCheckpoints)
            {
                if (cp.CheckpointName == CurrentSaveData.currentCheckpoint)
                {
                    // 呼叫 CheckpointManager 进行物理传送（会使用我们设定的位置 T）
                    CheckpointManager.Instance.TeleportPlayerTo(cp);
                    break;
                }
            }
        }
    }
}
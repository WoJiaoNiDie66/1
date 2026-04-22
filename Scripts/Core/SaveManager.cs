// Assets/Scripts/Core/SaveManager.cs
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static int ActiveSlot { get; set; } = 0;

    private const int SLOT_COUNT = 3;

    private string saveFilePath;
    public SaveData CurrentSaveData { get; private set; } = new SaveData();

    private bool isReloadingScene = false;
    private float lastLoadTime = -1f;

    private const string DEFAULT_SAVE_JSON =
        "{\"id\":0,\"currentCheckpoint\":\"StartingCheckpoint\"," +
        "\"activatedCheckpoints\":[\"StartingCheckpoint\"]," +
        "\"unlockedCharmIds\":[]," +
        "\"unlockedItemIds\":[]," +
        "\"equippedItemIds\":[]," +
        "\"equippedCharmIds\":[]," +
        "\"openedChestIds\":[]," +
        "\"unlockedSkillIds\":[]," +
        "\"defeatedBossNames\":[]," +
        "\"qLearningStats\":[]}";

    // ─── The name of your gameplay scene ───
    private const string GAME_SCENE_NAME = "combat demo";
    private const string MENU_SCENE_NAME = "Main Menu";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshFilePath();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Fires every time any scene finishes loading.
    /// We only act when entering the gameplay scene.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GAME_SCENE_NAME)
        {
            StartCoroutine(InitialLoadRoutine());
        }
    }

    private IEnumerator InitialLoadRoutine()
    {
        yield return null; // wait one frame for other singletons to Awake
        LoadGame(true);
        Debug.Log($"<color=cyan>[SaveManager]</color> Scene loaded: applied slot {ActiveSlot}.");
    }

    /// <summary>
    /// Call this before returning to the main menu to clear transient state.
    /// </summary>
    public void PrepareForMainMenu()
    {
        // Stop any in-progress reload coroutine so it doesn't bleed into the next session.
        StopAllCoroutines();
        isReloadingScene = false;
        lastLoadTime = -1f;
        CurrentSaveData = new SaveData();
        Debug.Log("[SaveManager] State cleared for main menu return.");
    }

    private void RefreshFilePath()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, $"gamesave_{ActiveSlot}.json");
    }

    // ─── Public slot helpers ───────────────────────────────────

    public static bool SlotExists(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"gamesave_{slot}.json");
        return File.Exists(path);
    }

    public static SaveData PeekSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"gamesave_{slot}.json");
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static void DeleteSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"gamesave_{slot}.json");
        if (File.Exists(path)) File.Delete(path);
    }

    // ─── Core Save / Load ─────────────────────────────────────

    public void SaveGame(Checkpoint currentCheckpoint)
    {
        Debug.Log($"Save Game (Slot {ActiveSlot}).");
        RefreshFilePath();

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

        SkeletonSwordDecision[] allEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);

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
        Debug.Log($"[SaveManager] Saved to: {saveFilePath}");
    }

    public void LoadGame(bool teleportPlayerToCurrentCheckpoint = false)
    {
        Debug.Log($"Load Game (Slot {ActiveSlot}).");
        RefreshFilePath();

        string json = File.Exists(saveFilePath)
            ? File.ReadAllText(saveFilePath)
            : DEFAULT_SAVE_JSON;

        CurrentSaveData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();

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

    private void ApplySaveData(bool teleportPlayerToCurrentCheckpoint)
    {
        if (Time.unscaledTime - lastLoadTime < 0.1f) return;
        lastLoadTime = Time.unscaledTime;

        CheckpointManager.Instance.RestoreCheckpointState(CurrentSaveData.activatedCheckpoints, CurrentSaveData.currentCheckpoint);

        if (CharmSaveBridge.Instance != null)
            CharmSaveBridge.Instance.LoadState(CurrentSaveData.unlockedCharmIds, CurrentSaveData.equippedCharmIds);

        if (InventorySaveBridge.Instance != null)
            InventorySaveBridge.Instance.LoadState(CurrentSaveData.unlockedItemIds, CurrentSaveData.equippedItemIds);

        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
            chest.LoadState(CurrentSaveData.openedChestIds.Contains(chest.ChestID));

        if (SkillSaveBridge.Instance != null)
            SkillSaveBridge.Instance.LoadState(CurrentSaveData.unlockedSkillIds);

        SkeletonSwordDecision[] allEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);

        foreach (var enemy in allEnemies)
        {
            if (!enemy.shouldRespawn)
            {
                if (CurrentSaveData.defeatedBossNames.Contains(enemy.gameObject.name))
                    enemy.gameObject.SetActive(false);
                else
                {
                    enemy.gameObject.SetActive(true);
                    enemy.ResetEnemy();
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

        if (teleportPlayerToCurrentCheckpoint && CheckpointManager.Instance != null &&
            !string.IsNullOrEmpty(CurrentSaveData.currentCheckpoint))
        {
            Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>(true);
            foreach (var cp in allCheckpoints)
            {
                if (cp.CheckpointName == CurrentSaveData.currentCheckpoint)
                {
                    CheckpointManager.Instance.TeleportPlayerTo(cp);
                    break;
                }
            }
        }
    }
}
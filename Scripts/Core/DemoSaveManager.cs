// Assets/Scripts/Core/DemoSaveManager.cs
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drop this MonoBehaviour into the "Game Demo" scene's hierarchy (NOT on a
/// DontDestroyOnLoad object). It manages a single save file: gamesave_Demo.json.
/// It will self-destruct if it finds itself in any scene other than DEMO_SCENE_NAME.
/// </summary>
public class DemoSaveManager : MonoBehaviour
{
    public static DemoSaveManager Instance { get; private set; }

    // ── DEMO SCENE GUARD ──────────────────────────────────────────────────────
    private const string DEMO_SCENE_NAME = "Demo Scene"; // ← set to your exact scene name

    // ── FIXED SAVE FILE ───────────────────────────────────────────────────────
    private const string DEMO_SAVE_FILENAME = "gamesave_Demo.json";
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

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Destroy self if we are NOT in the Demo scene.
        if (SceneManager.GetActiveScene().name != DEMO_SCENE_NAME)
        {
            Destroy(gameObject);
            return;
        }

        // Standard singleton (scene-local; no DontDestroyOnLoad intentionally).
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        saveFilePath = Path.Combine(Application.persistentDataPath, DEMO_SAVE_FILENAME);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // First load when the Demo scene starts.
        StartCoroutine(InitialLoadRoutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the Demo scene reloads itself (e.g. after dying), re-apply save data.
        if (scene.name == DEMO_SCENE_NAME)
            StartCoroutine(InitialLoadRoutine());
    }

    private IEnumerator InitialLoadRoutine()
    {
        yield return null; // wait one frame for other singletons to Awake
        LoadGame(true);
        Debug.Log($"<color=cyan>[DemoSaveManager]</color> Demo scene loaded – applied gamesave_Demo.");
    }

    // ─── Public helpers (parallel to SaveManager's static helpers) ────────────

    public static bool DemoSaveExists()
    {
        string path = Path.Combine(Application.persistentDataPath, DEMO_SAVE_FILENAME);
        return File.Exists(path);
    }

    public static SaveData PeekDemoSave()
    {
        string path = Path.Combine(Application.persistentDataPath, DEMO_SAVE_FILENAME);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static void DeleteDemoSave()
    {
        string path = Path.Combine(Application.persistentDataPath, DEMO_SAVE_FILENAME);
        if (File.Exists(path)) File.Delete(path);
    }

    // ─── Core Save ────────────────────────────────────────────────────────────

    public void SaveGame(Checkpoint currentCheckpoint)
    {
        Debug.Log("[DemoSaveManager] Saving to gamesave_Demo.");

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

        SaveableInteractable[] allInteractables = FindObjectsOfType<SaveableInteractable>(true);
        foreach (var interactable in allInteractables)
        {
            if (interactable.requireSaveState && interactable.hasBeenInteracted)
            {
                if (!CurrentSaveData.interactedObjectIds.Contains(interactable.uniqueID))
                    CurrentSaveData.interactedObjectIds.Add(interactable.uniqueID);
            }
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
        Debug.Log($"[DemoSaveManager] Saved to: {saveFilePath}");
    }

    // ─── Core Load ────────────────────────────────────────────────────────────

    public void LoadGame(bool teleportPlayerToCurrentCheckpoint = false)
    {
        Debug.Log("[DemoSaveManager] Loading gamesave_Demo.");

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

    /// <summary>Wipe the demo file and reset state – useful for "Restart Demo" buttons.</summary>
    public void ResetDemoSave()
    {
        DeleteDemoSave();
        CurrentSaveData = new SaveData();
        Debug.Log("[DemoSaveManager] Demo save reset.");
    }

    // ─── Internals ────────────────────────────────────────────────────────────

    private IEnumerator ReloadSceneAndApply(bool teleport)
    {
        isReloadingScene = true;
        Time.timeScale = 1f;
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        yield return new WaitUntil(() => op.isDone);
        yield return new WaitForEndOfFrame();
        ApplySaveData(teleport);
        isReloadingScene = false;
    }

    private void ApplySaveData(bool teleportPlayerToCurrentCheckpoint)
    {
        if (Time.unscaledTime - lastLoadTime < 0.1f) return;
        lastLoadTime = Time.unscaledTime;

        CheckpointManager.Instance.RestoreCheckpointState(
            CurrentSaveData.activatedCheckpoints, CurrentSaveData.currentCheckpoint);

        if (CharmSaveBridge.Instance != null)
            CharmSaveBridge.Instance.LoadState(CurrentSaveData.unlockedCharmIds, CurrentSaveData.equippedCharmIds);

        if (InventorySaveBridge.Instance != null)
            InventorySaveBridge.Instance.LoadState(CurrentSaveData.unlockedItemIds, CurrentSaveData.equippedItemIds);

        ChestController[] chestsInScene = FindObjectsOfType<ChestController>();
        foreach (var chest in chestsInScene)
            chest.LoadState(CurrentSaveData.openedChestIds.Contains(chest.ChestID));

        if (SkillSaveBridge.Instance != null)
            SkillSaveBridge.Instance.LoadState(CurrentSaveData.unlockedSkillIds);

        SaveableInteractable[] allInteractables = FindObjectsOfType<SaveableInteractable>(true);
        foreach (var interactable in allInteractables)
        {
            if (CurrentSaveData.interactedObjectIds.Contains(interactable.uniqueID))
            {
                interactable.RestoreState();
                Debug.Log($"<color=yellow>[DemoLoad]</color> Restored {interactable.gameObject.name}");
            }
        }

        SkeletonSwordDecision[] allEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        foreach (var enemy in allEnemies)
        {
            if (!enemy.shouldRespawn)
            {
                if (CurrentSaveData.defeatedBossNames.Contains(enemy.gameObject.name))
                    enemy.gameObject.SetActive(false);
                else { enemy.gameObject.SetActive(true); enemy.ResetEnemy(); }
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
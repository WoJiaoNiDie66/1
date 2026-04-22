// Assets/Scripts/Enemy/EnemyManager.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // 维持列表以兼容其他脚本的查询需求
    private List<SkeletonSwordDecision> allEnemies = new List<SkeletonSwordDecision>();
    private Dictionary<string, SkeletonSwordDecision> bossRegistry = new Dictionary<string, SkeletonSwordDecision>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshEnemyList();
        // 订阅检查点休息事件
        EventManager.StartListening("OnCheckpointRest", OnCheckpointRest);
    }

    private void OnDestroy()
    {
        EventManager.StopListening("OnCheckpointRest", OnCheckpointRest);
    }

    // =======================================================
    // 核心复活逻辑：触碰存档点时触发
    // =======================================================
    public void OnCheckpointRest()
    {
        // 【终极必杀技】：直接扫描场景中*所有*的骷髅怪，包含 SetActive(false) 躺在地上的死怪
        SkeletonSwordDecision[] currentEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        int respawnCount = 0;

        foreach (var enemy in currentEnemies)
        {
            if (enemy == null) continue;

            if (enemy.shouldRespawn)
            {
                // 普通怪：无论死活，强行复活
                enemy.ResetEnemy();
                respawnCount++;
            }
            else
            {
                // Boss：如果没有死，则回满血；如果已经死了，则无视
                if (!enemy.IsDead())
                {
                    enemy.ResetEnemy();
                }
            }
        }

        Debug.Log($"<color=cyan>[EnemyManager]</color> 存档点休息完毕。强行扫描并复活了 {respawnCount} 个敌人。");
    }

    // =======================================================
    // 恢复给其他脚本（如 LevelManager）调用的 API
    // =======================================================

    /// <summary>
    /// 供 LevelManager 等外部脚本调用的复活方法
    /// </summary>
    public void RespawnRegularEnemies()
    {
        SkeletonSwordDecision[] currentEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        foreach (var enemy in currentEnemies)
        {
            if (enemy != null && enemy.shouldRespawn)
            {
                enemy.ResetEnemy();
            }
        }
        Debug.Log("EnemyManager: 普通敌人已重生 (由外部调用)。");
    }

    /// <summary>
    /// 供 LevelManager 等外部脚本调用的清除方法
    /// </summary>
    public void DespawnAllRegularEnemies()
    {
        SkeletonSwordDecision[] currentEnemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        foreach (var enemy in currentEnemies)
        {
            if (enemy != null && enemy.shouldRespawn)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    public void RefreshEnemyList()
    {
        allEnemies.Clear();
        bossRegistry.Clear();
        SkeletonSwordDecision[] enemies = FindObjectsOfType<SkeletonSwordDecision>(true);
        foreach (var enemy in enemies)
        {
            allEnemies.Add(enemy);
            if (!enemy.shouldRespawn) bossRegistry[enemy.gameObject.name] = enemy;
        }
    }

    public void RegisterEnemy(SkeletonSwordDecision enemy)
    {
        if (!allEnemies.Contains(enemy)) allEnemies.Add(enemy);
        if (!enemy.shouldRespawn) bossRegistry[enemy.gameObject.name] = enemy;
    }

    public void UnregisterEnemy(SkeletonSwordDecision enemy)
    {
        allEnemies.Remove(enemy);
        if (!enemy.shouldRespawn) bossRegistry.Remove(enemy.gameObject.name);
    }

    public List<SkeletonSwordDecision> GetActiveEnemies()
    {
        return new List<SkeletonSwordDecision>(allEnemies);
    }

    public SkeletonSwordDecision GetBoss(string bossName)
    {
        if (bossRegistry.TryGetValue(bossName, out SkeletonSwordDecision boss)) return boss;
        return null;
    }

    public int GetActiveEnemyCount()
    {
        return allEnemies.Count;
    }
}
// Assets/Scripts/Core/CheckpointManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public static UnityAction<Checkpoint> OnCheckpointPanelOpened; 

    private readonly List<Checkpoint> activatedCheckpoints = new List<Checkpoint>();
    private Checkpoint currentCheckpoint;

    [Header("Map Optimization")]
    [SerializeField] private List<MapNode> allWorldMaps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (!activatedCheckpoints.Contains(checkpoint))
            activatedCheckpoints.Add(checkpoint);

        currentCheckpoint = checkpoint;
    }

    public void SetCurrentCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        if (!activatedCheckpoints.Contains(checkpoint))
            activatedCheckpoints.Add(checkpoint);

        currentCheckpoint = checkpoint;
    }

    public string GetCurrentCheckpointName()
    {
        return currentCheckpoint != null ? currentCheckpoint.CheckpointName : string.Empty;
    }

    public Checkpoint GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    public List<string> GetActivatedCheckpointNames()
    {
        List<string> result = new List<string>();

        for (int i = 0; i < activatedCheckpoints.Count; i++)
        {
            if (activatedCheckpoints[i] != null)
                result.Add(activatedCheckpoints[i].CheckpointName);
        }

        return result;
    }

    public void RestoreCheckpointState(List<string> activatedNames, string currentCheckpointName)
    {
        activatedCheckpoints.Clear();
        currentCheckpoint = null;

        HashSet<string> activatedSet = new HashSet<string>();
        if (activatedNames != null)
        {
            for (int i = 0; i < activatedNames.Count; i++)
            {
                if (!string.IsNullOrEmpty(activatedNames[i]))
                    activatedSet.Add(activatedNames[i]);
            }
        }

        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            Checkpoint cp = allCheckpoints[i];
            if (cp == null) continue;

            bool isActivated = activatedSet.Contains(cp.CheckpointName);
            cp.SetActivatedState(isActivated);

            if (isActivated)
                activatedCheckpoints.Add(cp);

            if (!string.IsNullOrEmpty(currentCheckpointName) &&
                cp.CheckpointName == currentCheckpointName)
            {
                currentCheckpoint = cp;
            }
        }

        if (currentCheckpoint == null && activatedCheckpoints.Count > 0)
            currentCheckpoint = activatedCheckpoints[activatedCheckpoints.Count - 1];
    }

    public List<Checkpoint> GetOtherCheckpoints(Checkpoint current)
    {
        return activatedCheckpoints.FindAll(c => c != current);
    }

    public void TeleportPlayerTo(Checkpoint target)
    {
        if (target == null) return;

        // 【关键】第一步：先确保存档点所在的地图和邻居是开启的
        if (target.assignedMap != null)
        {
            UpdateWorldMapVisibility(target.assignedMap);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("CwcPlayer_A1");
        if (player == null) return;

        // 第二步：执行物理传送
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 

        player.transform.position = target.TeleportPosition; 
        player.transform.rotation = target.TeleportRotation;
        
        Physics.SyncTransforms(); // 强制同步坐标

        if (cc != null) cc.enabled = true;

        currentCheckpoint = target;
        Debug.Log($"<color=cyan>[Teleport]</color> 传送至: {target.CheckpointName}，地图已激活。");
    }

    public void UpdateWorldMapVisibility(MapNode currentMap)
    {
        if (currentMap == null || allWorldMaps == null) return;

        // 1. 先把所有地图名字存进一个列表，方便查找
        List<string> activeMapIDs = new List<string>();
        activeMapIDs.Add(currentMap.mapID);
        if (currentMap.parentNode != null) activeMapIDs.Add(currentMap.parentNode.mapID);
        foreach (var child in currentMap.childNodes) activeMapIDs.Add(child.mapID);

        // 2. 遍历所有节点，通过名字找物体并开关
        foreach (var node in allWorldMaps)
        {
            // 【关键】通过 GameObject.Find 配合 MapID 找到场景里的物体
            GameObject mapObj = GameObject.Find(node.mapID);
            
            // 如果找不到，尝试找隐藏状态的物体 (Find 不到隐藏物体，所以建议在最外层加个管理或用标签)
            if (mapObj == null) 
            {
                // 这是一个小技巧：预先在 Start 里把所有地图物体存进一个 Dictionary
                // 或者直接用：GameObject.Find("/Catacomb") 这种路径方式
            }

            if (mapObj != null)
            {
                mapObj.SetActive(activeMapIDs.Contains(node.mapID));
            }
        }
    }
}
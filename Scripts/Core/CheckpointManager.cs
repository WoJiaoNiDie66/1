// Assets/Scripts/Core/CheckpointManager.cs
using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private readonly List<Checkpoint> activatedCheckpoints = new List<Checkpoint>();
    private Checkpoint currentCheckpoint;

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
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("CwcPlayer_A1");
        }
        
        if (player == null || target == null) return;

        // 关闭 CharacterController 防止物理引擎阻挡传送
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 1. 设置到位置 T
        player.transform.position = target.TeleportPosition;
        // 2. 设置朝向 T 的方向
        player.transform.rotation = target.TeleportRotation;
        
        // 3. 强制同步物理引擎坐标
        Physics.SyncTransforms();

        // 重新开启 CharacterController
        if (cc != null) cc.enabled = true;

        currentCheckpoint = target;
        Debug.Log($"<color=cyan>[Teleport]</color> 玩家已传送到存档点: {target.CheckpointName} 的位置 T");
    }
}
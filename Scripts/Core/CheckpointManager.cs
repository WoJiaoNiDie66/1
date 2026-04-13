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
        GameObject player = GameObject.Find("CwcPlayer_A0");
        if (player == null || target == null) return;

        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (var cp in allCheckpoints)
            cp.ResetRange();

        CharacterController cc = player.GetComponent<CharacterController>();
        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (cc != null) cc.enabled = false;
        player.transform.position = target.TeleportPosition;
        if (cc != null) cc.enabled = true;

        pm?.ResetVerticalVelocity();

        currentCheckpoint = target;

        Debug.Log($"Teleported to checkpoint: {target.CheckpointName}");
    }
}
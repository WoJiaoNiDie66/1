// Assets/Scripts/Core/CheckpointManager.cs
using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private readonly List<Checkpoint> activatedCheckpoints = new List<Checkpoint>();

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
    }

    public List<Checkpoint> GetOtherCheckpoints(Checkpoint current)
    {
        return activatedCheckpoints.FindAll(c => c != current);
    }

    public void TeleportPlayerTo(Checkpoint target)
    {
        GameObject player = GameObject.Find("CwcPlayer_A0");
        if (player == null) return;

        // Fix: FORCE clear playerInRange for all checkpoints in the scene.
        // This completely prevents the bug where the old checkpoint thinks you are still standing there.
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (var cp in allCheckpoints)
        {
            cp.ResetRange();
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (cc != null) cc.enabled = false;
        player.transform.position = target.TeleportPosition;
        if (cc != null) cc.enabled = true;

        pm?.ResetVerticalVelocity();

        Debug.Log($"Teleported to checkpoint: {target.CheckpointName}");
    }
}
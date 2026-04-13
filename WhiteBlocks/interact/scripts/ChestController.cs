using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("寶箱部位")]
    [Tooltip("請拖入寶箱蓋子 (Chest_02)")]
    public Transform chestLid;

    [Header("角度設定 (X軸)")]
    public float closedAngleX = 50f;
    public float openAngleX = -1.612f;

    [Header("互動設定")]
    public float openSpeed = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("寶箱內容")]
    [Tooltip("Assign a ChestData ScriptableObject to define rewards")]
    public ChestData chestData;

    [Header("Reward Animation Settings")]
    public float floatHeight = 1.8f;         // How high the sprite floats up
    public float spinSpeed = 360f;         // How fast it spins (degrees per second)
    public float spriteScale = 2f;         // Adjust if your sprites are too small/big
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0); // Starting offset from chest center

    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerInRange = false;
    private bool hasBeenOpened = false;

    private float defaultY;
    private float defaultZ;

    void Start()
    {
        if (chestLid != null)
        {
            defaultY = chestLid.localEulerAngles.y;
            defaultZ = chestLid.localEulerAngles.z;
            chestLid.localRotation = Quaternion.Euler(closedAngleX, defaultY, defaultZ);
        }
        else
        {
            Debug.LogError("請記得把 Chest_02 拖入 Chest Lid 欄位！");
        }
    }

    void Update()
    {
        if (playerInRange && !isMoving && !hasBeenOpened && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenChestAndGiveRewards());
        }
    }

    IEnumerator OpenChestAndGiveRewards()
    {
        isMoving = true;
        isOpen = true;
        hasBeenOpened = true;

        Debug.Log("【寶箱】打開");

        // --- Animate lid open ---
        Quaternion startRot = chestLid.localRotation;
        Quaternion targetRot = Quaternion.Euler(openAngleX, defaultY, defaultZ);
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            chestLid.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            yield return null;
        }

        chestLid.localRotation = targetRot;
        isMoving = false;

        // --- Give rewards ---
        if (chestData == null)
        {
            Debug.LogWarning("【寶箱】沒有設定 ChestData，沒有獎勵。");
            yield break;
        }

        List<ChestData.ChestReward> rewards = chestData.PickRewards();

        if (rewards.Count == 0)
        {
            Debug.Log("【寶箱】是空的！");
            yield break;
        }

        // Loop through rewards and spawn visuals
        foreach (var reward in rewards)
        {
            Sprite rewardSprite = null;

            if (reward.rewardType == ChestData.ChestReward.RewardType.Charm && reward.charm != null)
            {
                reward.charm.SetUnlock();
                rewardSprite = reward.charm.CharmSprite;
                Debug.Log($"【寶箱】獲得符文：{reward.charm.CharmName}");
            }
            else if (reward.rewardType == ChestData.ChestReward.RewardType.Item && reward.item != null)
            {
                reward.item.SetUnlock();
                rewardSprite = reward.item.ItemSprite;
                Debug.Log($"【寶箱】獲得道具：{reward.item.ItemName}");
            }

            // Spawn and animate the sprite if one exists
            if (rewardSprite != null)
            {
                StartCoroutine(SpawnAndAnimateReward(rewardSprite));
                
                // Wait 0.5 seconds before spawning the next item so they don't perfectly overlap
                yield return new WaitForSeconds(0.5f); 
            }
        }
    }

    // --- Reward Animation Coroutine ---
    IEnumerator SpawnAndAnimateReward(Sprite sprite)
    {
        // 1. Create a new empty GameObject for the visual
        GameObject visualObj = new GameObject("RewardVisual");
        visualObj.transform.position = transform.position + spawnOffset;
        visualObj.transform.localScale = Vector3.one * spriteScale;

        // 2. Add a SpriteRenderer and assign the item's sprite
        SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // To ensure it renders cleanly in 3D space, you can uncomment this if it hides behind other transparent things
        // sr.sortingOrder = 10; 

        Vector3 startPos = visualObj.transform.position;
        Vector3 targetPos = startPos + Vector3.up * floatHeight;

        // Phase 1: Spin up for 1 second
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            
            // Lerp position upwards (SmoothStep makes it ease out nicely at the top)
            visualObj.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, timer));
            
            // Spin on the Y axis
            visualObj.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
            
            yield return null;
        }

        // Lock exactly to target height to prevent floating point inaccuracies
        visualObj.transform.position = targetPos;

        // Phase 2: Stay in position and keep spinning for 2 seconds
        timer = 0f;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            visualObj.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
            yield return null;
        }

        // Phase 3: Disappear (Destroy the temporary GameObject)
        Destroy(visualObj);
    }

    // --- 感應區設定 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!hasBeenOpened)
                Debug.Log("靠近寶箱，按 E 互動");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
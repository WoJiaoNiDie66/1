using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestData", menuName = "Chest/New ChestData")]
public class ChestData : ScriptableObject
{
    [System.Serializable]
    public class ChestReward
    {
        public enum RewardType { Charm, Item }

        public RewardType rewardType;

        [Tooltip("Assign if rewardType is Charm")]
        public Charm charm;

        [Tooltip("Assign if rewardType is Item")]
        public ItemData item;

        [Range(0f, 1f)]
        [Tooltip("Drop weight (relative probability)")]
        public float weight = 1f;
    }

    [Header("Chest Reward Pool")]
    public List<ChestReward> rewardPool = new List<ChestReward>();

    [Tooltip("How many rewards to give when chest is opened")]
    public int rewardCount = 1;

    /// <summary>
    /// Randomly picks rewards from the pool based on weights.
    /// </summary>
    public List<ChestReward> PickRewards()
    {
        List<ChestReward> picked = new List<ChestReward>();
        List<ChestReward> available = new List<ChestReward>(rewardPool);

        for (int i = 0; i < rewardCount && available.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (var r in available) totalWeight += r.weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var r in available)
            {
                cumulative += r.weight;
                if (roll <= cumulative)
                {
                    picked.Add(r);
                    available.Remove(r);
                    break;
                }
            }
        }

        return picked;
    }
}
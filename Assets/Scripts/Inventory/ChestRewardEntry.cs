using UnityEngine;

[System.Serializable]
public class ChestRewardEntry
{
    public ChestRewardData rewardData;
    [Range(0, 1)] public float probability = 1f;
    public int minAmount = 1;
    public int maxAmount = 1;
} 
using UnityEngine;

public abstract class ChestRewardData : ScriptableObject
{
    public abstract void GrantReward(object player, int amount);
} 
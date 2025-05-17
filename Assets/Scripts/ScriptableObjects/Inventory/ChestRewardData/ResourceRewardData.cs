using UnityEngine;

public enum ResourceType { Gold, Gem }

[CreateAssetMenu(menuName = "ChestReward/ResourceReward")]
public class ResourceRewardData : ChestRewardData
{
    public ResourceType resourceType;
    public override void GrantReward(object player, int amount)
    {
        // TODO: Cộng tài nguyên cho player
        Debug.Log($"Player nhận được {amount} {resourceType}");
    }
} 
using UnityEngine;

[CreateAssetMenu(menuName = "ChestReward/ItemReward")]
public class ItemRewardData : ChestRewardData
{
    public ItemData itemData;
    public override void GrantReward(object player, int amount)
    {
        // TODO: Thêm item vào inventory player
        Debug.Log($"Player nhận được {amount} x {itemData.itemName}");
    }
} 
using UnityEngine;

[CreateAssetMenu(menuName = "ChestReward/ItemReward")]
public class ItemRewardData : ChestRewardData
{
    public ItemData itemData;
    public override void GrantReward(object player, int amount)
    {
        // Thêm item vào inventory player
        InventoryManager.Instance.AddItemToInventory(itemData, amount);
        Debug.Log($"Player nhận được {amount} x {itemData.itemName}");
    }
} 
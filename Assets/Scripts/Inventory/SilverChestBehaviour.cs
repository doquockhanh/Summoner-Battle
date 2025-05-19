using UnityEngine;

public class SilverChestBehaviour : IItemUsable
{
    private ChestData chestData;

    public SilverChestBehaviour()
    {
        chestData = Resources.Load<ChestData>("ScriptableObjects/Chest/SilverChest");
    }
    public void Use(Item item)
    {
        Debug.Log("Mở hòm bạc! (Chạy animation mở hòm ở đây)");
        if (chestData == null)
        {
            Debug.LogError("Không tìm thấy asset SilverChestData trong ScriptableObjects/Chest/SilverChest!");
            return;
        }
        // Xóa 1 Silver Chest khỏi inventory
        InventoryManager.Instance.RemoveItemFromInventory(item.data, 1);
        // Mở chest và nhận thưởng
        ChestOpener.OpenChest(chestData, null);
        // Có thể cập nhật UI ở đây nếu cần
    }
}
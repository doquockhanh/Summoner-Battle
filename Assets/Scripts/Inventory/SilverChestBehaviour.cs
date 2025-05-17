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
        // TODO: Thực hiện animation mở hòm, random phần thưởng, v.v.
        if (chestData == null)
        {
            Debug.LogError("Không tìm thấy asset SilverChestData trong ScriptableObjects/Chest/SilverChest!");
            return;
        }
        ChestOpener.OpenChest(chestData, null);
    }
}
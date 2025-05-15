using UnityEngine;

public class SilverChestBehaviour : IItemUsable
{
    public void Use(Item item)
    {
        Debug.Log("Mở hòm bạc! (Chạy animation mở hòm ở đây)");
        // TODO: Thực hiện animation mở hòm, random phần thưởng, v.v.
    }
} 
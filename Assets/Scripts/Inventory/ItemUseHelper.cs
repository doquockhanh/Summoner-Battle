using UnityEngine;

public static class ItemUseHelper
{
    public static void UseItem(Item item)
    {
        var behaviour = ItemUseRegistry.GetBehaviour(item.data.id);
        if (behaviour != null)
            behaviour.Use(item);
        else
            Debug.Log($"Item '{item.data.itemName}' không thể sử dụng hoặc chưa có logic đặc biệt.");
    }
} 
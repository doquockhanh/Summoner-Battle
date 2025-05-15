using UnityEngine;
using System.Collections.Generic;

public static class ItemDatabase
{
    public static Dictionary<int, ItemData> ItemsById;

    public static void LoadAllItems()
    {
        ItemsById = new Dictionary<int, ItemData>();
        var allItems = Resources.LoadAll<ItemData>("Items");
        foreach (var item in allItems)
        {
            ItemsById[item.id] = item;
        }
    }

    public static ItemData GetItemDataById(int id)
    {
        if (ItemsById == null) LoadAllItems();
        ItemsById.TryGetValue(id, out var data);
        return data;
    }
} 
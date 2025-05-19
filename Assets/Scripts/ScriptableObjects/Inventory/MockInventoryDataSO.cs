using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/MockInventoryDataSO")]
public class MockInventoryDataSO : ScriptableObject
{
    [System.Serializable]
    public class MockItemEntry
    {
        public ItemData itemData;
        public int quantity;
    }

    public List<MockItemEntry> items = new List<MockItemEntry>();

    public void AddItem(ItemData data, int quantity)
    {
        var entry = items.Find(i => i.itemData == data);
        if (entry != null)
            entry.quantity += quantity;
        else
            items.Add(new MockItemEntry { itemData = data, quantity = quantity });
    }

    public void RemoveItem(ItemData data, int quantity)
    {
        var entry = items.Find(i => i.itemData == data);
        if (entry != null)
        {
            entry.quantity -= quantity;
            if (entry.quantity <= 0)
                items.Remove(entry);
        }
    }

    public void SetItem(ItemData data, int quantity)
    {
        var entry = items.Find(i => i.itemData == data);
        if (entry != null)
            entry.quantity = quantity;
        else
            items.Add(new MockItemEntry { itemData = data, quantity = quantity });
    }

    public void Clear()
    {
        items.Clear();
    }
} 
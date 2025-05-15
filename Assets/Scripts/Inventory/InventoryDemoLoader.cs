using System.Collections.Generic;
using UnityEngine;

public class InventoryDemoLoader : MonoBehaviour
{
    public List<Item> playerInventory = new List<Item>();

    void Start()
    {
        // Khi chưa có API thật, dùng mock data
        LoadInventoryFromMock();

        // Khi có API thật, thay thế bằng LoadInventoryFromApi();
    }

    void LoadInventoryFromMock()
    {
        ItemDatabase.LoadAllItems();
        var mockData = MockInventoryData.GetMockInventory();
        playerInventory.Clear();
        foreach (var entry in mockData)
        {
            var data = ItemDatabase.GetItemDataById(entry.id);
            if (data != null)
                playerInventory.Add(new Item(data, entry.quantity));
        }
        Debug.Log($"Inventory loaded from mock: {playerInventory.Count} items");
    }

    // Khi có API thật, thay thế hàm này bằng call tới server
    // Ví dụ:
    // IEnumerator LoadInventoryFromApi()
    // {
    //     yield return StartCoroutine(ApiManager.GetInventory((apiData) => {
    //         playerInventory.Clear();
    //         foreach (var entry in apiData)
    //         {
    //             var data = ItemDatabase.GetItemDataById(entry.id);
    //             if (data != null)
    //                 playerInventory.Add(new Item(data, entry.quantity));
    //         }
    //         Debug.Log($"Inventory loaded from API: {playerInventory.Count} items");
    //     }));
    // }
} 
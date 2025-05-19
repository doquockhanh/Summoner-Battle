using System.Collections;
using UnityEngine;
// using ScriptableObjects.Inventory; // Remove vì không có namespace

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public InventoryDataSO inventoryDataSO; // Gán asset runtime hoặc tạo mới
    public MockInventoryDataSO mockDataSO; // Gán asset mock database trong Inspector

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (inventoryDataSO == null)
        {
            inventoryDataSO = ScriptableObject.CreateInstance<InventoryDataSO>();
        }

        LoadInventory();
    }

    public void LoadInventory()
    {
        StartCoroutine(LoadInventoryCoroutine());
    }

    private IEnumerator LoadInventoryCoroutine()
    {
        bool apiSuccess = false;
        // TODO: Thay thế bằng call API thật khi có
        yield return StartCoroutine(LoadInventoryFromMockSO((success) => { apiSuccess = success; }));
        if (!apiSuccess)
        {
            Debug.LogWarning("Load inventory API failed, dùng data cũ trong InventoryDataSO");
        }
    }

    private IEnumerator LoadInventoryFromMockSO(System.Action<bool> onDone)
    {
        // Giả lập delay
        yield return new WaitForSeconds(0.2f);
        if (mockDataSO == null)
        {
            Debug.LogError("Chưa gán MockInventoryDataSO cho InventoryManager");
            onDone?.Invoke(false);
            yield break;
        }
        inventoryDataSO.items.Clear();
        foreach (var entry in mockDataSO.items)
        {
            if (entry.itemData != null)
                inventoryDataSO.items.Add(new Item(entry.itemData, entry.quantity));
        }
        Debug.Log($"Inventory loaded from mock SO: {inventoryDataSO.items.Count} items");
        onDone?.Invoke(true);
    }

    public void AddItemToInventory(ItemData itemData, int quantity = 1)
    {
        StartCoroutine(AddItemToInventoryCoroutine(itemData, quantity));
    }

    private IEnumerator AddItemToInventoryCoroutine(ItemData itemData, int quantity)
    {
        bool apiSuccess = false;
        // TODO: Thay thế bằng call API thật khi có
        yield return StartCoroutine(SimulateAddItemApi(itemData, quantity, (success) => { apiSuccess = success; }));
        if (apiSuccess)
        {
            // Thêm vào mock database
            if (mockDataSO != null)
                mockDataSO.AddItem(itemData, quantity);
            // Đồng bộ lại inventoryDataSO
            LoadInventory();
        }
        else
        {
            Debug.LogWarning("Add item API failed, không cập nhật InventoryDataSO");
        }
    }

    private IEnumerator SimulateAddItemApi(ItemData itemData, int quantity, System.Action<bool> onDone)
    {
        // Giả lập delay và thành công
        yield return new WaitForSeconds(0.1f);
        onDone?.Invoke(true);
    }
}

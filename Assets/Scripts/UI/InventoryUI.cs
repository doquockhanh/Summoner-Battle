using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;           // Gán prefab ItemSlot
    public Transform contentRoot;

    private List<GameObject> spawnedSlots = new List<GameObject>();
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject canvas;

    void Start()
    {
        closeButton.GetComponent<Button>().onClick.AddListener(Hide);
        InventoryManager.Instance.HasChange += ShowInventory;
    }

    void OnEnable()
    {
        ShowInventory();
    }

    public void ShowInventory()
    {
        // Xóa các slot cũ
        foreach (var slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();

        var inventorySO = InventoryManager.Instance.inventoryDataSO;

        if (inventorySO == null) return;

        foreach (var item in inventorySO.items)
        {
            var slotObj = Instantiate(itemSlotPrefab, contentRoot);
            var slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Setup(item);
            spawnedSlots.Add(slotObj);
        }
    }
    private void Hide()
    {
        canvas.SetActive(false);
    }

    void OnDestroy()
    {
        InventoryManager.Instance.HasChange -= ShowInventory;
    }
}
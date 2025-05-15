using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    public InventoryDemoLoader inventoryLoader; // Gán trong Inspector
    public GameObject itemSlotPrefab;           // Gán prefab ItemSlot
    public Transform contentRoot;               // Gán là InventoryPanel

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        StartCoroutine(ShowInventoryDelayed(2));
    }

    private IEnumerator ShowInventoryDelayed(int delay) {
        yield return new WaitForSeconds(delay);
        ShowInventory();
    }

    public void ShowInventory()
    {
        // Xóa các slot cũ
        foreach (var slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();

        // Hiển thị từng item
        foreach (var item in inventoryLoader.playerInventory)
        {
            var slotObj = Instantiate(itemSlotPrefab, contentRoot);
            var slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Setup(item);
            spawnedSlots.Add(slotObj);
        }
    }
}
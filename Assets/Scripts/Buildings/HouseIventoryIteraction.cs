using UnityEngine;
using UnityEngine.UI;

public class HouseIventoryInteraction : MonoBehaviour
{
    private InventoryUI inventoryUI;
    public GameObject inventoryPanel;
    public bool isOpen = true;

    void Start()
    {
        //isOpen = inventoryPanel.activeInHierarchy;
    }

    void OnMouseDown()
    {
        ToggleCardInventory();
    }

    public void ToggleCardInventory()
    {
        inventoryPanel.SetActive(true);
        Debug.Log("Toggle Card Inventory");
        //isOpen = !isOpen;


        // if (isOpen)
        // {
        //     if (inventoryUI == null)
        //     {
        //         inventoryUI = GetComponentInChildren<InventoryUI>();
        //     }
        //     //cardInventoryUI.LoadCards();
        // }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class HouseInteraction : MonoBehaviour
{
    private CardInventoryUI cardInventoryUI;
    public GameObject cardInventory;
    public bool isOpen = true;

    void Start () {
        cardInventoryUI = GetComponentInChildren<CardInventoryUI>();
        cardInventory = cardInventoryUI.gameObject;
        ToggleCardInventory();
    }

    void OnMouseDown()
    {
        ToggleCardInventory();
    }

    public void ToggleCardInventory()
    {
        Debug.Log("Toggle Card Inventory");
        isOpen = !isOpen;
        cardInventory.SetActive(isOpen);

        if (isOpen)
            cardInventoryUI.LoadCards();
    }

}

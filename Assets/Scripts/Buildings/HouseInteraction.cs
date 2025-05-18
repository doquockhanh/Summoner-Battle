using UnityEngine;
using UnityEngine.UI;

public class HouseInteraction : MonoBehaviour
{
    private CardInventoryUI cardInventoryUI;
    public GameObject cardInventory;
    public GameObject cardDetailPanel;
    public bool isOpen = true;

    void Start()
    {
        isOpen = cardInventory.activeInHierarchy;
        if (CardDetailPanel.Instance == null)
        {
            cardDetailPanel.SetActive(true);
        }
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
        {
            if (cardInventoryUI == null)
            {
                cardInventoryUI = GetComponentInChildren<CardInventoryUI>();
            }
            cardInventoryUI.LoadCards();
        }
    }
}

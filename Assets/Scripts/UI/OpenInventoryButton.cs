using UnityEngine;
using UnityEngine.UI;

public class OpenInventoryButton : MonoBehaviour
{
    [SerializeField] private CardInventoryUI inventoryUI;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OpenInventory);
    }

    private void OpenInventory()
    {
        inventoryUI.Show();
    }
} 
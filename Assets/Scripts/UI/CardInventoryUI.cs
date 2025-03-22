using UnityEngine;
using UnityEngine.UI;

public class CardInventoryUI : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private CardInventory cardInventory;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private float sizePerRow = 40f;
    private const float cardPerRow = 5;
    
    private void Start()
    {
        gameObject.SetActive(false);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        LoadCards();
    }

    public void Hide() 
    {
        gameObject.SetActive(false);
    }

    private void LoadCards()
    {
        // Xóa các card cũ
        foreach (Transform child in cardContainer) {
            Destroy(child.gameObject);
        }

        // Tạo card mới từ inventory
        foreach (Card card in cardInventory.availableCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            CardInventoryView cardView = cardObj.GetComponent<CardInventoryView>();
            cardView.Setup(card);
        }

        int row = Mathf.CeilToInt(cardInventory.availableCards.Count / cardPerRow);
        float cardContainerHeight = row * sizePerRow;
        float cardContainerWidth = cardContainer.GetComponent<RectTransform>().rect.width;
        cardContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(cardContainerWidth, cardContainerHeight);
    }
} 
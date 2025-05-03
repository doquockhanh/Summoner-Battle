using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ResourcePoint : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public List<Image> defenseCardsUI;
    public Text RBName;
    public Button leaveButton;
    public Button combatButton;
    public List<Image> attackerCardsUi;
    public Transform InventoryScrollViewContent;
    public GameObject inventoryCardPrefab;

    public CardInventory cardInventory;
    public CardStorage cardStorageHolder;

    private CameraController cameraController;
    [HideInInspector] public ResourceBattleground resourceBattleground;
    private List<string> choosenAttackerIds = new List<string>(5);

    private void Start()
    {
        // Tìm CameraController
        cameraController = FindObjectOfType<CameraController>();

        SetupPanelInfo();

        for (int i = choosenAttackerIds.Count; i < 5; i++)
            choosenAttackerIds.Add(null);

        // Thêm collider cho điểm tài nguyên nếu chưa có
        if (!GetComponent<Collider2D>())
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void SetCardInSlot(int index, string id)
    {
        if (index >= 0 && index < choosenAttackerIds.Count)
        {
            choosenAttackerIds[index] = id;
        }
    }

    public string GetCardInSlot(int index)
    {
        return choosenAttackerIds[index];
    }

    private void SetupPanelInfo()
    {
        ResourcePointManager resourcePointManager = FindFirstObjectByType<ResourcePointManager>();
        resourcePointManager.SaveNewResourceBattleGround(this);
        if (resourceBattleground == null) return;

        RBName.text = "Fake name";
        List<CardInfo> defenseCards = resourceBattleground.cardInfos;
        List<Card> cardStorage = cardStorageHolder.cards;
        for (int i = 0; i < defenseCards.Count; i++)
        {
            Card card = cardStorage.Where(c => c.id == defenseCards[i].id).FirstOrDefault();
            defenseCardsUI[i].sprite = card.cardImage;
        }


        // Hiển thị các card ở inventory ra scroll view
        List<Card> cards = cardInventory.availableCards;
        foreach (var card in cards)
        {
            GameObject cardPrefab = Instantiate(inventoryCardPrefab, InventoryScrollViewContent); // Gắn vào content
            Image image = cardPrefab.GetComponentsInChildren<Image>().Last();
            DraggableCard draggableCard = cardPrefab.GetComponent<DraggableCard>();

            if (draggableCard != null)
                draggableCard.id = card.id;

            if (image != null)
                image.sprite = card.cardImage;
        }

        // Ẩn popup khi bắt đầu
        popupPanel.SetActive(false);

        // Thiết lập các button
        leaveButton.onClick.AddListener(ClosePopup);
        combatButton.onClick.AddListener(StartCombat);

    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (cameraController != null)
        {
            cameraController.FocusOnPosition(transform.position);
        }

        if (!popupPanel.activeSelf)
        {
            ShowPopup();
        }
    }

    private void ShowPopup()
    {
        ResourcePoint[] resourcePoints = FindObjectsOfType<ResourcePoint>();
        foreach (ResourcePoint resourcePoint in resourcePoints)
        {
            resourcePoint.ClosePopup();
        }
        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void StartCombat()
    {
        // assign data for BattleDataManager (instace not destroy on load)
        BattleDataManager.Instance.defenderIDs = resourceBattleground.cardInfos.Select(c => c.id).ToList();
        foreach (string id in BattleDataManager.Instance.defenderIDs) {
            Debug.Log(id);
        }
        BattleDataManager.Instance.attackerIDs = choosenAttackerIds;

        SceneManager.LoadScene("Battle");
    }
}
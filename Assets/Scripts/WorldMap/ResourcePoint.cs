using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ResourcePoint : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public List<Image> defenseCardsUI;
    public Text RBName;
    public Button leaveButton;
    public Button combatButton;
    public CardInventory cardInventory;

    private CameraController cameraController;
    [HideInInspector] public ResourceBattleground resourceBattleground;

    private void Start()
    {
        // Tìm CameraController
        cameraController = FindObjectOfType<CameraController>();

        SetupPanelInfo();

        // Thêm collider cho điểm tài nguyên nếu chưa có
        if (!GetComponent<Collider2D>())
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void SetupPanelInfo()
    {
        ResourcePointManager resourcePointManager = FindFirstObjectByType<ResourcePointManager>();
        resourcePointManager.SaveNewResourceBattleGround(this);
        if (resourceBattleground == null) return;

        RBName.text = "Fake name";
        List<CardInfo> defenseCards = resourceBattleground.cardInfos;
        // Debug.Log(defenseCards.Count);
        List<Card> cards = cardInventory.availableCards;
        for (int i = 0; i < defenseCards.Count; i++)
        {
            Card card = cards.Where(c => c.id == defenseCards[i].id).FirstOrDefault();
            defenseCardsUI[i].sprite = card.cardImage;
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
        // TODO: Implement combat logic
        Debug.Log("Starting combat at ");
    }
}
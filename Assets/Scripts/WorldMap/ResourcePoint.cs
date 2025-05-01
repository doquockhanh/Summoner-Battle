using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourcePoint : MonoBehaviour
{
    [Header("Resource Information")]
    public string resourceName;
    public string description;
    public int resourceLevel;
    public Sprite resourceIcon;

    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public Image iconImage;
    public Button leaveButton;
    public Button combatButton;

    private CameraController cameraController;

    private void Start()
    {
        // Tìm CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController not found in scene!");
        }

        // Ẩn popup khi bắt đầu
        popupPanel.SetActive(false);

        // Thiết lập các button
        leaveButton.onClick.AddListener(ClosePopup);
        combatButton.onClick.AddListener(StartCombat);

        // Thêm collider cho điểm tài nguyên
        if (!GetComponent<Collider2D>())
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void OnMouseDown()
    {
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
        // Cập nhật thông tin trên popup
        titleText.text = resourceName;
        descriptionText.text = description;
        levelText.text = "Level: " + resourceLevel;
        iconImage.sprite = resourceIcon;

        // Hiển thị popup
        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void StartCombat()
    {
        // TODO: Implement combat logic
        Debug.Log("Starting combat at " + resourceName);
    }

    private void Update()
    {
        // Kiểm tra click bên ngoài popup
        if (popupPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                popupPanel.GetComponent<RectTransform>(),
                mousePosition))
            {
                ClosePopup();
            }
        }
    }
}
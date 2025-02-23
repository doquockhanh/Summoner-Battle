using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitStatsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI rangeText;
    
    private Unit targetUnit;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset = new Vector2(0, 1f); // Offset so với unit (theo đơn vị world space)
    
    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);
    }
    
    public void ShowStats(Unit unit)
    {
        if (unit == null) return;
        
        targetUnit = unit;
        UpdateStats();
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        targetUnit = null;
        gameObject.SetActive(false);
    }
    
    private void LateUpdate()
    {
        if (targetUnit == null || targetUnit.IsDead)
        {
            Hide();
            return;
        }
        
        UpdatePosition();
        UpdateStats();
    }
    
    private void UpdatePosition()
    {
        if (targetUnit == null || mainCamera == null) return;

        // Lấy vị trí world space của unit + offset
        Vector3 targetPosition = targetUnit.transform.position + (Vector3)offset;
        
        // Chuyển sang viewport space (0-1)
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(targetPosition);
        
        // Kiểm tra xem unit có nằm trong tầm nhìn camera không
        if (viewportPoint.z < 0)
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Chuyển viewport space sang canvas space
        Vector2 screenPoint = new Vector2(
            viewportPoint.x * canvas.pixelRect.width,
            viewportPoint.y * canvas.pixelRect.height
        );
        
        // Tính toán giới hạn để panel không đi ra ngoài màn hình
        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;
        
        // Giới hạn trong canvas
        screenPoint.x = Mathf.Clamp(
            screenPoint.x,
            panelWidth/2,
            canvas.pixelRect.width - panelWidth/2
        );
        
        screenPoint.y = Mathf.Clamp(
            screenPoint.y,
            panelHeight/2,
            canvas.pixelRect.height - panelHeight/2
        );
        
        // Chuyển sang anchored position cho RectTransform
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPoint,
            canvas.worldCamera,
            out anchoredPosition
        );
        
        rectTransform.anchoredPosition = anchoredPosition;
    }
    
    private void UpdateStats()
    {
        UnitStats stats = targetUnit.GetUnitStats();
        UnitData data = stats.Data;
        
        unitNameText.text = data.unitName;
        hpText.text = $"HP: {stats.CurrentHP}/{data.maxHp}";
        damageText.text = $"Sát thương vật lý: {data.physicalDamage}";
        attackSpeedText.text = $"Tốc độ đánh: {data.attackSpeed}";
        moveSpeedText.text = $"Tốc độ di chuyển: {data.moveSpeed}";
        rangeText.text = $"Tầm đánh: {data.range}";
    }
} 
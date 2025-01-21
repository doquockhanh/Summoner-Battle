using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private Slider healthBar;
    [SerializeField] private ParticleSystem attackEffect;
    
    private Unit unit;
    private float maxHp;
    
    public void Setup(UnitData data, Unit unitComponent)
    {
        unit = unitComponent;
        maxHp = data.hp;
        
        // Cập nhật sprite
        if (unitSprite != null && data.unitImage != null)
        {
            unitSprite.sprite = data.unitImage;
        }
        
        // Thay đổi màu để phân biệt phe
        if (unitSprite != null)
        {
            Color teamColor = unitComponent.isPlayerUnit ? new Color(0.7f, 0.7f, 1f) : new Color(1f, 0.7f, 0.7f);
            unitSprite.color = teamColor;
        }
        
        // Chuyển thanh máu vào canvas chung
        if (healthBar != null)
        {
            healthBar.transform.SetParent(HealthBarManager.Instance.GetCanvasTransform(), true);
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
            
            // Đảm bảo hướng slider thống nhất
            if (!unitComponent.isPlayerUnit)
            {
                healthBar.direction = Slider.Direction.RightToLeft;
            }
        }
    }
    
    public void UpdateHealth(float currentHp)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHp;
            
            // Thay đổi màu
            Image fillImage = healthBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float healthPercent = Mathf.Clamp01(currentHp / maxHp);
                fillImage.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }
    }
    
    public void PlayAttackEffect()
    {
        if (attackEffect != null)
            attackEffect.Play();
    }
    
    private void LateUpdate()
    {
        if (healthBar != null)
        {
            // Cập nhật vị trí thanh máu theo unit
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            
            // Offset để thanh máu nằm phía trên unit
            screenPos.y += 50f; // Điều chỉnh số 50 tùy theo kích thước unit
            
            // Chuyển từ screen position sang world position
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            healthBar.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
        }
    }
    
    private void OnEnable()
    {
        // Hiện thanh máu khi unit được kích hoạt từ pool
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // Ẩn thanh máu khi unit được trả về pool
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
    }
} 
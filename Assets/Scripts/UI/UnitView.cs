using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private ParticleSystem attackEffect;
    
    private Unit unit;
    private float maxHp;
    private UnitStats stats;
    
    public void Initialize(Unit unit, UnitStats stats)
    {
        this.unit = unit;
        this.stats = stats;
        maxHp = stats.Data.hp;
        
        // Cập nhật sprite
        if (unitSprite != null && stats.Data.unitImage != null)
        {
            unitSprite.sprite = stats.Data.unitImage;
        }
        
        // Thay đổi màu để phân biệt phe
        if (unitSprite != null)
        {
            Color teamColor = unit.IsPlayerUnit ? new Color(0.7f, 0.7f, 1f) : new Color(1f, 0.7f, 0.7f);
            unitSprite.color = teamColor;
        }
        
        // Chuyển thanh máu vào canvas chung
        if (healthBar != null)
        {
            healthBar.transform.SetParent(HealthBarManager.Instance.GetCanvasTransform(), true);
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
            
            // Đảm bảo hướng slider thống nhất
            if (!unit.IsPlayerUnit)
            {
                healthBar.direction = Slider.Direction.RightToLeft;
            }
        }
        
        // Tạo health bar và shield bar
        if (healthBar == null)
        {
            healthBar = CreateBar(Color.green);
            healthBar.transform.localPosition = new Vector3(0, 50, 0);
        }
        
        if (shieldBar == null)
        {
            shieldBar = CreateBar(Color.cyan);
            shieldBar.transform.localPosition = new Vector3(0, 55, 0); // Đặt cao hơn health bar
        }
        
        // Setup các giá trị ban đầu
        healthBar.maxValue = stats.MaxHp;
        healthBar.value = stats.CurrentHP;
        
        shieldBar.maxValue = stats.MaxHp;
        shieldBar.value = 0;
        shieldBar.gameObject.SetActive(false);
        
        // Subscribe vào các event
        unit.OnShieldChanged += UpdateShieldBar;
    }
    
    private Slider CreateBar(Color color)
    {
        GameObject barObj = Instantiate(healthBar.gameObject, transform);
        Slider bar = barObj.GetComponent<Slider>();
        bar.fillRect.GetComponent<Image>().color = color;
        return bar;
    }
    
    private void UpdateShieldBar(float shieldAmount)
    {
        shieldBar.gameObject.SetActive(shieldAmount > 0);
        shieldBar.value = shieldAmount;
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

    private void Update()
    {
        if (healthBar != null && stats != null)
        {
            healthBar.value = stats.CurrentHP;
            
            // Thay đổi màu dựa vào % máu
            Image fillImage = healthBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float healthPercent = Mathf.Clamp01(stats.CurrentHP / maxHp);
                fillImage.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }
    }

    private void OnDestroy()
    {
        if (unit != null)
        {
            unit.OnShieldChanged -= UpdateShieldBar;
        }
    }
} 
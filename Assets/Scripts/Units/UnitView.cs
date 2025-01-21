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
        Debug.Log($"Setting up health bar with max HP: {data.hp}");
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
        
        // Thiết lập thanh máu
        healthBar.maxValue = maxHp;
        healthBar.value = maxHp;
        
        // Đảm bảo Canvas luôn hướng về camera
        Canvas canvas = healthBar.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }
        
        // Đảm bảo hướng slider thống nhất
        if (!unitComponent.isPlayerUnit)
        {
            // Đảo ngược direction của slider cho enemy units
            healthBar.direction = Slider.Direction.RightToLeft;
        }
    }
    
    public void UpdateHealth(float currentHp)
    {
        Debug.Log($"Updating health: {currentHp}/{maxHp}");
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
} 
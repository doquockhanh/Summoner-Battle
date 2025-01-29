using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private ParticleSystem attackEffect;
    
    private float maxHp;
    private readonly Color healthyColor = Color.green;
    private readonly Color criticalColor = Color.red;
    private static readonly int FlashProperty = Shader.PropertyToID("_Flash");
    
    private Material spriteMaterial;
    private Coroutine flashCoroutine;

    private void OnEnable()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            UpdateHealthBarPosition();
        }
    }

    private void OnDisable()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        UpdateHealthBarPosition();
    }

    public void Initialize(Unit unit)
    {
        if (unitSprite == null) unitSprite = GetComponentInChildren<SpriteRenderer>();
        spriteMaterial = unitSprite.material;
        
        var stats = unit.GetComponent<UnitStats>();
        maxHp = stats.MaxHp;
        
        SetupHealthBar();
        SetupShieldBar();
        
        // Subscribe to events
        stats.OnHealthChanged += UpdateHealth;
        stats.OnShieldChanged += UpdateShieldBar;
        stats.OnDeath += OnUnitDeath;
    }

    private void SetupHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.transform.SetParent(HealthBarManager.Instance.GetCanvasTransform(), true);
            healthBar.gameObject.SetActive(true);
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
            UpdateHealthBarPosition();
            UpdateHealthBarColor(maxHp / maxHp);
        }
    }

    private void SetupShieldBar()
    {
        if (shieldBar != null)
        {
            shieldBar.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBar != null)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 0.5f;
            healthBar.transform.position = worldPosition;
        }
    }

    public void UpdateHealth(float currentHp)
    {
        if (healthBar == null) return;
        
        healthBar.value = currentHp;
        UpdateHealthBarColor(currentHp / maxHp);
        
        // Flash effect khi nhận damage
        if (currentHp < healthBar.value)
        {
            PlayDamageFlash();
        }
    }

    private void UpdateHealthBarColor(float healthPercent)
    {
        if (healthBar?.fillRect?.GetComponent<Image>() is Image fillImage)
        {
            fillImage.color = Color.Lerp(criticalColor, healthyColor, healthPercent);
        }
    }

    private void UpdateShieldBar(float shieldAmount)
    {
        if (shieldBar == null) return;
        
        shieldBar.gameObject.SetActive(shieldAmount > 0);
        shieldBar.value = shieldAmount;
    }

    public void PlayAttackEffect()
    {
        if (attackEffect != null && !attackEffect.isPlaying)
        {
            attackEffect.Play();
        }
    }

    private void PlayDamageFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(DamageFlashCoroutine());
    }

    private IEnumerator DamageFlashCoroutine()
    {
        spriteMaterial.SetFloat(FlashProperty, 1f);
        
        float flashDuration = 0.1f;
        float elapsedTime = 0;
        
        while (elapsedTime < flashDuration)
        {
            float flashStrength = Mathf.Lerp(1f, 0f, elapsedTime / flashDuration);
            spriteMaterial.SetFloat(FlashProperty, flashStrength);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteMaterial.SetFloat(FlashProperty, 0f);
    }

    private void OnUnitDeath()
    {
        // Có thể thêm animation chết ở đây
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (shieldBar != null) shieldBar.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
    }
}
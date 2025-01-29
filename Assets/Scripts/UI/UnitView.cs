using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private ParticleSystem attackEffect;
    
    private HealthBarUI healthBarUI;
    private Material spriteMaterial;
    private Coroutine flashCoroutine;
    private static readonly int FlashProperty = Shader.PropertyToID("_Flash");

    private void OnEnable()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Show();
            UpdateHealthBarPosition();
        }
    }

    private void OnDisable()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
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
        SetupHealthBar(stats.MaxHp);
        
        // Subscribe to events
        stats.OnHealthChanged += UpdateHealth;
        stats.OnShieldChanged += UpdateShield;
        stats.OnDeath += OnUnitDeath;
    }

    private void SetupHealthBar(float maxHp)
    {
        if (healthBarUI == null && healthBarPrefab != null)
        {
            var healthBarObject = Instantiate(healthBarPrefab, HealthBarManager.Instance.GetCanvasTransform());
            healthBarUI = healthBarObject.GetComponent<HealthBarUI>();
            healthBarUI.transform.SetParent(HealthBarManager.Instance.GetCanvasTransform(), true); // Gắn healthbar vào unit
        }
        
        healthBarUI.Initialize(maxHp);
        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarUI != null)
        {
            Vector3 worldPosition = transform.position + Vector3.up * 0.5f;
            healthBarUI.transform.position = worldPosition;
        }
    }

    private void UpdateHealth(float newHp)
    {
        healthBarUI.UpdateHealth(newHp);
        PlayDamageFlash();
    }

    private void UpdateShield(float shieldAmount)
    {
        healthBarUI.UpdateShield(shieldAmount);
    }

    private void OnUnitDeath()
    {
        healthBarUI.Hide();
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

    private void OnDestroy()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        if (healthBarUI != null)
        {
            Destroy(healthBarUI.gameObject);
        }
    }
}

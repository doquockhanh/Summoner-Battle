using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private Animator animator;

    private HealthBarUI healthBarUI;
    private Material spriteMaterial;
    private static readonly int FlashProperty = Shader.PropertyToID("_Flash");
    private static readonly int IsMovingParam = Animator.StringToHash("isMoving");
    private static readonly int AttackParam = Animator.StringToHash("attack");
    private static readonly int skillAnimParam = Animator.StringToHash("runSkill");

    private Unit unit;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

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
        this.unit = unit;
        if (unitSprite == null) unitSprite = GetComponentInChildren<SpriteRenderer>();
        spriteMaterial = unitSprite.material;

        var stats = unit.GetComponent<UnitStats>();
        SetupHealthBar(stats.GetMaxHp());
        SetupOutline();

        // Subscribe to events
        stats.OnHealthChanged += UpdateHealth;
        stats.OnShieldChanged += UpdateShield;
        stats.OnDeath += OnUnitDeath;
    }

    public void SetMoving(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool(IsMovingParam, isMoving);
        }
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackParam);
            SetMoving(false);
        }
    }


    public void PlaySkillAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger(skillAnimParam);
        SetMoving(false);
    }

    public void FlipSprite(bool faceRight)
    {
        if (unitSprite != null)
        {
            unitSprite.flipX = !faceRight;
        }
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
            Vector3 worldPosition = GetHealthBarPosition();
            healthBarUI.transform.position = worldPosition;
        }
    }

    private Vector3 GetHealthBarPosition()
    {
        if (unitSprite == null) return transform.position;

        // Lấy bounds của sprite trong không gian world
        Bounds spriteBounds = unitSprite.bounds;

        // Tính toán vị trí trên cùng của sprite
        Vector3 topPosition = transform.position;
        topPosition.y += spriteBounds.extents.y * 2;

        // Thêm một khoảng nhỏ để health bar không bị dính vào sprite
        topPosition.y += 0.1f;

        return topPosition;
    }

    private void UpdateHealth(float newHp)
    {
        healthBarUI.UpdateHealth(newHp, unit.GetComponent<UnitStats>().GetMaxHp());
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
        StopAllCoroutines();
        this.StartCoroutineSafely(DamageFlashCoroutine());
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
        StopAllCoroutines();

        if (healthBarUI != null)
        {
            Destroy(healthBarUI.gameObject);
        }
    }

    public HealthBarUI GetHealthBar()
    {
        return healthBarUI;
    }

    private void SetupOutline()
    {
        if (unitSprite != null && MaterialManager.Instance != null)
        {
            unitSprite.material = MaterialManager.Instance.GetUnitMaterial(unit.IsPlayerUnit);
        }
    }

    public void SetAlpha(float alpha)
    {
        if (unitSprite != null)
        {
            Color color = unitSprite.color;
            color.a = alpha;
            unitSprite.color = color;
        }
    }
}
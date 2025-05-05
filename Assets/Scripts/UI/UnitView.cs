using System.Collections;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer unitSprite;
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private Animator animator;
    [SerializeField] private HealthBarController healthBarController;

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
        if (healthBarController != null)
        {
            healthBarController.Show();
        }
    }

    private void OnDisable()
    {
        if (healthBarController != null)
        {
            healthBarController.Hide();
        }
    }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        if (unitSprite == null) unitSprite = GetComponentInChildren<SpriteRenderer>();
        spriteMaterial = unitSprite.material;

        var stats = unit.GetComponent<UnitStats>();
        healthBarController.Initialize(stats.GetMaxHp(), unit.IsPlayerUnit);
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

    private void UpdateHealth(float newHp)
    {
        healthBarController.UpdateHealth(newHp, unit.GetComponent<UnitStats>().GetMaxHp());
        PlayDamageFlash();
    }

    private void UpdateShield(float shieldAmount)
    {
        healthBarController.UpdateShield(shieldAmount);
    }

    private void OnUnitDeath()
    {
        healthBarController.Hide();
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

        if (healthBarController != null)
        {
            Destroy(healthBarController.gameObject);
        }
    }

    public HealthBarUI GetHealthBar()
    {
        return healthBarController != null ? healthBarController.GetHealthBarUI() : null;
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
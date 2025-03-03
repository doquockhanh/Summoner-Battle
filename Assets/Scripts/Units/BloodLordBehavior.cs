using UnityEngine;

[RequireComponent(typeof(Unit))]
public class BloodLordBehavior : MonoBehaviour
{
    #region Constants
    private const int MAX_SOULS = 5;
    public const float BLOODSTORM_HP_THRESHOLD = 0.5f;
    private const float SOUL_COUNTER_Y_OFFSET = 1f;
    #endregion

    #region Components
    private Unit unit;
    private UnitStats stats;
    private UnitCombat combat;
    private UnitTargeting targeting;
    private UnitStatusEffects statusEffects;
    private UnitMovement movement;
    private UnitView view;
    private HealthBarUI healthBar;
    private Animator animator;
    private static readonly int SpawnTrigger = Animator.StringToHash("spawn");
    private static readonly int UseSkillBool = Animator.StringToHash("useSkill");
    #endregion

    #region State
    private BloodstormSkill skill;
    private int absorbedSouls;
    private bool isBloodstormActive;
    private float damageTimer;
    #endregion

    #region Events
    public event System.Action<int> OnSoulCountChanged;
    public event System.Action OnBloodstormActivated;
    #endregion

    public void Initialize(BloodstormSkill bloodstormSkill)
    {
        if (bloodstormSkill == null)
            throw new System.ArgumentNullException(nameof(bloodstormSkill));

        InitializeComponents();
        InitializeState(bloodstormSkill);
        RegisterEvents();
    }

    private void InitializeComponents()
    {
        unit = GetComponent<Unit>();
        stats = GetComponent<UnitStats>();
        combat = GetComponent<UnitCombat>();
        targeting = GetComponent<UnitTargeting>();
        statusEffects = GetComponent<UnitStatusEffects>();
        movement = GetComponent<UnitMovement>();
        view = GetComponent<UnitView>();
        healthBar = view?.GetHealthBar();
        animator = GetComponent<Animator>();

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (unit == null || stats == null || combat == null ||
            targeting == null || statusEffects == null ||
            movement == null || view == null || healthBar == null || animator == null)
        {
            throw new MissingComponentException("Required components not found on BloodLord");
        }

        // Kích hoạt animation spawn ngay khi khởi tạo
        animator.SetTrigger(SpawnTrigger);
    }

    private void InitializeState(BloodstormSkill bloodstormSkill)
    {
        skill = bloodstormSkill;
        absorbedSouls = 0;
        isBloodstormActive = false;
        combat.enabled = false;

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        healthBar.ShowSoulCounter(true);
        healthBar.UpdateSoulCount(absorbedSouls);
    }

    private void RegisterEvents()
    {
        UnitEvents.Status.OnUnitDeath += OnUnitDeath;
    }

    private void OnUnitDeath(Unit deadUnit)
    {
        if (ShouldSkipSoulAbsorption(deadUnit)) return;
        if (!IsClosestBloodLordTo(deadUnit)) return;

        TryAbsorbSoul(deadUnit);
    }

    private bool ShouldSkipSoulAbsorption(Unit deadUnit)
    {
        return isBloodstormActive ||
               deadUnit == unit ||
               deadUnit == null;
    }

    private bool IsClosestBloodLordTo(Unit deadUnit)
    {
        float myDistance = Vector2.Distance(transform.position, deadUnit.transform.position);
        var bloodLords = FindObjectsOfType<BloodLordBehavior>();

        foreach (var bloodLord in bloodLords)
        {
            if (bloodLord == this) continue;

            float otherDistance = Vector2.Distance(
                bloodLord.transform.position,
                deadUnit.transform.position
            );

            if (otherDistance < myDistance) return false;
        }

        return true;
    }

    private void TryAbsorbSoul(Unit deadUnit)
    {
        if (absorbedSouls >= MAX_SOULS) return;

        absorbedSouls++;
        healthBar.UpdateSoulCount(absorbedSouls);
        OnSoulCountChanged?.Invoke(absorbedSouls);

        SpawnSoulAbsorbEffect(deadUnit.transform.position);
        UnitEvents.Status.RaiseSoulAbsorbed(unit, deadUnit);

        CheckBloodstormCondition();
    }

    private void SpawnSoulAbsorbEffect(Vector3 position)
    {
        if (skill.soulAbsorbEffectPrefab == null) return;

        var effect = Instantiate(
            skill.soulAbsorbEffectPrefab,
            position,
            Quaternion.identity
        );
        Destroy(effect, BloodLordConfig.EFFECT_DURATION);
    }

    private void CheckBloodstormCondition()
    {
        bool shouldActivate = !isBloodstormActive &&
            (absorbedSouls >= MAX_SOULS ||
             stats.CurrentHealthPercent <= BLOODSTORM_HP_THRESHOLD);

        if (shouldActivate)
        {
            ActivateBloodstorm();
        }
    }

    private void ActivateBloodstorm()
    {
        isBloodstormActive = true;
        combat.enabled = true;
        animator.SetBool(UseSkillBool, true);

        ApplyBloodstormEffect();
        SpawnBloodstormVisualEffect();
        ResetTimers();

        NotifyBloodstormActivation();
    }

    private void ApplyBloodstormEffect()
    {
        var effect = new BloodstormStatusEffect(unit, skill, absorbedSouls);
        statusEffects.AddEffect(effect);
    }

    private void SpawnBloodstormVisualEffect()
    {
        if (skill.bloodstormEffectPrefab == null) return;

        var effect = Instantiate(
            skill.bloodstormEffectPrefab,
            transform
        );
        effect.transform.localPosition = Vector3.zero;
    }

    private void ResetTimers()
    {
        damageTimer = 0f;
    }

    private void NotifyBloodstormActivation()
    {
        OnBloodstormActivated?.Invoke();
        UnitEvents.Status.RaiseSkillActivated(unit, skill);

        FloatingTextManager.Instance.ShowFloatingText(
            $"Bloodstorm ({absorbedSouls} Souls)",
            transform.position + Vector3.up * SOUL_COUNTER_Y_OFFSET,
            Color.red
        );
    }

    private void FixedUpdate()
    {
        if (isBloodstormActive)
        {
            UpdateDamageTimer();
            HandleBloodstormMovement();
        }
        else
        {
            CheckBloodstormCondition();
        }
    }

    private void UpdateDamageTimer()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0)
        {
            DealBloodstormDamage();
            damageTimer = skill.damageInterval;
        }
    }

    private void DealBloodstormDamage()
    {
        float baseDamage = stats.GetMagicDamage();
        float totalDamagePercent = CalculateTotalDamagePercent();
        float actualDamage = baseDamage * (totalDamagePercent / 100f);

        Unit[] nearbyUnits = targeting.GetUnitsInRange(skill.effectRadius);
        foreach (var enemy in nearbyUnits)
        {
            if (IsValidTarget(enemy))
            {
                enemy.TakeDamage(actualDamage, DamageType.Magic, unit);
            }
        }
    }

    private float CalculateTotalDamagePercent()
    {
        return skill.damageBasePercent +
               (skill.damagePerSoulPercent * absorbedSouls);
    }

    private bool IsValidTarget(Unit target)
    {
        return target != null &&
               target != unit &&
               target.IsPlayerUnit != unit.IsPlayerUnit;
    }

    private void HandleBloodstormMovement()
    {
        if (!isBloodstormActive) return;

        if (movement.TargetPosition == Vector3.zero ||
            Vector2.Distance(transform.position, movement.TargetPosition) < 0.1f)
        {
            Vector3 newPos = RandomMovementHandler.Instance.GetNextRandomPosition(
                transform.position,
                unit.IsPlayerUnit
            );

            movement.SetTargetPosition(newPos);
        }

        Vector3 direction = (movement.TargetPosition - transform.position).normalized;
        movement.SetMoveDirection(direction);
    }

    private void OnDestroy()
    {
        UnitEvents.Status.OnUnitDeath -= OnUnitDeath;

        // Reset animation khi destroy
        if (animator != null)
        {
            animator.SetBool(UseSkillBool, false);
        }
    }
}

public static class BloodLordConfig
{
    public const float MAP_WIDTH = 10f;
    public const float MAX_STAT_MULTIPLIER = 1.5f;
    public const float MIN_STAT_MULTIPLIER = 0.5f;
    public const float EFFECT_DURATION = 1f;
}
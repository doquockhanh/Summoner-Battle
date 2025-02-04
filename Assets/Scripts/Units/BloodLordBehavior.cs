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
    private HealthBarUI healthBar;
    #endregion

    #region State
    private BloodstormSkill skill;
    private int absorbedSouls;
    private bool isBloodstormActive;
    private float damageTimer;
    private float healingTimer;
    private float healingDecreaseTimer;
    private float currentHealPercent;
    private bool isHealingDecreaseComplete;
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
        healthBar = unit.GetComponent<UnitView>()?.GetHealthBar();

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (unit == null || stats == null || combat == null || 
            targeting == null || statusEffects == null || 
            movement == null || healthBar == null)
        {
            throw new MissingComponentException("Required components not found on BloodLord");
        }
    }

    private void InitializeState(BloodstormSkill bloodstormSkill)
    {
        skill = bloodstormSkill;
        absorbedSouls = 0;
        isBloodstormActive = false;
        combat.enabled = false;

        AdjustStatsByPosition();
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

    private void AdjustStatsByPosition()
    {
        // Chuẩn hóa vị trí X về khoảng 0-1
        float normalizedX = (transform.position.x + BloodLordConfig.MAP_WIDTH/2) / BloodLordConfig.MAP_WIDTH;

        // Đảo ngược normalizedX nếu là unit của player
        if (unit.IsPlayerUnit)
        {
            normalizedX = 1 - normalizedX;
        }

        // Càng xa căn cứ (normalizedX càng lớn):
        // - Tăng defense (1/multiplier càng lớn)
        // - Giảm damage (multiplier càng nhỏ)
        float multiplier = Mathf.Lerp(
            BloodLordConfig.MAX_STAT_MULTIPLIER,  // Gần căn cứ: damage cao, defense thấp
            BloodLordConfig.MIN_STAT_MULTIPLIER,  // Xa căn cứ: damage thấp, defense cao
            normalizedX
        );

        Debug.Log($"[BloodLord] Vị trí X: {transform.position.x}, NormalizedX: {normalizedX}, " +
                  $"Damage Multiplier: {multiplier}, Defense Multiplier: {1/multiplier}");

        stats.ModifyDamage(multiplier);
        stats.ModifyDefense(1f / multiplier);
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
        healingTimer = 0f;
        healingDecreaseTimer = 0f;
        currentHealPercent = skill.healingStartPercent;
        isHealingDecreaseComplete = false;
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

    private void Update()
    {
        if (isBloodstormActive)
        {
            HandleDamageAndHealing();
            HandleBloodstormMovement();
        }
        else
        {
            CheckBloodstormCondition();
        }
    }

    private void HandleDamageAndHealing()
    {
        UpdateDamageTimer();
        UpdateHealingTimer();
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

    private void UpdateHealingTimer()
    {
        if (isHealingDecreaseComplete && currentHealPercent > 0)
            return;

        healingTimer -= Time.deltaTime;
        if (healingTimer <= 0)
        {
            healingTimer = 1f;
            
            if (healingDecreaseTimer < skill.healingDecreaseDuration)
            {
                healingDecreaseTimer += 1f;
                currentHealPercent = Mathf.Max(
                    currentHealPercent - skill.healingDecreasePercent, 
                    0
                );
                
                Debug.Log($"[BloodLord] Giảm healing: {currentHealPercent}% " +
                         $"(Thời gian: {healingDecreaseTimer}/{skill.healingDecreaseDuration}s)");

                if (healingDecreaseTimer >= skill.healingDecreaseDuration)
                {
                    isHealingDecreaseComplete = true;
                    Debug.Log($"[BloodLord] Kết thúc giảm healing. " +
                            $"Healing cuối cùng: {currentHealPercent}%");
                }
            }
        }
    }

    private void DealBloodstormDamage()
    {
        float baseDamage = stats.GetModifiedDamage();
        float totalDamagePercent = CalculateTotalDamagePercent();
        float actualDamage = baseDamage * (totalDamagePercent / 100f);

        Unit[] nearbyUnits = targeting.GetUnitsInRange(skill.effectRadius);
        foreach (var enemy in nearbyUnits)
        {
            if (IsValidTarget(enemy))
            {
                DealDamageAndHeal(enemy, actualDamage);
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

    private void DealDamageAndHeal(Unit enemy, float damage)
    {
        enemy.TakeDamage(damage);
        
        if (currentHealPercent > 0)
        {
            float healAmount = damage * (currentHealPercent / 100f);
            stats.Heal(healAmount);
            
            Debug.Log($"[BloodLord] Hồi {healAmount} máu " +
                     $"(Tỉ lệ hồi máu: {currentHealPercent}%)");
        }
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
            
            Debug.Log($"[BloodLord] Di chuyển đến vị trí mới: {newPos}");
            movement.SetTargetPosition(newPos);
        }

        Vector3 direction = (movement.TargetPosition - transform.position).normalized;
        movement.SetMoveDirection(direction);
    }

    private void OnDestroy()
    {
        UnitEvents.Status.OnUnitDeath -= OnUnitDeath;
    }
}

// Config class để tránh magic numbers
public static class BloodLordConfig
{
    public const float MAP_WIDTH = 10f;
    public const float MAX_STAT_MULTIPLIER = 1.5f;
    public const float MIN_STAT_MULTIPLIER = 0.5f;
    public const float EFFECT_DURATION = 1f;
} 
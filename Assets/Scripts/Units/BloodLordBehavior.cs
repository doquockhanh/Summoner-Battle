using UnityEngine;

[RequireComponent(typeof(Unit))]
public class BloodLordBehavior : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitCombat combat;
    private UnitTargeting targeting;
    private UnitStatusEffects statusEffects;
    private BloodstormSkill skill;
    
    private int absorbedSouls = 0;
    private const int MAX_SOULS = 5;
    private bool isBloodstormActive = false;
    
    private float damageTimer = 0f;
    private float healingTimer = 0f;
    private float currentHealPercent;

    private HealthBarUI healthBar;
    
    private Unit currentTarget;
    private UnitMovement movement;

    public void Initialize(BloodstormSkill skill)
    {
        this.skill = skill;
        
        // Lấy các component
        unit = GetComponent<Unit>();
        stats = GetComponent<UnitStats>();
        combat = GetComponent<UnitCombat>();
        targeting = GetComponent<UnitTargeting>();
        statusEffects = GetComponent<UnitStatusEffects>();
        movement = GetComponent<UnitMovement>();
        
        // Điều chỉnh chỉ số dựa trên vị trí X
        AdjustStatsByPosition();
        
        // Disable combat cho đến khi kích hoạt Bloodstorm
        combat.enabled = false;
        
        // Đăng ký sự kiện
        UnitEvents.Status.OnUnitDeath += OnUnitDeath;

        healthBar = unit.GetComponent<UnitView>().GetHealthBar();
        healthBar.ShowSoulCounter(true);
        Debug.Log("ShowSoulCounter: " + healthBar);
    }

    private void AdjustStatsByPosition()
    {
        float normalizedX = transform.position.x / 10f; // Giả sử map rộng 10 unit
        float multiplier = Mathf.Lerp(1.5f, 0.5f, normalizedX);
        Debug.Log("AdjustStatsByPosition: " + multiplier);
        stats.ModifyDamage(multiplier);
        stats.ModifyDefense(1f / multiplier);
    }

    private void OnUnitDeath(Unit deadUnit)
    {
        if (isBloodstormActive || deadUnit == unit) return;

        // Kiểm tra xem có phải Huyết quỷ gần nhất không
        if (IsClosestBloodLordTo(deadUnit))
        {
            AbsorbSoul(deadUnit);
        }
    }

    private bool IsClosestBloodLordTo(Unit deadUnit)
    {
        float myDistance = Vector2.Distance(transform.position, deadUnit.transform.position);
        
        // Tìm tất cả Huyết quỷ khác
        var bloodLords = FindObjectsOfType<BloodLordBehavior>();
        foreach (var bloodLord in bloodLords)
        {
            if (bloodLord == this) continue;
            
            float otherDistance = Vector2.Distance(bloodLord.transform.position, deadUnit.transform.position);
            if (otherDistance < myDistance) return false;
        }
        
        return true;
    }

    private void AbsorbSoul(Unit deadUnit)
    {
        if (absorbedSouls >= MAX_SOULS) return;
        
        absorbedSouls++;
        healthBar.UpdateSoulCount(absorbedSouls);
        
        // Tạo hiệu ứng hút linh hồn
        if (skill.soulAbsorbEffectPrefab != null)
        {
            var effect = Instantiate(skill.soulAbsorbEffectPrefab, 
                deadUnit.transform.position, 
                Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // Thông báo event
        UnitEvents.Status.RaiseSoulAbsorbed(unit, deadUnit);
        
        CheckBloodstormCondition();
    }

    private void CheckBloodstormCondition()
    {
        if (!isBloodstormActive && 
            (absorbedSouls >= MAX_SOULS || stats.CurrentHealthPercent <= 0.5f))
        {
            ActivateBloodstorm();
        }
    }

    private void ActivateBloodstorm()
    {
        isBloodstormActive = true;
        
        // Enable combat và thêm status effect
        combat.enabled = true;
        statusEffects.AddEffect(new BloodstormStatusEffect(unit, skill, absorbedSouls));
        
        // Tạo hiệu ứng visual
        if (skill.bloodstormEffectPrefab != null)
        {
            var effect = Instantiate(skill.bloodstormEffectPrefab, transform);
            effect.transform.localPosition = Vector3.zero;
        }
        
        // Reset timers
        damageTimer = 0f;
        healingTimer = 0f;
        currentHealPercent = skill.healingStartPercent;

        // Thông báo event
        UnitEvents.Status.RaiseSkillActivated(unit, skill);
        
        FloatingTextManager.Instance.ShowFloatingText(
            $"Bloodstorm ({absorbedSouls} Souls)", 
            transform.position + new Vector3(0, 1, 0), 
            Color.red
        );
    }

    private void Update()
    {
        if (!isBloodstormActive) return;

        HandleDamageAndHealing();
        HandleBloodstormMovement();
    }

    private void HandleDamageAndHealing()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0)
        {
            DealBloodstormDamage();
            damageTimer = skill.damageInterval;
        }

        healingTimer -= Time.deltaTime;
        if (healingTimer <= 0)
        {
            healingTimer = 1f;
            currentHealPercent = Mathf.Max(currentHealPercent - skill.healingDecreasePercent, 0);
        }
    }

    private void DealBloodstormDamage()
    {
        float baseDamage = stats.GetModifiedDamage();
        float totalDamagePercent = skill.damageBasePercent + (skill.damagePerSoulPercent * absorbedSouls);
        float actualDamage = baseDamage * (totalDamagePercent / 100f);

        // Sử dụng targeting system để lấy các unit trong range
        Unit[] nearbyUnits = targeting.GetUnitsInRange(skill.effectRadius);
        foreach (var enemy in nearbyUnits)
        {
            if (enemy != unit && enemy.IsPlayerUnit != unit.IsPlayerUnit)
            {
                enemy.TakeDamage(actualDamage);
                float healAmount = actualDamage * (currentHealPercent / 100f);
                stats.Heal(healAmount);
            }
        }
    }

    private void HandleBloodstormMovement()
    {
        if (!isBloodstormActive) return;

        // Nếu chưa có vị trí đích hoặc đã đến gần đích
        if (movement.TargetPosition == Vector3.zero || 
            Vector2.Distance(transform.position, movement.TargetPosition) < 0.1f)
        {
            // Lấy vị trí mới
            Vector3 newPos = RandomMovementHandler.Instance.GetNextRandomPosition(
                transform.position, 
                unit.IsPlayerUnit
            );
            
            Debug.Log($"[BloodLord] Di chuyển đến vị trí mới: {newPos}");
            movement.SetTargetPosition(newPos);
        }

        // Di chuyển về phía đích
        Vector3 direction = (movement.TargetPosition - transform.position).normalized;
        movement.SetMoveDirection(direction);
    }

    private void FindFurthestTarget()
    {
        float maxDistance = 0f;
        Unit furthestTarget = null;
        
        // Tìm tất cả unit đối phương trong scene
        var allUnits = FindObjectsOfType<Unit>();
        foreach (var potentialTarget in allUnits)
        {
            if (potentialTarget == unit || 
                potentialTarget.IsDead || 
                potentialTarget.IsPlayerUnit == unit.IsPlayerUnit) 
                continue;

            float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestTarget = potentialTarget;
            }
        }

        currentTarget = furthestTarget;
        Debug.Log($"[BloodLord] Tìm thấy mục tiêu mới: {(currentTarget ? currentTarget.name : "Không có")} " +
                 $"ở khoảng cách: {maxDistance:F1}");
    }

    private void OnDestroy()
    {
        UnitEvents.Status.OnUnitDeath -= OnUnitDeath;
    }
} 
using UnityEngine;
using System.Collections.Generic;

public class UnitTargeting : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    
    private Unit currentTarget;
    private Base currentBaseTarget;
    
    private readonly Collider2D[] detectedColliders = new Collider2D[20]; // Pool cố định
    private int detectedCount;

    public Unit CurrentTarget => currentTarget;
    public Base CurrentBaseTarget => currentBaseTarget;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
    }

    public void UpdateTarget()
    {
        // Kiểm tra xem unit có đang trong trạng thái Bloodstorm không
        var bloodstormEffect = unit.GetComponent<UnitStatusEffects>()
            ?.GetEffect(StatusEffectType.Bloodstorm) as BloodstormStatusEffect;
            
        if (bloodstormEffect != null)
        {
            // Nếu đang trong Bloodstorm, tìm mục tiêu xa nhất
            currentTarget = FindFurthestTarget();
        }
        else
        {
            // Kiểm tra target hiện tại
            if (IsValidTarget(currentTarget))
            {
                return;
            }

            if (IsValidBaseTarget(currentBaseTarget))
            {
                currentTarget = null;
                return;
            }

            // Tìm target mới
            FindNewTarget();
        }
    }

    public bool IsInRange(Unit target)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.transform.position) <= stats.Data.range;
    }

    public bool IsInRangeOfBase()
    {
        if (currentBaseTarget == null) return false;
        
        Vector2 closestPoint = currentBaseTarget.GetComponent<Collider2D>()
            .ClosestPoint(transform.position);
        return Vector2.Distance(transform.position, closestPoint) <= stats.Data.range;
    }

    private void FindNewTarget()
    {
        detectedCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            stats.Data.detectRange,
            detectedColliders
        );

        Unit bestTarget = null;
        float closestDistance = float.MaxValue;
        Base nearestBase = null;

        for (int i = 0; i < detectedCount; i++)
        {
            // Kiểm tra Unit
            Unit potentialTarget = detectedColliders[i].GetComponent<Unit>();
            if (IsValidTarget(potentialTarget))
            {
                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = potentialTarget;
                }
                continue;
            }

            // Kiểm tra Base
            Base potentialBase = detectedColliders[i].GetComponent<Base>();
            if (IsValidBaseTarget(potentialBase))
            {
                nearestBase = potentialBase;
            }
        }

        currentTarget = bestTarget;
        currentBaseTarget = bestTarget == null ? nearestBase : null;
    }

    private bool IsValidTarget(Unit target)
    {
        return target != null && 
               !target.IsDead && 
               target.IsPlayerUnit != unit.IsPlayerUnit;
    }

    private bool IsValidBaseTarget(Base baseTarget)
    {
        return baseTarget != null && 
               baseTarget.IsPlayerBase != unit.IsPlayerUnit;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentTarget != null) return;

        Base enemyBase = other.GetComponent<Base>();
        if (IsValidBaseTarget(enemyBase))
        {
            currentBaseTarget = enemyBase;
            currentTarget = null;
            return;
        }

        Unit otherUnit = other.GetComponent<Unit>();
        if (IsValidTarget(otherUnit))
        {
            currentTarget = otherUnit;
            currentBaseTarget = null;
        }
    }

    public Unit FindFurthestTarget()
    {
        float maxDistance = 0f;
        Unit furthestTarget = null;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.Data.detectRange);
        foreach (var hit in hits)
        {
            Unit potentialTarget = hit.GetComponent<Unit>();
            if (!IsValidTarget(potentialTarget)) continue;
            
            float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestTarget = potentialTarget;
            }
        }
        
        return furthestTarget;
    }

    public Unit[] GetUnitsInRange(float range)
    {
        List<Unit> units = new List<Unit>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        
        foreach (var hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null && unit != this.unit)
            {
                units.Add(unit);
            }
        }
        
        return units.ToArray();
    }
} 
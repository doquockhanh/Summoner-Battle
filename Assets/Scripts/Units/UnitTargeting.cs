using UnityEngine;
using System.Collections.Generic;

public class UnitTargeting : MonoBehaviour
{
    // Constants
    private const int MAX_COLLIDERS = 20;

    // Dependencies
    private Unit unit;
    private UnitStats stats;
    private BloodstormStatusEffect bloodstormEffect;

    // Target state
    private Unit currentTarget;
    private Base currentBaseTarget;
    private readonly Collider2D[] detectedColliders = new Collider2D[MAX_COLLIDERS];
    private int detectedCount;

    private bool isPaused = false;
    private float targetCheckTimer;

    // Properties với XML documentation
    /// <summary>Unit hiện đang được nhắm tới</summary>
    public Unit CurrentTarget => currentTarget;
    /// <summary>Base hiện đang được nhắm tới</summary>
    public Base CurrentBaseTarget => currentBaseTarget;

    public bool IsPaused
    {
        get => isPaused;
        private set
        {
            if (isPaused != value)
            {
                isPaused = value;
            }
        }
    }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
        bloodstormEffect = unit.GetComponent<UnitStatusEffects>()
            ?.GetEffect(StatusEffectType.Bloodstorm) as BloodstormStatusEffect;
    }

    public void UpdateTarget()
    {
        if (IsPaused) return;

        // Kiểm tra target hiện tại
        if (!IsValidTarget(currentTarget) && !IsValidBaseTarget(currentBaseTarget))
        {
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
        ScanForTargets();
        (Unit bestTarget, Base nearestBase) = FindBestTargetsFromDetected();

        if (bestTarget != null)
        {
            AssignTarget(bestTarget);
        }
        else
        {
            currentTarget = null;
            currentBaseTarget = nearestBase;
        }
    }

    private void ScanForTargets()
    {
        detectedCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            stats.Data.detectRange,
            detectedColliders
        );
    }

    private (Unit unit, Base baseTarget) FindBestTargetsFromDetected()
    {
        Unit bestTarget = null;
        float closestDistance = float.MaxValue;
        Base nearestBase = null;

        for (int i = 0; i < detectedCount; i++)
        {
            var collider = detectedColliders[i];

            Unit potentialTarget = TryGetValidUnit(collider);
            if (potentialTarget != null)
            {
                float distance = GetDistanceTo(potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = potentialTarget;
                }
                continue;
            }

            Base potentialBase = TryGetValidBase(collider);
            if (potentialBase != null)
            {
                nearestBase = potentialBase;
            }
        }

        return (bestTarget, nearestBase);
    }

    private Unit TryGetValidUnit(Collider2D collider)
    {
        Unit potentialTarget = collider.GetComponent<Unit>();
        return IsValidTarget(potentialTarget) ? potentialTarget : null;
    }

    private Base TryGetValidBase(Collider2D collider)
    {
        Base potentialBase = collider.GetComponent<Base>();
        return IsValidBaseTarget(potentialBase) ? potentialBase : null;
    }

    private float GetDistanceTo(Vector2 position)
    {
        return Vector2.Distance(transform.position, position);
    }

    public Unit FindNearestTarget() => FindTargetByDistance((d1, d2) => d1 < d2);

    private Unit FindTargetByDistance(System.Func<float, float, bool> compareDistance)
    {
        float bestDistance = compareDistance(0, float.MaxValue) ? 0 : float.MaxValue;
        Unit bestTarget = null;

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, stats.Data.detectRange))
        {
            Unit potentialTarget = hit.GetComponent<Unit>();
            if (!IsValidTarget(potentialTarget)) continue;

            float distance = GetDistanceTo(potentialTarget.transform.position);
            if (compareDistance(distance, bestDistance))
            {
                bestDistance = distance;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
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
            currentTarget = null;
            currentBaseTarget = enemyBase;
            return;
        }

        Unit otherUnit = other.GetComponent<Unit>();
        if (IsValidTarget(otherUnit))
        {
            AssignTarget(otherUnit);
        }
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

    public void AssignTarget(Unit newTarget)
    {
        currentTarget = newTarget;
        if (newTarget != null)
        {
            currentBaseTarget = null;
        }
    }

    public void SetTarget(Unit target)
    {
        if (target != null && !target.IsDead)
        {
            currentTarget = target;
            currentBaseTarget = null;
        }
    }

    // Thêm các phương thức điều khiển targeting
    public void PauseTargeting()
    {
        IsPaused = true;
    }

    public void ResumeTargeting()
    {
        IsPaused = false;
    }

    public void ForceTargetUpdate()
    {
        targetCheckTimer = 0;
        UpdateTarget();
    }
}
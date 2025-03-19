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

    public Unit CurrentTarget => currentTarget;
    public Base CurrentBaseTarget => currentBaseTarget;

    public bool IsPaused
    {
        get => isPaused;
        private set => isPaused = value;
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

        if (!IsValidTarget(currentTarget) && !IsValidBaseTarget(currentBaseTarget))
        {
            FindNewTarget();
        }
    }

    public bool IsInRange(Unit target)
    {
        if (target == null) return false;
        
        var targetCell = HexGrid.Instance.GetCellAtPosition(target.transform.position);
        var myCell = HexGrid.Instance.GetCellAtPosition(transform.position);
        
        if (targetCell == null || myCell == null) return false;
        
        int distance = targetCell.Coordinates.DistanceTo(myCell.Coordinates);
        return distance <= Mathf.CeilToInt(stats.Data.range / HexMetrics.outerRadius);
    }

    public bool IsInRangeOfBase()
    {
        if (currentBaseTarget == null) return false;

        var myCell = HexGrid.Instance.GetCellAtPosition(transform.position);
        var baseCell = HexGrid.Instance.GetCellAtPosition(currentBaseTarget.transform.position);
        
        if (baseCell == null || myCell == null) return false;

        int distance = baseCell.Coordinates.DistanceTo(myCell.Coordinates);
        return distance <= Mathf.CeilToInt(stats.Data.range / HexMetrics.outerRadius);
    }

    private void FindNewTarget()
    {
        var myCell = HexGrid.Instance.GetCellAtPosition(transform.position);
        if (myCell == null) return;

        int searchRange = Mathf.CeilToInt(stats.Data.detectRange / HexMetrics.outerRadius);
        var cellsInRange = HexGrid.Instance.GetCellsInRange(myCell.Coordinates, searchRange);

        Unit bestTarget = null;
        float closestDistance = float.MaxValue;
        Base nearestBase = null;

        foreach (var cell in cellsInRange)
        {
            if (cell.OccupyingUnit != null && IsValidTarget(cell.OccupyingUnit))
            {
                float distance = Vector3.Distance(transform.position, cell.OccupyingUnit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = cell.OccupyingUnit;
                }
            }
        }

        if (bestTarget != null)
        {
            AssignTarget(bestTarget);
        }
        else
        {
            currentTarget = null;
            currentBaseTarget = FindNearestBase();
        }
    }

    private Base FindNearestBase()
    {
        // Logic tìm base gần nhất (giữ nguyên logic cũ vì base không nằm trong hex grid)
        Base[] bases = GameObject.FindObjectsOfType<Base>();
        Base nearestBase = null;
        float minDistance = float.MaxValue;

        foreach (Base b in bases)
        {
            if (b.IsPlayerBase != unit.IsPlayerUnit)
            {
                float distance = Vector3.Distance(transform.position, b.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestBase = b;
                }
            }
        }

        return nearestBase;
    }

    public bool IsValidTarget(Unit target)
    {
        return target != null && !target.IsDead && target.IsPlayerUnit != unit.IsPlayerUnit;
    }

    private bool IsValidBaseTarget(Base baseTarget)
    {
        return baseTarget != null && baseTarget.IsPlayerBase != unit.IsPlayerUnit;
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
        if (IsValidTarget(target))
        {
            currentTarget = target;
            currentBaseTarget = null;
        }
    }

    // Thêm các phương thức điều khiển targeting
    public void PauseTargeting() => IsPaused = true;
    public void ResumeTargeting() => IsPaused = false;

    public void ForceTargetUpdate()
    {
        targetCheckTimer = 0;
        UpdateTarget();
    }
}
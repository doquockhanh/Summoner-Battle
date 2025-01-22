using UnityEngine;

public class UnitTargeting : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;

    public void Initialize(Unit unit, UnitStats stats)
    {
        this.unit = unit;
        this.stats = stats;
    }

    public (Unit unit, Base baseTarget) FindTarget()
    {
        // Ưu tiên target hiện tại nếu vẫn hợp lệ
        if (unit.CurrentTarget != null && IsValidTarget(unit.CurrentTarget))
        {
            return (unit.CurrentTarget, null);
        }

        if (unit.CurrentBaseTarget != null && unit.CurrentBaseTarget.IsPlayerBase != unit.IsPlayerUnit)
        {
            return (null, unit.CurrentBaseTarget);
        }

        // Logic tìm target mới
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, stats.Data.detectRange);
        
        // Ưu tiên tìm unit trước
        Unit bestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            Unit potentialTarget = collider.GetComponent<Unit>();
            if (IsValidTarget(potentialTarget))
            {
                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = potentialTarget;
                }
            }
        }

        if (bestTarget != null)
        {
            return (bestTarget, null);
        }

        // Nếu không tìm thấy unit, tìm base
        foreach (Collider2D collider in colliders)
        {
            Base baseTarget = collider.GetComponent<Base>();
            if (baseTarget != null && baseTarget.IsPlayerBase != unit.IsPlayerUnit)
            {
                return (null, baseTarget);
            }
        }

        return (null, null);
    }

    private bool IsValidTarget(Unit target)
    {
        if (target == null || target == unit || target.IsDead)
            return false;
            
        return target.IsPlayerUnit != unit.IsPlayerUnit;
    }

    public bool IsInRange(Unit target)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.transform.position) <= stats.Data.range;
    }

    public bool IsInRange(Base target)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.transform.position) <= stats.Data.range;
    }
} 
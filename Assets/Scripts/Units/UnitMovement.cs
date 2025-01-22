using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitTargeting targeting;
    private const float MIN_DISTANCE_BETWEEN_UNITS = 0.5f;

    public void Initialize(Unit unit, UnitStats stats)
    {
        this.unit = unit;
        this.stats = stats;
        this.targeting = unit.GetComponent<UnitTargeting>();
    }

    public Vector3 CalculateMovement()
    {
        if ((unit.CurrentTarget != null && targeting.IsInRange(unit.CurrentTarget)) ||
            (unit.CurrentBaseTarget != null && targeting.IsInRange(unit.CurrentBaseTarget)))
        {
            return Vector3.zero;
        }

        Vector3 direction = CalculateDirection();
        Vector3 separationForce = CalculateSeparation();
        
        return (direction + separationForce).normalized * stats.GetModifiedSpeed();
    }

    private Vector3 CalculateDirection()
    {
        Unit targetUnit = unit.CurrentTarget;
        Base targetBase = unit.CurrentBaseTarget;

        if (targetUnit != null)
        {
            return (targetUnit.transform.position - transform.position).normalized;
        }
        
        if (targetBase != null)
        {
            Vector3 direction = (targetBase.transform.position - transform.position).normalized;
            return new Vector3(Mathf.Sign(direction.x), 0, 0);
        }

        return unit.IsPlayerUnit ? Vector3.right : Vector3.left;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separationForce = Vector3.zero;
        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(transform.position, MIN_DISTANCE_BETWEEN_UNITS);
        
        foreach (Collider2D collider in nearbyUnits)
        {
            Unit otherUnit = collider.GetComponent<Unit>();
            if (otherUnit != null && otherUnit != unit)
            {
                Vector3 awayFromOther = transform.position - otherUnit.transform.position;
                float distance = awayFromOther.magnitude;
                
                if (distance < MIN_DISTANCE_BETWEEN_UNITS)
                {
                    separationForce += awayFromOther.normalized * (1 - distance/MIN_DISTANCE_BETWEEN_UNITS);
                }
            }
        }
        
        return separationForce;
    }
} 
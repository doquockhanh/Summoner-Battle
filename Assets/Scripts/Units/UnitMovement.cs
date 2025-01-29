using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    
    private const float MIN_DISTANCE_BETWEEN_UNITS = 0.5f;
    private const float SEPARATION_FORCE = 1.5f;
    private LayerMask unitLayer;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
        unitLayer = LayerMask.GetMask("Units");
    }

    public void Move(Unit targetUnit, Base targetBase)
    {
        if (unit.IsDead) return;

        if (IsInAttackRange(targetUnit, targetBase)) return;

        Vector3 direction = CalculateDesiredDirection(targetUnit, targetBase);
        Vector3 separation = CalculateSeparation();
        
        Vector3 finalDirection = (direction + separation).normalized;
        transform.position += finalDirection * stats.Data.moveSpeed * Time.deltaTime;
    }

    private Vector3 CalculateDesiredDirection(Unit targetUnit, Base targetBase)
    {
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
        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(
            transform.position, 
            MIN_DISTANCE_BETWEEN_UNITS,
            unitLayer
        );
        
        foreach (Collider2D collider in nearbyUnits)
        {
            if (collider.gameObject == gameObject) continue;

            Vector3 awayFromOther = transform.position - collider.transform.position;
            float distance = awayFromOther.magnitude;
            
            if (distance < MIN_DISTANCE_BETWEEN_UNITS)
            {
                float strength = 1 - (distance / MIN_DISTANCE_BETWEEN_UNITS);
                separationForce += awayFromOther.normalized * strength * SEPARATION_FORCE;
            }
        }

        return separationForce;
    }

    private bool IsInAttackRange(Unit targetUnit, Base targetBase)
    {
        if (targetUnit != null)
        {
            return Vector2.Distance(transform.position, targetUnit.transform.position) <= stats.Data.range;
        }
        
        if (targetBase != null)
        {
            Vector2 closestPoint = targetBase.GetComponent<Collider2D>().ClosestPoint(transform.position);
            return Vector2.Distance(transform.position, closestPoint) <= stats.Data.range;
        }

        return false;
    }
} 
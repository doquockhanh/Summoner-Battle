using UnityEngine;
using System.Collections;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private Vector3 originalPosition;
    private bool isKnockedUp;
    private Coroutine knockupCoroutine;
    private UnitStatusEffects statusEffects;
    
    private const float MIN_DISTANCE_BETWEEN_UNITS = 0.5f;
    private const float SEPARATION_FORCE = 1.5f;
    private LayerMask unitLayer;

    [SerializeField] private float knockupHeight = 0.5f;

    private float moveSpeed;
    private Vector3 moveDirection;
    private Vector3 targetPosition;

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public Vector3 TargetPosition => targetPosition;

    private void OnDisable()
    {
        if (knockupCoroutine != null)
        {
            StopCoroutine(knockupCoroutine);
            knockupCoroutine = null;
        }
        
        if (isKnockedUp)
        {
            transform.position = originalPosition;
            isKnockedUp = false;
        }
    }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
        this.statusEffects = unit.GetComponent<UnitStatusEffects>();
        originalPosition = transform.position;
        unitLayer = LayerMask.GetMask("Units");
        moveSpeed = unit.GetUnitStats().Data.moveSpeed;
        targetPosition = Vector3.zero;
    }

    public void Move(Unit targetUnit, Base targetBase)
    {
        if (unit.IsDead || statusEffects.IsKnockedUp) return;

        if (IsInAttackRange(targetUnit, targetBase))
        {
            unit.GetComponent<UnitView>().SetMoving(false);
            return;
        }

        Vector3 direction = CalculateDesiredDirection(targetUnit, targetBase);
        Vector3 separation = CalculateSeparation();
        
        Vector3 finalDirection = (direction + separation).normalized;
        transform.position += finalDirection * stats.Data.moveSpeed * Time.deltaTime;
        
        var view = unit.GetComponent<UnitView>();
        view.SetMoving(true);
        view.FlipSprite(finalDirection.x > 0);
        
        originalPosition = new Vector3(transform.position.x, originalPosition.y, transform.position.z);
    }

    public void Knockup(float duration)
    {
        if (unit.IsDead) return;

        if (knockupCoroutine != null)
        {
            StopCoroutine(knockupCoroutine);
        }
        
        originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        knockupCoroutine = StartCoroutine(KnockupCoroutine(duration));
    }

    private IEnumerator KnockupCoroutine(float duration)
    {
        isKnockedUp = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration && !unit.IsDead)
        {
            float heightPercent = Mathf.Sin((elapsedTime / duration) * Mathf.PI);
            float currentHeight = knockupHeight * heightPercent;
            
            transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y + currentHeight,
                originalPosition.z
            );
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
        isKnockedUp = false;
        knockupCoroutine = null;
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

    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }
} 
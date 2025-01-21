using UnityEngine;

public class Unit : MonoBehaviour
{
    private UnitData data;
    private float currentHp;
    public bool isPlayerUnit;
    private float attackTimer;
    private Unit currentTarget;
    private UnitView unitView;
    private Base targetBase;
    
    private const float MIN_DISTANCE_BETWEEN_UNITS = 1f; // Khoảng cách tối thiểu giữa các unit
    
    public bool IsDead => currentHp <= 0;
    
    public void Initialize(UnitData unitData, bool isPlayer)
    {
        data = unitData;
        currentHp = data.hp;
        isPlayerUnit = isPlayer;
        attackTimer = 0;
        unitView = GetComponent<UnitView>();
        
        // Điều chỉnh collider radius theo range
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.radius = data.range;
        }
        
        Debug.Log($"Initializing unit with HP: {currentHp}");
        if (unitView != null)
        {
            unitView.Setup(data, this);
            unitView.UpdateHealth(currentHp);
        }
        
        // Xoay unit nếu là enemy
        if (!isPlayer)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    
    private void Update()
    {
        if (IsDead) return;
        
        if (targetBase != null)
        {
            // Nếu đang tấn công base thì dừng lại và tấn công
            if (attackTimer <= 0)
            {
                AttackBase(targetBase);
                attackTimer = 1f / data.attackSpeed;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
            return;
        }

        // Reset target nếu target đã chết
        if (currentTarget != null && currentTarget.IsDead)
        {
            currentTarget = null;
        }

        // Luôn tìm target mới nếu trong tầm phát hiện
        FindTarget();

        if (currentTarget != null)
        {
            if (IsInRange(currentTarget))
            {
                TryAttack();
            }
            else
            {
                Move();
            }
        }
        else
        {
            Move();
        }
    }
    
    private void Move()
    {
        Vector3 direction;
        
        if (currentTarget != null)
        {
            // Di chuyển tự do về phía target
            direction = (currentTarget.transform.position - transform.position).normalized;
        }
        else
        {
            // Tìm vị trí base đối phương và di chuyển về phía đó
            Base[] bases = FindObjectsOfType<Base>();
            foreach (Base baseUnit in bases)
            {
                if (baseUnit.IsPlayerBase != isPlayerUnit)
                {
                    direction = (baseUnit.transform.position - transform.position).normalized;
                    // Khi không có target, vẫn giữ di chuyển ngang để tiếp cận base
                    direction = new Vector3(Mathf.Sign(direction.x), 0, 0);
                    break;
                }
            }
            // Fallback nếu không tìm thấy base
            direction = isPlayerUnit ? Vector3.right : Vector3.left;
        }
        
        // Tính toán lực đẩy từ các unit lân cận
        Vector3 separationForce = Vector3.zero;
        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(transform.position, MIN_DISTANCE_BETWEEN_UNITS);
        
        foreach (Collider2D collider in nearbyUnits)
        {
            Unit otherUnit = collider.GetComponent<Unit>();
            if (otherUnit != null && otherUnit != this)
            {
                Vector3 awayFromOther = transform.position - otherUnit.transform.position;
                float distance = awayFromOther.magnitude;
                
                if (distance < MIN_DISTANCE_BETWEEN_UNITS)
                {
                    // Lực đẩy tăng khi càng gần nhau
                    float strength = (MIN_DISTANCE_BETWEEN_UNITS - distance) / MIN_DISTANCE_BETWEEN_UNITS;
                    separationForce += awayFromOther.normalized * strength;
                }
            }
        }
        
        // Kết hợp hướng di chuyển chính với lực đẩy
        Vector3 finalDirection = (direction + separationForce * 0.5f).normalized;
        transform.position += finalDirection * data.moveSpeed * Time.deltaTime;
        
        // Xoay sprite theo hướng di chuyển
        if (finalDirection.x != 0)
        {
            transform.localScale = new Vector3(
                finalDirection.x > 0 ? 1 : -1,
                1,
                1
            );
        }
    }
    
    private void FindTarget()
    {
        // Tìm unit gần nhất của đối phương trong tầm phát hiện
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, data.detectRange);
        float closestScore = float.MaxValue;
        Base allyBase = null;
        
        // Tìm nhà chính đồng minh
        Base[] bases = FindObjectsOfType<Base>();
        foreach (Base baseUnit in bases)
        {
            if (baseUnit.IsPlayerBase == isPlayerUnit)
            {
                allyBase = baseUnit;
                break;
            }
        }
        
        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.isPlayerUnit != isPlayerUnit && !unit.IsDead)
            {
                float distanceToMe = Vector2.Distance(transform.position, unit.transform.position);
                
                // Tính điểm ưu tiên dựa trên khoảng cách đến unit và nhà chính
                float score;
                if (allyBase != null)
                {
                    float distanceToBase = Vector2.Distance(unit.transform.position, allyBase.transform.position);
                    // Unit càng gần base và càng gần mình càng được ưu tiên
                    score = distanceToBase * 0.7f + distanceToMe * 0.3f;
                }
                else
                {
                    score = distanceToMe;
                }
                
                if (score < closestScore)
                {
                    closestScore = score;
                    currentTarget = unit;
                }
            }
        }
    }
    
    private void TryAttack()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        Attack(currentTarget);
        attackTimer = 1f / data.attackSpeed;
    }
    
    private bool IsInRange(Unit target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= data.range;
    }
    
    private void Attack(Unit target)
    {
        target.TakeDamage(data.damage);
        unitView.PlayAttackEffect();
    }
    
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"Unit took {damage} damage, current HP: {currentHp}");
        if (unitView != null)
        {
            unitView.UpdateHealth(currentHp);
        }
        
        if (IsDead)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Có thể thêm animation chết ở đây
        Destroy(gameObject, 1f);
    }
    
    // Để debug trong Unity Editor
    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            // Vẽ tầm đánh
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.range);
            
            // Vẽ tầm phát hiện
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.detectRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Base enemyBase = other.GetComponent<Base>();
        if (enemyBase != null && enemyBase.IsPlayerBase != isPlayerUnit)
        {
            targetBase = enemyBase;
            return;
        }

        if (currentTarget != null) return;
        
        Unit otherUnit = other.GetComponent<Unit>();
        if (otherUnit != null && !otherUnit.IsDead && otherUnit.isPlayerUnit != isPlayerUnit)
        {
            currentTarget = otherUnit;
        }
    }

    private void AttackBase(Base enemyBase)
    {
        enemyBase.TakeDamage(data.damage);
        unitView.PlayAttackEffect();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Thêm phát hiện trong trường hợp units đi qua nhau
        if (currentTarget == null)
        {
            Unit otherUnit = other.GetComponent<Unit>();
            if (otherUnit != null && !otherUnit.IsDead && otherUnit.isPlayerUnit != isPlayerUnit)
            {
                currentTarget = otherUnit;
            }
        }
    }
}

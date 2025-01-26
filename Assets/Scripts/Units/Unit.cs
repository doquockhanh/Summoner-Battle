using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    private UnitStats stats;
    private UnitCombat combat;
    private UnitMovement movement;
    private UnitTargeting targeting;
    private UnitView view;

    private Unit currentTarget;
    private Base currentBaseTarget;
    private bool isPlayerUnit;
    private float hpLossTimer;
    private CardController ownerCard;

    public bool IsDead => stats.IsDead;
    public bool IsPlayerUnit => isPlayerUnit;
    public Unit CurrentTarget => currentTarget;
    public Base CurrentBaseTarget => currentBaseTarget;
    public UnitData GetUnitData() => stats.Data;
    public UnitStats GetUnitStats() => stats;
    public float GetCurrentHP() => stats.CurrentHP;

    // Thêm delegates để xử lý sự kiện
    public delegate float DamageTakenHandler(float damage);
    public delegate void DamageDealtHandler(float damage, Unit target);
    
    public event DamageTakenHandler OnDamageTaken;
    public event DamageDealtHandler OnDamageDealt;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        combat = GetComponent<UnitCombat>();
        movement = GetComponent<UnitMovement>();
        targeting = GetComponent<UnitTargeting>();
        view = GetComponent<UnitView>();
    }

    public void Initialize(UnitData data, bool isPlayer, CardController card)
    {
        ownerCard = card;
        isPlayerUnit = isPlayer;
        stats.Initialize(data);
        combat.Initialize(this, stats, view);
        movement.Initialize(this, stats);
        targeting.Initialize(this, stats);
        view.Initialize(this, stats);
        
        hpLossTimer = 0;
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateTarget();
        HandleCombat();
        HandleMovement();
        HandleHpLoss();
    }

    private void UpdateTarget()
    {
        (Unit unit, Base baseTarget) = targeting.FindTarget();
        currentTarget = unit;
        currentBaseTarget = baseTarget;
    }

    private void HandleCombat()
    {
        if (currentTarget != null && targeting.IsInRange(currentTarget.transform.position))
        {
            combat.TryAttack(currentTarget);
        }
        else if (currentBaseTarget != null)
        {
            Vector2 closestPoint = currentBaseTarget.GetComponent<Collider2D>().ClosestPoint(transform.position);
            float distanceToBase = Vector2.Distance(transform.position, closestPoint);
            
            if (distanceToBase <= stats.Data.range)
            {
                combat.AttackBase(currentBaseTarget);
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = this.movement.CalculateMovement();
        transform.position += movement * Time.deltaTime;
    }

    private void HandleHpLoss()
    {
        hpLossTimer += Time.deltaTime;
        if (hpLossTimer >= 1f)
        {
            stats.TakeDamage(stats.Data.hpLossPerSecond);
            hpLossTimer = 0;
        }
    }

    public void TakeDamage(float damage)
    {
        // Cho phép các effect xử lý sát thương trước
        if (OnDamageTaken != null)
        {
            damage = OnDamageTaken(damage);
        }
        
        stats.TakeDamage(damage);
        
        // Thông báo cho Card về sát thương
        if (ownerCard != null)
        {
            ownerCard.GainManaFromDamage(damage, stats.Data.hp, true);
        }
        
        if (IsDead)
        {
            UnitPoolManager.Instance.ReturnToPool(this);
        }
    }

    public void DealDamage(float damage, Unit target)
    {
        target.TakeDamage(damage);
        
        // Thông báo cho các effect
        if (OnDamageDealt != null)
        {
            OnDamageDealt(damage, target);
        }
        
        // Thông báo cho Card về sát thương gây ra
        if (ownerCard != null)
        {
            ownerCard.GainManaFromDamage(damage, target.GetUnitData().hp, false);
        }
    }

    public void ApplyBuff(float damageModifier, float speedModifier, float defenseModifier, float duration)
    {
        stats.ModifyDamage(damageModifier);
        stats.ModifySpeed(speedModifier);
        stats.ModifyDefense(defenseModifier);
        
        // Reset buff sau duration
        Invoke(nameof(RemoveBuff), duration);
    }

    private void RemoveBuff(float damageModifier, float speedModifier, float defenseModifier)
    {
        stats.ModifyDamage(-damageModifier);
        stats.ModifySpeed(-speedModifier);
        stats.ModifyDefense(-defenseModifier);
    }

    private void OnDrawGizmosSelected()
    {
        if (stats.Data != null)
        {
            // Vẽ tầm đánh
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.Data.range);
            
            // Vẽ tầm phát hiện
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.Data.detectRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Base enemyBase = other.GetComponent<Base>();
        if (enemyBase != null && enemyBase.IsPlayerBase != isPlayerUnit)
        {
            currentBaseTarget = enemyBase;
            currentTarget = null; // Reset current target khi chuyển sang tấn công base
            Debug.Log($"Found enemy base: {enemyBase.name}");
            return;
        }
        
        if (currentTarget != null) return;
        
        Unit otherUnit = other.GetComponent<Unit>();
        if (otherUnit != null && !otherUnit.IsDead && otherUnit.isPlayerUnit != isPlayerUnit)
        {
            currentTarget = otherUnit;
        }
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

    public void ModifyDamage(float amount)
    {
        stats.ModifyDamage(amount);
    }
    
    public void ModifySpeed(float amount)
    {
        stats.ModifySpeed(amount);
    }
    
    public void ModifyDefense(float amount)
    {
        stats.ModifyDefense(amount);
    }
    
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitStats stats;
    [SerializeField] private UnitCombat combat;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitTargeting targeting;
    [SerializeField] private UnitView view;

    private Unit currentTarget;
    private Base currentBaseTarget;
    private bool isPlayerUnit;
    private float hpLossTimer;
    private CardController ownerCard;

    // Chuyển các property vào interface IUnit để dễ mở rộng
    public interface IUnit 
    {
        bool IsDead { get; }
        bool IsPlayerUnit { get; }
        Unit CurrentTarget { get; }
        Base CurrentBaseTarget { get; } 
        UnitData UnitData { get; }
        UnitStats Stats { get; }
        float CurrentHP { get; }
        CardController OwnerCard { get; }
    }

    // Events được tổ chức lại
    public event System.Action<float> OnDamageTaken;
    public event System.Action<float, Unit> OnDamageDealt;
    public event System.Action<float> OnShieldChanged;
    public event System.Action OnDeath;

    public bool IsDead => stats.IsDead;
    public bool IsPlayerUnit => isPlayerUnit;
    public Unit CurrentTarget => currentTarget;
    public Base CurrentBaseTarget => currentBaseTarget;
    public UnitData GetUnitData() => stats.Data;
    public UnitStats GetUnitStats() => stats;
    public float GetCurrentHP() => stats.CurrentHP;
    public CardController OwnerCard => ownerCard;

    private void Awake()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        stats = stats ?? GetComponent<UnitStats>();
        combat = combat ?? GetComponent<UnitCombat>();
        movement = movement ?? GetComponent<UnitMovement>();
        targeting = targeting ?? GetComponent<UnitTargeting>();
        view = view ?? GetComponent<UnitView>();

        if (stats == null || combat == null || movement == null || 
            targeting == null || view == null)
        {
            Debug.LogError($"Missing required components on {gameObject.name}");
        }
    }

    public void Initialize(UnitData data, bool isPlayer, CardController cardController)
    {
        ownerCard = cardController;
        isPlayerUnit = isPlayer;
        
        stats.Initialize(data);
        combat.Initialize(this);
        movement.Initialize(this);
        targeting.Initialize(this);
        view.Initialize(this);
        
        hpLossTimer = 0;
    }

    private void Update()
    {
        if (stats.IsDead) return;

        targeting.UpdateTarget();
        HandleCombat();
        HandleMovement();
        HandleHpLoss();
    }

    private void HandleCombat()
    {
        var target = targeting.CurrentTarget;
        if (target != null && targeting.IsInRange(target))
        {
            combat.TryAttack(target);
        }
        else if (targeting.CurrentBaseTarget != null && targeting.IsInRangeOfBase())
        {
            combat.AttackBase(targeting.CurrentBaseTarget);
        }
    }

    private void HandleMovement()
    {
        movement.Move(targeting.CurrentTarget, targeting.CurrentBaseTarget);
    }

    private void HandleHpLoss()
    {
        if (stats.Data.hpLossPerSecond <= 0) return;
        
        hpLossTimer += Time.deltaTime;
        if (hpLossTimer >= 1f)
        {
            stats.TakeDamage(stats.Data.hpLossPerSecond);
            hpLossTimer = 0;
        }
    }

    public void TakeDamage(float damage)
    {
        
        stats.TakeDamage(damage);
        
        if (ownerCard != null)
        {
            ownerCard.GainManaFromDamage(damage, stats.MaxHp, true);
        }
        
        if (IsDead)
        {
            UnitPoolManager.Instance.ReturnToPool(this);
        }
    }

    public void DealDamage(float damage, Unit target)
    {
        target.TakeDamage(damage);
        stats.ProcessLifesteal(damage);
        
        if (ownerCard != null)
        {
            ownerCard.GainManaFromDamage(damage, target.GetUnitStats().MaxHp, false);
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

    public void Heal(float amount)
    {
        stats.TakeDamage(-amount); // Sử dụng số âm để hồi máu
    }
}

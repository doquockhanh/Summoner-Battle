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

    public virtual void Initialize(UnitData data, bool isPlayer, CardController cardController)
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

        if (!targeting.IsPaused)
        {
            targeting.UpdateTarget();
            HandleCombat();
            HandleMovement();
        }
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

    public void TakeDamage(float amount, DamageType damageType, Unit source = null)
    {
        if (stats != null)
        {
            stats.TakeDamage(amount, damageType, source);
        }

        if (IsDead)
        {
            UnitPoolManager.Instance.ReturnToPool(this);
        }
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
}

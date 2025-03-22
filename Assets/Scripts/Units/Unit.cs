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

    private Base currentBaseTarget;
    private bool isPlayerUnit;
    private CardController ownerCard;

    public event System.Action OnDeath;

    public bool IsDead => stats.IsDead;
    public bool IsPlayerUnit => isPlayerUnit;
    public Base CurrentBaseTarget => currentBaseTarget;
    public UnitData GetUnitData() => stats.Data;
    public UnitStats GetUnitStats() => stats;
    public float GetCurrentHP() => stats.CurrentHP;
    public CardController OwnerCard => ownerCard;
    public HexCell OccupiedCell => movement.OccupiedCell;
    public UnitTargeting Targeting => targeting;
    public Unit CurrentTarget => targeting.CurrentTarget;

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
        // movement no need Initialize
        targeting.Initialize(this);
        view.Initialize(this);
    }

    void Start()
    {
        targeting.autoTargeting = true;
    }

    private void Update()
    {
        if (IsDead) return;

        if (targeting.CurrentTarget != null)
        {
            combat.TryAttack(targeting.CurrentTarget);
            HandleMovement();
        }
    }


    private void HandleMovement()
    {
        movement.Move(targeting.CurrentTarget.OccupiedCell);
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
}

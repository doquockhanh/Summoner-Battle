using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitView view;
    private float attackTimer;
    private UnitTargeting targeting;
    private UnitStatusEffects statusEffects;

    private const float ATTACK_COOLDOWN_BUFFER = 0.1f;

    [Header("Projectile Settings")]
    [SerializeField] private bool useProjectile;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Color projectileColor = Color.white;
    [SerializeField] private Transform projectileSpawnPoint;

    private void Awake()
    {
        statusEffects = GetComponent<UnitStatusEffects>();
        targeting = GetComponent<UnitTargeting>();
    }

    private void Start()
    {
        unit = GetComponent<Unit>();
        stats = unit.GetComponent<UnitStats>();
        view = unit.GetComponent<UnitView>();
        attackTimer = 0f;
    }

    private void Update()
    {
        if (unit.IsDead) return;
        
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (CanAttack())
        {
            TryAttackTarget();
        }
    }

    private void TryAttackTarget()
    {
        // Ưu tiên tấn công Unit target
        if (targeting.CurrentTarget != null && !targeting.CurrentTarget.IsDead)
        {
            if (IsInAttackRange(targeting.CurrentTarget.OccupiedCell))
            {
                PerformAttack(targeting.CurrentTarget);
                ResetAttackTimer();
                return;
            }
        }

        if (targeting.CurrentCardTarget != null)
        {
            var cardStats = targeting.CurrentCardTarget.GetComponent<CardStats>();
            if (cardStats != null && cardStats.CurrentHp > 0)
            {
                if (IsInAttackRange(targeting.CurrentCardTarget.occupiedHex))
                {
                    PerformAttackOnCard(targeting.CurrentCardTarget);
                    ResetAttackTimer();
                }
            }
        }
    }

    private bool IsInAttackRange(HexCell targetCell)
    {
        if (targetCell == null || unit.OccupiedCell == null) return false;
        return unit.OccupiedCell.Coordinates.DistanceTo(targetCell.Coordinates) <= stats.GetRange();
    }

    private void PerformAttack(Unit target)
    {
        float damage = stats.GetPhysicalDamage();

        if (stats.RollForCritical())
        {
            damage = stats.CalculateCriticalDamage(damage);
        }

        bool faceRight = target.transform.position.x > transform.position.x;
        view.FlipSprite(faceRight);
        view.PlayAttackAnimation();

        if (useProjectile && projectilePrefab != null)
        {
            Vector3 spawnPos = projectileSpawnPoint != null ?
                projectileSpawnPoint.position : transform.position;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var projectile = proj.GetComponent<Projectile>();
            projectile.Initialize(target, damage, projectileColor, null);
        }
        else
        {
            target.TakeDamage(damage, DamageType.Physical, unit);
            view.PlayAttackEffect();
        }

        UnitEvents.Combat.RaiseDamageDealt(unit, target, damage);
    }

    private void PerformAttackOnCard(CardController card)
    {
        float damage = stats.GetPhysicalDamage();

        if (useProjectile && projectilePrefab != null)
        {
            Vector3 spawnPos = projectileSpawnPoint != null ? 
                projectileSpawnPoint.position : transform.position;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var projectile = proj.GetComponent<Projectile>();
            projectile.InitializeCardTarget(card, damage, projectileColor);
        }
        else
        {
            var cardStats = card.GetComponent<CardStats>();
            if (cardStats != null)
            {
                // cardStats.TakeDamage(damage, DamageType.Physical);
            }
        }

        view.PlayAttackAnimation();
        view.FlipSprite(card.transform.position.x > transform.position.x);
    }

    private bool CanAttack() => attackTimer <= 0;

    private void ResetAttackTimer()
    {
        attackTimer = (1f / stats.GetAttackSpeed()) + ATTACK_COOLDOWN_BUFFER;
    }
}
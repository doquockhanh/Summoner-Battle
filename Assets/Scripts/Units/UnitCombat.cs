using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitTargeting))]
public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitView view;
    private float attackTimer;
    private UnitTargeting targeting;
    private UnitStatusEffects statusEffects;
    private HexGrid hexGrid;
    private HexPathFinder pathFinder;
    private List<HexCell> currentPath;
    private int currentPathIndex;

    private HexCell registeredCell;
    private float Speed => unit.GetUnitStats().GetMoveSpeed();
    private int AttackRange => unit.GetUnitStats().GetRange();
    private HexCell lastTarget;
    private bool autoCombat = true;
    private Coroutine autoCombatCoroutine;

    private const float ATTACK_COOLDOWN_BUFFER = 0.1f;
    private const float centerHexOffset = 0.1f;

    [Header("Projectile Settings")]
    [SerializeField] private bool useProjectile;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Color projectileColor = Color.white;
    [SerializeField] private Transform projectileSpawnPoint;

    private void Awake()
    {
        statusEffects = GetComponent<UnitStatusEffects>();
        targeting = GetComponent<UnitTargeting>();
        unit = GetComponent<Unit>();
        stats = unit.GetComponent<UnitStats>();
        view = unit.GetComponent<UnitView>();
    }

    private void Start()
    {
        attackTimer = 0f;
        hexGrid = HexGrid.Instance;
        pathFinder = new HexPathFinder(hexGrid);
        currentPath = null;
        currentPathIndex = 0;
        unit.GetUnitStats().OnDeath += Reset;

        if (targeting == null)
        {
            Debug.LogError("Ko thể thiếu targeting cho Unitcombat");
        }
    }

    private void FixedUpdate()
    {
        if (unit.IsDead || autoCombat == false) return;

        if (attackTimer > 0)
            attackTimer -= Time.fixedDeltaTime;

        if (targeting.CurrentTarget == null && targeting.CurrentCardTarget == null)
            return;

        if (IsInCenterOfHex())
        {
            UnitTryDeciding();
        }
        else
        {
            if (!CanMove()) return;
            Move();
        }
    }

    private void UnitTryDeciding()
    {
        // Kiểm tra xem đang có target trong tầm đánh không.
        bool canAttack = false;

        if (targeting.CurrentTarget != null && !targeting.CurrentTarget.IsDead)
        {
            canAttack = targeting.IsInAttackRange(targeting.CurrentTarget);
        }
        else if (targeting.CurrentCardTarget != null)
        {
            canAttack = IsInAttackRange(targeting.CurrentCardTarget.occupiedHex);
        }

        if (canAttack)
        {
            TryAttackTarget();
            return;
        }
        else
        {
            // Nếu ko thể tấn công thì đi tiếp
            HexCell targetCell = GetTargetCell();
            if (targetCell == null) return;

            List<HexCell> newPath = HandleFindPath(targetCell);
            if (newPath != null)
            {
                currentPath = newPath;
                registeredCell?.UnregisterUnit();
                registeredCell = null;
            }

            // Đăng ký ô tiếp theo
            if (currentPath == null || currentPathIndex >= currentPath.Count) return;
            HexCell nextCell = currentPath[currentPathIndex];
            registeredCell = nextCell;
            registeredCell.RegisterUnit(unit);
            currentPathIndex++;
        }
    }

    private bool IsInCenterOfHex()
    {
        // Kiểm tra xem unit đã đến được trung tâm ô cần đến chưa
        // Nếu có ô đăng ký thì kiểm tra xem đã đến ô đăng ký chưa
        if (registeredCell != null)
            return Vector3.Distance(transform.position, registeredCell.WorldPosition) <= centerHexOffset;
        return Vector3.Distance(transform.position, unit.OccupiedCell.WorldPosition) <= centerHexOffset;
    }

    private void Move()
    {
        // Di chuyển đến vị trí tiếp theo
        HexCell hexToMove = registeredCell ?? unit.OccupiedCell;
        transform.position = Vector2.MoveTowards(
            transform.position,
            hexToMove.WorldPosition,
            Speed * Time.fixedDeltaTime
        );

        HexCell newCell = hexGrid.GetCellAtPosition(transform.position);
        // Cập nhật occupied cell
        HexGrid.Instance.OccupyCell(newCell, unit);

        // Cập nhật animation
        view.SetMoving(true);
        view.FlipSprite(hexToMove.WorldPosition.x - transform.position.x > 0);
    }

    private bool TryAttackTarget()
    {
        if (!CanAttack()) return false;

        // Ưu tiên tấn công Unit target
        if (targeting.CurrentTarget != null && !targeting.CurrentTarget.IsDead)
        {
            if (IsInAttackRange(targeting.CurrentTarget.OccupiedCell))
            {
                PerformAttack(targeting.CurrentTarget);
                ResetAttackTimer();
                return true;
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
                    return true;
                }
            }
        }

        return false;
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
            projectile.Initialize(target, damage, projectileColor, unit);
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
            if (card.TryGetComponent<CardStats>(out var cardStats))
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

    private HexCell GetTargetCell()
    {
        // Ưu tiên unit target
        if (targeting.CurrentTarget != null && targeting.CurrentTarget.OccupiedCell != null)
        {
            return targeting.CurrentTarget.OccupiedCell;
        }

        // Nếu không có unit target, kiểm tra card target
        if (targeting.CurrentCardTarget != null && targeting.CurrentCardTarget.occupiedHex != null)
        {
            return targeting.CurrentCardTarget.occupiedHex;
        }

        return null;
    }

    public List<HexCell> HandleFindPath(HexCell target)
    {
        if (target != lastTarget)
        {
            lastTarget = target;
            currentPathIndex = 0;
            return pathFinder.FindPath(unit.OccupiedCell, target, AttackRange);
        }


        if (targeting.IsInAttackRange(targeting.CurrentTarget) || targeting.IsCurrentTargetMoved() || targeting.IsTargetChanged() || IsPathBlocked())
        {
            currentPathIndex = 0;
            return pathFinder.FindPath(unit.OccupiedCell, target, AttackRange);
        }

        return null;
    }

    private bool CanMove()
    {
        if (statusEffects == null) return true;
        return statusEffects.CanAct();
    }


    private bool IsPathBlocked()
    {
        if (currentPathIndex < currentPath.Count - 1)
        {
            HexCell hexCell = currentPath[currentPathIndex + 1];
            if (hexCell.IsOccupied && hexCell.OccupyingUnit != unit) return true;
        }

        return false;
    }


    public void TurnOnAutoCombat()
    {
        autoCombat = true;
    }

    public void TurnOffAutoCombat()
    {
        autoCombat = false;
    }

    public void TurnOffAutoCombatTemporarily(float duration)
    {
        if (autoCombatCoroutine != null)
        {
            StopCoroutine(autoCombatCoroutine);
        }
        autoCombatCoroutine = StartCoroutine(TurnOffAutoCombatCoroutine(duration));
    }

    private IEnumerator TurnOffAutoCombatCoroutine(float duration)
    {
        TurnOffAutoCombat();
        yield return new WaitForSeconds(duration);
        TurnOnAutoCombat();
        autoCombatCoroutine = null;
    }

    public void Reset()
    {
        unit.OccupiedCell?.SetUnit(null);
        registeredCell?.UnregisterUnit();
        unit.SetOccupiedCell(null);
        registeredCell = null;
    }

    public void SetRegisteredCell(HexCell cell)
    {
        registeredCell?.UnregisterUnit();
        registeredCell = cell;
        registeredCell?.RegisterUnit(unit);
    }
}
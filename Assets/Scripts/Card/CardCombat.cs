using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardCombat : MonoBehaviour
{
    [Header("Combat Settings")]

    [Tooltip("Scan rate is rate to check enemies nearby")]
    public float scanRate = 0.5f;

    private CardStats cardStats;
    private CardController cardController;
    private float attackTimer;
    private const float ATTACK_COOLDOWN_BUFFER = 0.1f;

    [Header("Projectile Settings")]
    [SerializeField] private bool useProjectile;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Color projectileColor = Color.white;
    [SerializeField] private Transform projectileSpawnPoint;

    public void Initialize(CardController cardController)
    {
        this.cardController = cardController;
        cardStats = GetComponent<CardStats>();
        StartCoroutine(StartCombat());
        ResetAttackTimer();
    }

    public IEnumerator StartCombat()
    {
        while (true)
        {
            if (!CanAttack()) yield return new WaitForSeconds(scanRate);
            Unit target = SearchingTarget();

            if (target == null)
            {
                yield return new WaitForSeconds(scanRate);
                continue;
            }

            PerformAttack(target);
            ResetAttackTimer();
            yield return new WaitForSeconds(scanRate);
        }

    }

    private Unit SearchingTarget()
    {
        if (HexGrid.Instance == null) return null;

        Unit target = null;

        List<HexCell> cellHasEms = HexGrid.Instance
                                .GetCellsInRange(cardController.occupiedHex.Coordinates, cardStats.GetRange())
                                .Where(cell => cell.IsOccupied && cell.OccupyingUnit.IsPlayerUnit != cardController.IsPlayer)
                                .ToList();

        if (cellHasEms.Count == 0) return null;

        var targetsByDistance = new Dictionary<int, List<Unit>>();
        int closestDistance = int.MaxValue;
        foreach (HexCell cell in cellHasEms)
        {
            Unit unit = cell.OccupyingUnit;
            int distance = cell.Coordinates.DistanceTo(unit.OccupiedCell.Coordinates);
            closestDistance = Mathf.Min(closestDistance, distance);

            if (!targetsByDistance.ContainsKey(distance))
            {
                targetsByDistance[distance] = new List<Unit>();
            }
            targetsByDistance[distance].Add(cell.OccupyingUnit);
        }

        if (targetsByDistance.Count > 0)
        {
            List<Unit> closestTargets = targetsByDistance[closestDistance];

            if (closestTargets.Count > 1)
            {
                int randomIndex = Random.Range(0, closestTargets.Count);
                target = closestTargets[randomIndex];
            }
            else
            {
                target = closestTargets[0];
            }
        }
        else
        {
            target = null;
        }

        return target;
    }

    private bool CanAttack() => attackTimer <= 0;

    private void PerformAttack(Unit target)
    {
        float damage = cardStats.GetPhysicalDamage();

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
            target.TakeDamage(damage, DamageType.Physical);
        }
        if (cardController != null && cardController.Unit != null && cardController.Unit.unitName != null && cardController.Unit != null)
        {
            BattleStatsManager.Instance?.AddDamageDealtToCard(cardController.Unit.unitName, damage);
        }
        BattleStatsManager.Instance?.AddDamageTakenToUnit(target, damage);
    }

    private void ResetAttackTimer()
    {
        attackTimer = (1f / cardStats.GetAttackSpeed()) + ATTACK_COOLDOWN_BUFFER;
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        StopCoroutine(StartCombat());
    }
}
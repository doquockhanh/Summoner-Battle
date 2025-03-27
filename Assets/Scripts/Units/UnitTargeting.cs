using UnityEngine;
using System.Collections.Generic;

public class UnitTargeting : MonoBehaviour
{
    public bool autoTargeting = true;
    private Unit currentTarget;
    private HexCell currentTargetHex;
    private bool targetChanged = false;
    private CardController currentCardTarget;
    private int detectRange => stats.GetDetectRange();
    private int attackRange => stats.GetRange();
    private HexGrid hexGrid;
    private Unit unit;
    private UnitStats stats;
    public Unit CurrentTarget => currentTarget;
    public CardController CurrentCardTarget => currentCardTarget;
    private HexCell occupiedCell => unit.OccupiedCell;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        stats = unit.GetComponent<UnitStats>();
        hexGrid = HexGrid.Instance;
    }

    private void FixedUpdate()
    {
        if (unit == null || unit.IsDead) return;

        if (autoTargeting)
        {
            AutoTargeting();
        }
    }

    private void AutoTargeting()
    {
        // Kiểm tra unit target
        if (!IsUnitValidToTargeted(currentTarget))
        {
            currentTarget = null;
            FindNewTarget();
            if (currentTarget != null)
            {
                currentTargetHex = currentTarget.OccupiedCell;
                targetChanged = true;
            }
        }


        if (!IsCardValidToTarget(currentCardTarget))
        {
            currentCardTarget = null;
            FindNearestEnemyCard();
        }

    }

    private bool IsUnitValidToTargeted(Unit unit)
    {
        if (unit == null) return false;
        if (unit.IsPlayerUnit == this.unit.IsPlayerUnit) return false;
        if (unit.IsDead) return false;
        if (!IsInDetectRange(unit)) return false;
        return true;
    }

    private bool IsCardValidToTarget(CardController card)
    {
        if (card == null) return false;
        if (card.IsPlayer == unit.IsPlayerUnit) return false;

        var cardStats = card.GetComponent<CardStats>();
        if (cardStats == null || (cardStats != null && cardStats.CurrentHp <= 0)) return false;
        if (card.occupiedHex == null) return false;

        return true;
    }

    private void FindNewTarget()
    {
        if (unit.OccupiedCell == null) return;

        // Tìm unit trong tầm detect như cũ
        List<HexCell> cellsInRange = hexGrid.GetCellsInRange(unit.OccupiedCell.Coordinates, detectRange);
        var targetsByDistance = new Dictionary<int, List<Unit>>();
        int closestDistance = int.MaxValue;

        foreach (HexCell cell in cellsInRange)
        {
            if (cell.OccupyingUnit != null &&
                !cell.OccupyingUnit.GetUnitStats().IsDead &&
                cell.OccupyingUnit.IsPlayerUnit != unit.IsPlayerUnit)
            {
                int distance = cell.Coordinates.DistanceTo(unit.OccupiedCell.Coordinates);
                closestDistance = Mathf.Min(closestDistance, distance);

                if (!targetsByDistance.ContainsKey(distance))
                {
                    targetsByDistance[distance] = new List<Unit>();
                }
                targetsByDistance[distance].Add(cell.OccupyingUnit);
            }
        }

        // Nếu tìm thấy unit trong tầm
        if (targetsByDistance.Count > 0)
        {
            List<Unit> closestTargets = targetsByDistance[closestDistance];
            if (closestTargets.Count > 1)
            {
                int randomIndex = Random.Range(0, closestTargets.Count);
                currentTarget = closestTargets[randomIndex];
            }
            else
            {
                currentTarget = closestTargets[0];
            }
            return;
        }
    }

    private void FindNearestEnemyCard()
    {
        CardController nearestCard = null;
        float minDistance = float.MaxValue;

        List<CardController> allCards = BattleManager.Instance.ActiveCards;

        foreach (CardController card in allCards)
        {
            if (IsCardValidToTarget(card))
            {
                float distance = unit.OccupiedCell.Coordinates.DistanceTo(card.occupiedHex.Coordinates);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCard = card;
                }
            }
        }

        currentCardTarget = nearestCard;
    }

    public bool IsCurrentTargetMoved()
    {
        if (currentTarget == null || currentTarget.OccupiedCell == null || currentTargetHex == null) return false;

        if (currentTarget.OccupiedCell.WorldPosition != currentTargetHex.WorldPosition)
        {
            currentTargetHex = currentTarget.OccupiedCell;
            return true;
        }

        return false;
    }

    public bool IsTargetChanged()
    {
        if (targetChanged)
        {
            targetChanged = false;
            return true;
        }

        return false;
    }

    public bool IsInDetectRange(Unit target)
    {
        if (target == null || target.OccupiedCell == null) return false;

        return unit.OccupiedCell.Coordinates.DistanceTo(target.OccupiedCell.Coordinates) <= detectRange;
    }

    public bool IsInAttackRange(Unit target)
    {
        if (target == null || target.OccupiedCell == null) return false;
        return unit.OccupiedCell.Coordinates.DistanceTo(target.OccupiedCell.Coordinates) <= attackRange;
    }


    public void SetTarget(Unit newTarget)
    {
        // Kiểm tra target mới có hợp lệ không
        if (!IsValidEnemy(newTarget))
        {
            Debug.LogWarning("Cố gắng set target không hợp lệ");
            return;
        }

        // Kiểm tra target có trong tầm detect không
        if (!IsInDetectRange(newTarget))
        {
            Debug.LogWarning("Target được chỉ định nằm ngoài tầm phát hiện");
            return;
        }

        // Set target mới
        currentTarget = newTarget;
    }

    public bool IsValidAlly(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;

        // Kiểm tra cùng phe
        if (this.unit.IsPlayerUnit != unit.IsPlayerUnit) return false;

        // Kiểm tra có thể target không
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null && !statusEffects.IsTargetable) return false;

        return true;
    }

    public bool IsValidEnemy(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;

        // Kiểm tra khác phe
        if (this.unit.IsPlayerUnit == unit.IsPlayerUnit) return false;

        // Kiểm tra có thể target không
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null && !statusEffects.IsTargetable) return false;

        return true;
    }
}
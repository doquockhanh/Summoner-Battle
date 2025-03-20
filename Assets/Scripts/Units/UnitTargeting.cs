using UnityEngine;
using System.Collections.Generic;

public class UnitTargeting : MonoBehaviour
{
    public bool autoTargeting = true;
    private Unit currentTarget;
    private int detectRange => GetComponent<UnitStats>().GetDetectRange();
    private float attackRange => GetComponent<UnitStats>().GetRange();
    private HexGrid hexGrid;
    private Unit unit;
    private UnitStats stats;
    public Unit CurrentTarget => currentTarget;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        stats = unit.GetComponent<UnitStats>();
    }

    private void Start()
    {
        hexGrid = HexGrid.Instance;
    }

    private void FixedUpdate()
    {
        if (autoTargeting)
        {
            AutoTargeting();
        }
    }

    private void AutoTargeting()
    {
        // Kiểm tra target hiện tại
        if (currentTarget == null ||
            currentTarget.IsDead ||
            !IsInDetectRange(currentTarget))
        {
            FindNewTarget();
        }
    }

    private void FindNewTarget()
    {
        if(unit.OccupiedCell == null) return;
        // Lấy các ô trong tầm phát hiện
        var cellsInRange = hexGrid.GetCellsInRange(unit.OccupiedCell.Coordinates, detectRange);

        // Dictionary để nhóm các unit theo khoảng cách
        var targetsByDistance = new Dictionary<int, List<Unit>>();
        int closestDistance = int.MaxValue;

        // Phân loại các unit theo khoảng cách
        foreach (var cell in cellsInRange)
        {
            if (cell.OccupyingUnit != null &&
                !cell.OccupyingUnit.IsDead &&
                cell.OccupyingUnit.IsPlayerUnit != unit.IsPlayerUnit)
            {
                int distance = cell.Coordinates.DistanceTo(unit.OccupiedCell.Coordinates);

                // Chỉ quan tâm đến khoảng cách trong detect range
                if (distance <= detectRange)
                {
                    // Cập nhật khoảng cách gần nhất
                    closestDistance = Mathf.Min(closestDistance, distance);

                    // Thêm unit vào nhóm cùng khoảng cách
                    if (!targetsByDistance.ContainsKey(distance))
                    {
                        targetsByDistance[distance] = new List<Unit>();
                    }
                    targetsByDistance[distance].Add(cell.OccupyingUnit);
                }
            }
        }

        // Nếu tìm thấy target
        if (targetsByDistance.Count > 0)
        {
            // Lấy danh sách các unit ở khoảng cách gần nhất
            var closestTargets = targetsByDistance[closestDistance];

            // Nếu có nhiều hơn 1 target ở cùng khoảng cách gần nhất
            if (closestTargets.Count > 1)
            {
                int randomIndex = Random.Range(0, closestTargets.Count);
                currentTarget = closestTargets[randomIndex];
            }
            else
            {
                currentTarget = closestTargets[0];
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    public bool IsInDetectRange(Unit target)
    {
        if (target == null && target.OccupiedCell == null) return false;

        return unit.OccupiedCell.Coordinates.DistanceTo(target.OccupiedCell.Coordinates) <= detectRange;
    }

    public bool IsInAttackRange(Unit target)
    {
        if (target == null && target.OccupiedCell == null) return false;

        return unit.OccupiedCell.Coordinates.DistanceTo(target.OccupiedCell.Coordinates) <= attackRange;
    }


    public void SetTarget(Unit newTarget)
    {
        // Kiểm tra target mới có hợp lệ không
        if (IsValidEnemy(newTarget))
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
        if (unit.IsPlayerUnit != unit.IsPlayerUnit) return false;
        
        // Kiểm tra có thể target không
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null && !statusEffects.IsTargetable) return false;

        return true;
    }

    public bool IsValidEnemy(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;
        
        // Kiểm tra khác phe
        if (unit.IsPlayerUnit == unit.IsPlayerUnit) return false;
        
        // Kiểm tra có thể target không
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null && !statusEffects.IsTargetable) return false;

        return true;
    }
}
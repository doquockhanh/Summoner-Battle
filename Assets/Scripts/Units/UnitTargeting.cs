using UnityEngine;
using System.Collections.Generic;

public class UnitTargeting : MonoBehaviour
{
    public bool autoTargeting = true;
    private Unit currentTarget;
    private float detectRange;
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
        detectRange = GetComponent<UnitStats>().Data.detectRange;
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
        // Lấy các ô trong tầm phát hiện
        var currentCell = hexGrid.GetCellAtPosition(transform.position);
        var cellsInRange = hexGrid.GetCellsInRange(currentCell.Coordinates, Mathf.RoundToInt(detectRange));
        
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
                int distance = cell.Coordinates.DistanceTo(currentCell.Coordinates);
                
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

    private bool IsInDetectRange(Unit target)
    {
        if (target == null) return false;

        var targetCell = hexGrid.GetCellAtPosition(target.transform.position);
        var currentCell = hexGrid.GetCellAtPosition(transform.position);

        return targetCell.Coordinates.DistanceTo(currentCell.Coordinates) <= detectRange;
    }

    public void SetTarget(Unit newTarget)
    {
        // Kiểm tra target mới có hợp lệ không
        if (newTarget == null || newTarget.IsDead || newTarget.IsPlayerUnit == unit.IsPlayerUnit)
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
}
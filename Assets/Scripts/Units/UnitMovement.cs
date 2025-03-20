using UnityEngine;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private HexGrid hexGrid;
    private HexPathFinder pathFinder;
    private UnitStatusEffects statusEffects;
    private List<HexCell> currentPath;
    private int currentPathIndex;

    // Thêm biến lưu trữ ô hiện tại
    private HexCell occupiedCell;
    public HexCell OccupiedCell => occupiedCell;
    private float speed => unit.GetUnitStats().GetMoveSpeed();
    private int attackRange => unit.GetUnitStats().GetRange();

    private void Start()
    {
        unit = GetComponent<Unit>();
        hexGrid = HexGrid.Instance;
        pathFinder = new HexPathFinder(hexGrid);
        statusEffects = GetComponent<UnitStatusEffects>();
        currentPath = null;
        currentPathIndex = 0;

        // Khởi tạo ô ban đầu
        occupiedCell = hexGrid.GetCellAtPosition(transform.position);
        if (occupiedCell != null)
        {
            occupiedCell.SetUnit(unit);
        }
    }

    private void OnDestroy()
    {
        // Giải phóng ô khi unit bị hủy
        if (occupiedCell != null)
        {
            occupiedCell.SetUnit(null);
        }
    }

    public void Move(HexCell target)
    {
        // Kiểm tra điều kiện di chuyển
        if (!CanMove())
        {
            return;
        }

        // Nếu chưa có đường đi hoặc ô tiếp theo bị chiếm, tìm đường đi mới
        if (currentPath == null ||
            IsNextCellOccupied() ||
            currentPathIndex >= currentPath.Count)
        {
            currentPath = pathFinder.FindPath(occupiedCell, target, attackRange);
            currentPathIndex = 0;

            // Không tìm được đường đi
            if (currentPath == null || currentPath.Count == 0)
            {
                return;
            }
        }

        // Di chuyển đến ô tiếp theo trong path
        MoveAlongPath();
    }

    private bool CanMove()
    {
        if (statusEffects == null) return true;

        return statusEffects.CanAct();
    }

    private bool IsNextCellOccupied()
    {
        if (currentPath == null ||
            currentPathIndex >= currentPath.Count ||
            currentPathIndex < 0)
        {
            return false;
        }

        return currentPath[currentPathIndex].IsOccupied;
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            return;
        }

        // Lấy ô tiếp theo
        HexCell nextCell = currentPath[currentPathIndex];

        // Nếu mới bắt đầu di chuyển đến ô tiếp theo
        if (occupiedCell != nextCell && !nextCell.IsOccupied)
        {
            // Cập nhật trạng thái các ô
            if (occupiedCell != null)
            {
                occupiedCell.SetUnit(null);
            }
            nextCell.SetUnit(unit);
            occupiedCell = nextCell;
        }

        // Di chuyển về phía ô tiếp theo
        Vector3 targetPosition = nextCell.WorldPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Kiểm tra xem đã đến ô tiếp theo chưa
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private HexGrid hexGrid;
    private HexPathFinder pathFinder;
    private UnitStatusEffects statusEffects;
    private UnitTargeting targeting;
    private List<HexCell> currentPath;
    private int currentPathIndex;
    private float pathUpdateTimer;
    private const float PATH_UPDATE_INTERVAL = 1f;

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
        targeting = GetComponent<UnitTargeting>();
        currentPath = null;
        currentPathIndex = 0;
        pathUpdateTimer = 0f;

        // Khởi tạo ô ban đầu
        occupiedCell = hexGrid.GetCellAtPosition(transform.position);
        if (occupiedCell != null)
        {
            occupiedCell.SetUnit(unit);
        }
    }

    private void Update()
    {
        if (unit.IsDead) return;
        if (!CanMove()) return;

        HexCell targetCell = GetTargetCell();
        if (targetCell != null)
        {
            Move(targetCell);
        }
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
        if (!CanMove() || target == null) return;

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= PATH_UPDATE_INTERVAL || currentPath == null)
        {
            HandleFindPath(target);
            pathUpdateTimer = 0f;
        }

        MoveAlongPath();
    }

    public void HandleFindPath(HexCell target)
    {
        if (target == null || occupiedCell == null) return;

        // Tìm đường đi mới nếu:
        // - Chưa có đường đi
        // - Ô tiếp theo bị chiếm
        // - Đã đi hết đường
        if (currentPath == null ||
            IsNextCellOccupied() ||
            currentPathIndex >= currentPath.Count)
        {
            currentPath = pathFinder.FindPath(occupiedCell, target, attackRange);
            currentPathIndex = 0;
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath == null ||
            currentPathIndex >= currentPath.Count ||
            !CanMove()) return;

        HexCell nextCell = currentPath[currentPathIndex];
        if (nextCell == null) return;

        // Chiếm ô tiếp theo nếu chưa bị chiếm
        if (occupiedCell != nextCell)
        {
            bool occupied = HexGrid.Instance.OccupyCell(nextCell, unit);
            if (occupied)
            {
                occupiedCell = nextCell;
            }
        }

        // Di chuyển đến vị trí tiếp theo
        Vector2 targetPosition = nextCell.WorldPosition;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Cập nhật animation
        var view = unit.GetComponent<UnitView>();
        view.SetMoving(true);
        view.FlipSprite(targetPosition.x - transform.position.x > 0);

        // Kiểm tra đã đến ô tiếp theo chưa
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
            view.SetMoving(false);
        }
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
}
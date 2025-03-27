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
    // private HexCell occupiedCell;
    private HexCell registeredCell;
    private float speed => unit.GetUnitStats().GetMoveSpeed();
    private int attackRange => unit.GetUnitStats().GetRange();
    private HexCell lastTarget;
    private UnitView view;
    private bool autoMoving = true;

    private void Start()
    {
        unit = GetComponent<Unit>();
        hexGrid = HexGrid.Instance;
        pathFinder = new HexPathFinder(hexGrid);
        statusEffects = GetComponent<UnitStatusEffects>();
        targeting = GetComponent<UnitTargeting>();
        currentPath = null;
        currentPathIndex = 0;
        view = unit.GetComponent<UnitView>();

        unit.GetUnitStats().OnDeath += Reset;
    }

    private void Update()
    {
        if (unit.IsDead)
        {
            Reset();
            return;
        }

        if (!autoMoving) return;

        if (!CanMove()) return;

        Move(GetTargetCell());
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

    public void Move(HexCell target)
    {
        if (!CanMove() || target == null) return;

        List<HexCell> newPath = HandleFindPath(target);
        if (newPath != null)
        {
            currentPath = newPath;
            registeredCell?.UnregisterUnit();
            registeredCell = null;
        }

        MoveAlongPath(currentPath, currentPathIndex);
    }

    public List<HexCell> HandleFindPath(HexCell target)
    {
        if (target != lastTarget)
        {
            lastTarget = target;
            currentPathIndex = 0;
            return pathFinder.FindPath(unit.OccupiedCell, target, attackRange);
        }


        if (currentPath == null || targeting.IsCurrentTargetMoved() || targeting.IsTargetChanged() || IsPathBlocked())
        {
            currentPathIndex = 0;
            return pathFinder.FindPath(unit.OccupiedCell, target, attackRange);
        }

        return null;
    }

    private void MoveAlongPath(List<HexCell> path, int index)
    {
        if (path == null || index >= path.Count || !CanMove())
        {
            view.SetMoving(false);
            return;
        }

        HexCell nextCell = path[index];
        if (nextCell == null)
        {
            view.SetMoving(false);
            return;
        }

        // Đăng ký ô tiếp theo
        registeredCell = nextCell;
        registeredCell.RegisterUnit(unit);

        // Di chuyển đến vị trí tiếp theo
        Vector2 targetPosition = registeredCell.WorldPosition;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        HexCell newCell = hexGrid.GetCellAtPosition(transform.position);
        // Cập nhật occupied cell
        HexGrid.Instance.OccupyCell(newCell, unit);

        // Cập nhật animation
        view.SetMoving(true);
        view.FlipSprite(targetPosition.x - transform.position.x > 0);

        // Kiểm tra đã đến ô tiếp theo chưa
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
            registeredCell.UnregisterUnit();
        }
    }

    private bool CanMove()
    {
        if (statusEffects == null) return true;
        return statusEffects.CanAct();
    }

    public void TurnOnAutoMoving()
    {
        autoMoving = true;
    }

    public void TurnOffAutoMoving()
    {
        autoMoving = false;
    }


    private bool IsPathBlocked()
    {
        if (currentPath == null) return false;

        foreach (HexCell hexCell in currentPath)
        {
            if (hexCell.IsOccupied) return true;
        }

        return false;
    }

    private void Reset()
    {
        unit.OccupiedCell?.SetUnit(null);
        registeredCell?.UnregisterUnit();
        unit.SetOccupiedCell(null);
        registeredCell = null;
    }
}
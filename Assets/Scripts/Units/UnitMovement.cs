using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private HexCell currentCell;
    private HexCell targetCell;
    private bool isMoving;
    private float moveSpeed;
    private Vector3 moveDirection;
    private UnitTargeting unitTargeting;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
        this.unitTargeting = unit.GetComponent<UnitTargeting>();
        moveSpeed = unit.GetUnitStats().Data.moveSpeed;
        
        // Đăng ký unit với hex grid
        UpdateCurrentCell();
    }

    private void UpdateCurrentCell()
    {
        var newCell = HexGrid.Instance.GetCellAtPosition(transform.position);
        if (newCell != currentCell)
        {
            if (currentCell != null)
            {
                currentCell.SetUnit(null);
            }
            currentCell = newCell;
            if (currentCell != null)
            {
                currentCell.SetUnit(unit);
            }
        }
    }

    public void Move(Unit targetUnit, Base targetBase)
    {
        if (unit.IsDead) return;

        if (targetUnit != null)
        {
            MoveTowardsTarget(targetUnit);
        }
        else if (targetBase != null)
        {
            MoveTowardsBase(targetBase);
        }
    }

    private void MoveTowardsTarget(Unit target)
    {
        if (unitTargeting.IsInRange(target)) 
        {
            StopMoving();
            return;
        }

        HexCell targetHexCell = HexGrid.Instance.GetCellAtPosition(target.transform.position);
        if (targetHexCell != currentCell)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            
            // Kiểm tra cell mới trước khi di chuyển
            var nextCell = HexGrid.Instance.GetCellAtPosition(newPosition);
            if (nextCell != null && !nextCell.IsOccupied)
            {
                transform.position = newPosition;
                UpdateCurrentCell();
                
                // Cập nhật animation
                var view = unit.GetComponent<UnitView>();
                view.SetMoving(true);
                view.FlipSprite(direction.x > 0);
            }
        }
    }

    private void MoveTowardsBase(Base targetBase)
    {
        if (unitTargeting.IsInRangeOfBase())
        {
            StopMoving();
            return;
        }

        Vector3 direction = (targetBase.transform.position - transform.position).normalized;
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        
        var nextCell = HexGrid.Instance.GetCellAtPosition(newPosition);
        if (nextCell != null && !nextCell.IsOccupied)
        {
            transform.position = newPosition;
            UpdateCurrentCell();
            
            var view = unit.GetComponent<UnitView>();
            view.SetMoving(true);
            view.FlipSprite(direction.x > 0);
        }
    }

    private void StopMoving()
    {
        isMoving = false;
        unit.GetComponent<UnitView>().SetMoving(false);
    }

    private void OnDestroy()
    {
        if (currentCell != null)
        {
            currentCell.SetUnit(null);
        }
    }
}
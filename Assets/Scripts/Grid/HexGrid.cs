using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    public static HexGrid Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int width = 28;
    [SerializeField] private int height = 14;

    private Dictionary<HexCoord, HexCell> cells;
    [SerializeField] private bool turnOnCoordinates = false;

    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGrid()
    {
        cells = new Dictionary<HexCoord, HexCell>();

        // Tạo grid 14x28
        for (int q = 0; q < width; q++)
        {
            int rStart = -q / 2;
            int rEnd = height - q / 2;

            for (int r = rStart; r < rEnd; r++)
            {
                var coord = new HexCoord(q, r);
                cells[coord] = new HexCell(coord);
            }
        }
    }

    public HexCell GetCell(HexCoord coord)
    {
        return cells.TryGetValue(coord, out var cell) ? cell : null;
    }

    public HexCell GetCellAtPosition(Vector3 worldPos)
    {
        HexCoord coord = HexMetrics.WorldToHex(worldPos);
        return GetCell(coord);
    }

    public List<HexCell> GetCellsInRange(HexCoord center, int range)
    {
        var results = new List<HexCell>();

        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q - range);
                 r <= Mathf.Min(range, -q + range); r++)
            {
                var coord = new HexCoord(center.q + q, center.r + r);
                var cell = GetCell(coord);
                if (cell != null)
                {
                    results.Add(cell);
                }
            }
        }

        return results;
    }

    public IEnumerable<HexCell> GetAllCells()
    {
        return cells.Values;
    }

    public List<HexCell> GetNeighbors(HexCell cell)
    {
        if (cell == null) return new List<HexCell>();

        var neighbors = new List<HexCell>();
        var directions = new[]
        {
            new HexCoord(1, 0),   // Phải
            new HexCoord(1, -1),  // Phải dưới
            new HexCoord(0, -1),  // Dưới
            new HexCoord(-1, 0),  // Trái
            new HexCoord(-1, 1),  // Trái trên
            new HexCoord(0, 1)    // Trên
        };

        foreach (var dir in directions)
        {
            var neighborCoord = cell.Coordinates + dir;
            var neighbor = GetCell(neighborCoord);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public List<Unit> GetUnitsInRange(HexCoord center, int range, bool isPlayer)
    {
        var cellsInRange = GetCellsInRange(center, range);
        var units = new List<Unit>();

        foreach (var cell in cellsInRange)
        {
            Unit unitInCell = cell.OccupyingUnit;
            if (IsValidTarget(unitInCell, isPlayer))
            {
                units.Add(unitInCell);
            }
        }

        return units;
    }

    private bool IsValidTarget(Unit unit, bool isPlayerTeam)
    {
        if (unit == null || unit.IsDead) return false;

        // Kiểm tra khác phe
        if (unit.IsPlayerUnit != isPlayerTeam) return false;

        // Kiểm tra có thể target không
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null && !statusEffects.IsTargetable) return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        if (cells == null) return;

        // Vẽ từng hex cell
        foreach (var cell in cells.Values)
        {
            Vector3 center = cell.WorldPosition;

            // Vẽ viền hex
            Gizmos.color = cell.IsOccupied ? Color.red : Color.white;
            Gizmos.color = cell.IsRegistered ? Color.green : Gizmos.color;
            for (int i = 0; i < 6; i++)
            {
                Vector2 currentCorner = center + (Vector3)HexMetrics.Corners[i];
                Vector2 nextCorner = center + (Vector3)HexMetrics.Corners[(i + 1) % 6];
                Gizmos.DrawLine(currentCorner, nextCorner);
            }

            if (turnOnCoordinates)
            {
                // Vẽ tọa độ hex ở giữa cell
                Handles.Label(
                    center,
                    $"({cell.Coordinates.q}, {cell.Coordinates.r})"
                );
            }

        }
    }

    public bool OccupyCell(HexCell newCell, Unit unit)
    {
        if (newCell == null || unit == null || newCell.IsOccupied) return false;

        // Giải phóng ô cũ
        var currentCell = unit.OccupiedCell;
        currentCell?.SetUnit(null);

        // Chiếm ô mới
        newCell.SetUnit(unit);
        unit.SetOccupiedCell(newCell);
        return true;
    }

    public bool AssertOccupyCell(Vector3 worldPosition, Unit unit, int maxRange)
    {
        if (unit == null) return false;

        HexCoord targetCoord = HexMetrics.WorldToHex(worldPosition);

        HexCell nearestEmptyCell = FindNearestEmptyCell(targetCoord, maxRange);

        if (nearestEmptyCell != null)
        {
            return OccupyCell(nearestEmptyCell, unit);
        }

        return false;
    }

    public bool OccupyCell(HexCell newCell, CardController card)
    {
        if (newCell == null || card == null || newCell.IsOccupied) return false;

        // Giải phóng ô cũ nếu có
        card.occupiedHex?.SetUnit(null);

        // Chiếm ô mới
        newCell.SetUnit(card.GetActiveUnits().FirstOrDefault());
        card.occupiedHex = newCell;
        return true;
    }

    public HexCell FindSpotForAOESkill(int radius, bool isPlayer)
    {
        // Cache danh sách units cần check
        var validUnits = new Dictionary<HexCell, Unit>();
        foreach (var cell in cells.Values)
        {
            Unit unit = cell.OccupyingUnit;
            if (IsValidTarget(unit, isPlayer))
            {
                validUnits.Add(cell, unit);
            }
        }

        // Nếu không có units hợp lệ
        if (validUnits.Count == 0) return null;

        HexCell bestCell = null;
        int maxUnitsAffected = 0;
        var checkedCells = new HashSet<HexCell>();

        // Chỉ kiểm tra các ô xung quanh units
        foreach (var unitCell in validUnits.Keys)
        {
            // Lấy các ô có thể làm tâm AOE (bao gồm các ô xung quanh unit)
            var potentialCenters = GetCellsInRange(unitCell.Coordinates, radius);

            foreach (var centerCell in potentialCenters)
            {
                // Skip nếu đã check
                if (!checkedCells.Add(centerCell)) continue;

                // Đếm số unit bị ảnh hưởng
                var affectedCells = GetCellsInRange(centerCell.Coordinates, radius);
                int unitsAffected = affectedCells.Count(cell => validUnits.ContainsKey(cell));

                // Cập nhật vị trí tốt nhất
                if (unitsAffected > maxUnitsAffected)
                {
                    maxUnitsAffected = unitsAffected;
                    bestCell = centerCell;
                }
                else if (unitsAffected == maxUnitsAffected && unitsAffected > 0)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        bestCell = centerCell;
                    }
                }
            }
        }

        return bestCell;
    }

    public HexCell FindNearestEmptyCell(HexCoord center, int maxRange)
    {
        // Kiểm tra ô trung tâm trước
        var centerCell = GetCell(center);
        if (centerCell != null && !centerCell.IsOccupied && !centerCell.IsRegistered)
        {
            return centerCell;
        }

        // Tìm kiếm theo vòng tròn, từ gần đến xa
        for (int radius = 1; radius <= maxRange; radius++)
        {
            var cellsInRange = GetCellsInRange(center, radius);
            foreach (var cell in cellsInRange)
            {
                if (!cell.IsOccupied && !cell.IsRegistered)
                {
                    return cell;
                }
            }
        }

        return null; // Không tìm thấy ô trống nào
    }

    public HexCell FindNearestUnitCell(HexCoord center, int maxRange, bool isPlayerUnit)
    {
        HexCell nearestCell = null;
        float nearestDistance = float.MaxValue;

        // Tìm kiếm trong phạm vi maxRange
        for (int radius = 0; radius <= maxRange; radius++)
        {
            var cellsInRange = GetCellsInRange(center, radius);
            
            foreach (HexCell cell in cellsInRange)
            {
                if (IsValidTarget(cell.OccupyingUnit, isPlayerUnit))
                {
                    float distance = center.DistanceTo(cell.Coordinates);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCell = cell;
                    }
                }
            }

            // Nếu đã tìm thấy cell gần nhất trong vòng hiện tại, dừng tìm kiếm
            if (nearestCell != null)
            {
                break;
            }
        }

        return nearestCell;
    }
}
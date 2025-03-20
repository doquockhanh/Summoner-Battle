using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class HexGrid : MonoBehaviour
{
    public static HexGrid Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int width = 14;
    [SerializeField] private int height = 28;

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
        for (int r = 0; r < height; r++)
        {
            int qStart = -r / 2;
            int qEnd = width - r / 2;

            for (int q = qStart; q < qEnd; q++)
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
}
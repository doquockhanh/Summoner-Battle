using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class HexGrid : MonoBehaviour
{
    public static HexGrid Instance { get; private set; }

    private Dictionary<HexCoord, HexCell> cells;
    [SerializeField] private bool turnOnCoordinates = false;

    private int width = 14;
    private int height = 28;

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

        // Tạo lưới hex
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
        var coord = HexMetrics.WorldToHex(worldPos);
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

        // Vẽ đường phân chia giữa 2 phe
        Gizmos.color = Color.yellow;
        float midQ = width / 2f;
        Vector3 topMid = HexMetrics.HexToWorld(new HexCoord((int)midQ, 0));
        Vector3 bottomMid = HexMetrics.HexToWorld(new HexCoord((int)midQ, height - 1));
        Gizmos.DrawLine(topMid, bottomMid);
    }
}
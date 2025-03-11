using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    private static HexGrid instance;
    public static HexGrid Instance => instance;

    [Header("Grid Settings")]
    [SerializeField] private int width = 14;
    [SerializeField] private int height = 28;
    [SerializeField] private float hexSize = 1f;

    private Dictionary<HexCoordinates, HexCell> cells;

    public float HexSize => hexSize;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        cells = new Dictionary<HexCoordinates, HexCell>();

        // Tạo grid offset để center được grid
        int qOffset = -width / 2;
        int rOffset = -height / 2;

        // Tạo các ô hex
        for (int r = 0; r < height; r++)
        {
            int qStart = r / 2; // Offset cho mỗi row để tạo grid đều
            for (int q = -qStart; q < width - qStart; q++)
            {
                HexCoordinates coordinates = new HexCoordinates(q + qOffset, r + rOffset);
                cells[coordinates] = new HexCell(coordinates);
            }
        }
    }

    public bool IsValidCoordinates(HexCoordinates coordinates)
    {
        return cells.ContainsKey(coordinates);
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        return cells.TryGetValue(coordinates, out HexCell cell) ? cell : null;
    }

    public HexCell GetCellAtPosition(Vector3 worldPosition)
    {
        HexCoordinates coordinates = HexCoordinates.FromPosition(worldPosition, hexSize);
        return GetCell(coordinates);
    }

    public Vector3 GetCellCenter(HexCoordinates coordinates)
    {
        return coordinates.ToPosition(hexSize);
    }

    // Lấy danh sách các ô lân cận
    public List<HexCell> GetNeighbors(HexCoordinates coordinates)
    {
        List<HexCell> neighbors = new List<HexCell>();
        HexCell[] possibleNeighbors = new HexCell[6];
        
        // 6 hướng của hex
        HexCoordinates[] directions = {
            new HexCoordinates(1, 0), new HexCoordinates(1, -1), 
            new HexCoordinates(0, -1), new HexCoordinates(-1, 0),
            new HexCoordinates(-1, 1), new HexCoordinates(0, 1)
        };

        foreach (var dir in directions)
        {
            HexCoordinates neighborCoord = new HexCoordinates(
                coordinates.Q + dir.Q,
                coordinates.R + dir.R
            );
            
            if (IsValidCoordinates(neighborCoord))
            {
                neighbors.Add(GetCell(neighborCoord));
            }
        }

        return neighbors;
    }
} 
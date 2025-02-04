using UnityEngine;
using System.Collections.Generic;

public class RandomMovementHandler : MonoBehaviour
{
    private static RandomMovementHandler instance;
    public static RandomMovementHandler Instance => instance;

    [Header("Map Settings")]
    [SerializeField] private float mapWidth = 20f;
    [SerializeField] private float mapHeight = 10f;
    [SerializeField] private float gridSize = 1f;
    
    // Properties để Editor có thể truy cập
    public float MapWidth => mapWidth;
    public float MapHeight => mapHeight;
    public float GridSize => gridSize;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGrid = true;
    [SerializeField] private bool showVisitedPositions = true;

    private bool[,] visitedPositions;
    private int gridWidth, gridHeight;

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
        gridWidth = Mathf.CeilToInt(mapWidth / gridSize);
        gridHeight = Mathf.CeilToInt(mapHeight / gridSize);
        visitedPositions = new bool[gridWidth, gridHeight];
    }

    public Vector3 GetNextRandomPosition(Vector3 currentPos, bool isPlayerUnit)
    {
        List<Vector3> possiblePositions = new List<Vector3>();
        Vector2Int currentGrid = WorldToGrid(currentPos);

        // Tìm các ô lân cận chưa đi qua
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector2Int checkGrid = new Vector2Int(
                    currentGrid.x + x,
                    currentGrid.y + y
                );

                if (IsValidPosition(checkGrid, isPlayerUnit) && !IsVisited(checkGrid))
                {
                    possiblePositions.Add(GridToWorld(checkGrid));
                }
            }
        }

        // Nếu không có vị trí chưa đi qua, reset visited và tìm vị trí ngẫu nhiên
        if (possiblePositions.Count == 0)
        {
            ResetVisited();
            return GetRandomPosition(isPlayerUnit);
        }

        Vector3 nextPos = possiblePositions[Random.Range(0, possiblePositions.Count)];
        MarkVisited(WorldToGrid(nextPos));
        return nextPos;
    }

    private Vector3 GetRandomPosition(bool isPlayerUnit)
    {
        Vector3 randomPos;
        Vector2Int gridPos;
        
        do
        {
            randomPos = new Vector3(
                Random.Range(-mapWidth/2, mapWidth/2),  // Cho phép di chuyển toàn bộ chiều rộng
                Random.Range(-mapHeight/2, mapHeight/2),
                0
            );
            gridPos = WorldToGrid(randomPos);
        } while (!IsValidPosition(gridPos, isPlayerUnit));

        MarkVisited(gridPos);
        return randomPos;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x + mapWidth/2) / gridSize),
            Mathf.FloorToInt((worldPos.y + mapHeight/2) / gridSize)
        );
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.x * gridSize) - mapWidth/2 + gridSize/2,
            (gridPos.y * gridSize) - mapHeight/2 + gridSize/2,
            0
        );
    }

    private bool IsValidPosition(Vector2Int gridPos, bool isPlayerUnit)
    {
        // Chỉ kiểm tra xem vị trí có nằm trong grid không
        return gridPos.x >= 0 && gridPos.x < gridWidth && 
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    private bool IsVisited(Vector2Int gridPos)
    {
        return visitedPositions[gridPos.x, gridPos.y];
    }

    private void MarkVisited(Vector2Int gridPos)
    {
        visitedPositions[gridPos.x, gridPos.y] = true;
    }

    private void ResetVisited()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                visitedPositions[x, y] = false;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGrid) return;

        // Vẽ các ô đã đi qua
        if (showVisitedPositions && visitedPositions != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (visitedPositions[x, y])
                    {
                        Vector3 pos = GridToWorld(new Vector2Int(x, y));
                        Gizmos.DrawCube(pos, Vector3.one * gridSize * 0.8f);
                    }
                }
            }
        }
    }

    // Thêm phương thức mới để lấy vị trí spawn ngẫu nhiên
    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPos;
        Vector2Int gridPos;
        
        do
        {
            randomPos = new Vector3(
                Random.Range(-mapWidth/2, mapWidth/2),   // Ngẫu nhiên toàn bộ chiều rộng
                Random.Range(-mapHeight/2, mapHeight/2),  // Ngẫu nhiên toàn bộ chiều cao
                0
            );
            gridPos = WorldToGrid(randomPos);
        } while (!IsValidPosition(gridPos, true));  // Chỉ kiểm tra có nằm trong map không

        return randomPos;
    }
} 
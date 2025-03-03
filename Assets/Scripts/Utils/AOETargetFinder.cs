using UnityEngine;
using System.Collections.Generic;

public class AOETargetFinder
{
    private const float DEFAULT_GRID_SIZE = 2f; // Kích thước mỗi ô lưới
    
    public struct AOESearchParams
    {
        public float searchWidth;      // Chiều rộng khu vực tìm kiếm
        public float searchHeight;     // Chiều cao khu vực tìm kiếm
        public float effectRadius;     // Bán kính hiệu ứng
        public float gridSize;         // Kích thước ô lưới (để tối ưu)
        public bool isPlayerTeam;      // Thuộc phe nào
        public System.Func<Unit, bool> customFilter; // Filter tùy chỉnh (optional)
    }

    public static Vector3 FindBestAOEPosition(AOESearchParams searchParams)
    {
        Vector3 bestPosition = Vector3.zero;
        float maxTargetCount = 0;
        float gridSize = searchParams.gridSize > 0 ? searchParams.gridSize : DEFAULT_GRID_SIZE;

        // Tính số lượng ô lưới
        int gridWidth = Mathf.CeilToInt(searchParams.searchWidth / gridSize);
        int gridHeight = Mathf.CeilToInt(searchParams.searchHeight / gridSize);

        // Cache danh sách unit để tránh gọi FindObjectsOfType nhiều lần
        var units = new List<Unit>(GameObject.FindObjectsOfType<Unit>());
        var enemyUnits = units.FindAll(u => u != null && u.IsPlayerUnit != searchParams.isPlayerTeam);

        if (enemyUnits.Count == 0) return Vector3.zero;

        // Tối ưu: Chỉ kiểm tra các vị trí có unit
        var checkPositions = new HashSet<Vector2>();
        
        // Thêm vị trí của các unit và các điểm xung quanh
        foreach (var unit in enemyUnits)
        {
            Vector2 unitPos = unit.transform.position;
            for (float x = -searchParams.effectRadius; x <= searchParams.effectRadius; x += gridSize)
            {
                for (float y = -searchParams.effectRadius; y <= searchParams.effectRadius; y += gridSize)
                {
                    Vector2 checkPos = unitPos + new Vector2(x, y);
                    
                    // Kiểm tra giới hạn map
                    if (Mathf.Abs(checkPos.x) <= searchParams.searchWidth/2 && 
                        Mathf.Abs(checkPos.y) <= searchParams.searchHeight/2)
                    {
                        checkPositions.Add(checkPos);
                    }
                }
            }
        }

        // Kiểm tra từng vị trí tiềm năng
        foreach (Vector2 checkPos in checkPositions)
        {
            int targetCount = CountTargetsAtPosition(
                checkPos,
                searchParams.effectRadius,
                enemyUnits,
                searchParams.customFilter
            );

            if (targetCount > maxTargetCount)
            {
                maxTargetCount = targetCount;
                bestPosition = new Vector3(checkPos.x, checkPos.y, 0);
            }
        }

        return bestPosition;
    }

    private static int CountTargetsAtPosition(
        Vector2 center,
        float radius,
        List<Unit> enemyUnits,
        System.Func<Unit, bool> customFilter = null)
    {
        int count = 0;
        float sqrRadius = radius * radius;

        foreach (var unit in enemyUnits)
        {
            if (Vector2.SqrMagnitude(center - (Vector2)unit.transform.position) <= sqrRadius)
            {
                if (customFilter == null || customFilter(unit))
                {
                    count++;
                }
            }
        }

        return count;
    }
} 
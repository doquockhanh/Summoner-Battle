using System;
using System.Collections.Generic;
using UnityEngine;

public class PathTester : MonoBehaviour
{
    // Các ô start và target được thiết lập trong Inspector.
    public Vector2 startCell;
    public Vector2 targetCell;

    // Dùng để kiểm tra sự thay đổi.
    private Vector2 prevStartCell;
    private Vector2 prevTargetCell;
    private HexGrid hexGrid;
    private HexPathFinder pathFinder;

    void Start()
    {
        hexGrid = HexGrid.Instance;
        pathFinder = new HexPathFinder(hexGrid);
    }

    void Update()
    {
        // Nếu start hoặc target thay đổi, thực thi kiểm thử
        if (startCell != prevStartCell || targetCell != prevTargetCell)
        {
            TestFindPath();
            prevStartCell = startCell;
            prevTargetCell = targetCell;
        }
    }

    /// <summary>
    /// Gọi hàm FindPath của grid và log kết quả.
    /// </summary>
    void TestFindPath()
    {
        if (hexGrid == null)
        {
            Debug.LogWarning("Grid chưa được thiết lập.");
            return;
        }
        if (startCell == null || targetCell == null)
        {
            Debug.LogWarning("StartCell hoặc TargetCell chưa được thiết lập.");
            return;
        }

        HexCoord coord = new HexCoord((int)(startCell.x), (int) startCell.y);
        HexCoord coord2 = new HexCoord((int)(targetCell.x), (int) targetCell.y);
        List<HexCell> path = pathFinder.FindPath(HexGrid.Instance.GetCell(coord), HexGrid.Instance.GetCell(coord2));
        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi từ " + startCell + " đến " + targetCell);
            return;
        }

        string pathStr = "";
        foreach (HexCell cell in path)
        {
            pathStr += cell.Coordinates.ToString() + " ";
        }
        Debug.Log("Đường đi từ " + startCell + " đến " + targetCell + " : " + pathStr);
    }
}

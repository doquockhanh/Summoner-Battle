using UnityEngine;

public class HexCell
{
    public HexCoord Coordinates { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; set; }
    public Unit OccupyingUnit { get; set; }

    public HexCell(HexCoord coordinates)
    {
        Coordinates = coordinates;
        WorldPosition = HexMetrics.HexToWorld(coordinates);
        IsOccupied = false;
        OccupyingUnit = null;
    }

    public void SetUnit(Unit unit)
    {
        OccupyingUnit = unit;
        IsOccupied = unit != null;
    }
}
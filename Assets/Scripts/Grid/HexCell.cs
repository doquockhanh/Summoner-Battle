using UnityEngine;

public class HexCell
{
    public HexCoord Coordinates { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Unit OccupyingUnit { get; private set; }
    public Unit RegisteredUnit { get; private set; }
    
    public bool IsOccupied => OccupyingUnit != null;
    public bool IsRegistered => RegisteredUnit != null;
    public bool IsAvailable => !IsOccupied && !IsRegistered;

    public HexCell(HexCoord coordinates)
    {
        Coordinates = coordinates;
        WorldPosition = HexMetrics.HexToWorld(coordinates);
    }

    public void SetUnit(Unit unit)
    {
        OccupyingUnit = unit;
        // Khi unit chiếm ô, hủy đăng ký
        if(unit != null && RegisteredUnit == unit)
        {
            RegisteredUnit = null;
        }
    }

    public bool RegisterUnit(Unit unit)
    {
        if(!IsAvailable) return false;
        RegisteredUnit = unit;
        return true;
    }

    public void UnregisterUnit()
    {
        RegisteredUnit = null;
    }
}
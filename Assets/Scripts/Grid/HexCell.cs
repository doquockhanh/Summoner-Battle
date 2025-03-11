using UnityEngine;
using System.Collections.Generic;

public class HexCell
{
    public HexCoordinates Coordinates { get; private set; }
    public bool IsWalkable { get; set; } = true;
    public HashSet<Unit> OccupyingUnits { get; private set; }

    public HexCell(HexCoordinates coordinates)
    {
        Coordinates = coordinates;
        OccupyingUnits = new HashSet<Unit>();
    }

    public void AddUnit(Unit unit)
    {
        OccupyingUnits.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        OccupyingUnits.Remove(unit);
    }

    public bool IsOccupied()
    {
        return OccupyingUnits.Count > 0;
    }
} 
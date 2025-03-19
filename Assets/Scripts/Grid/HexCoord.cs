using UnityEngine;

public struct HexCoord
{
    public readonly int q; // trục q (từ trái sang phải)
    public readonly int r; // trục r (từ trên xuống dưới)
    
    public static readonly HexCoord[] Directions = new HexCoord[]
    {
        new HexCoord(1, 0),    // Phải
        new HexCoord(1, -1),   // Phải trên
        new HexCoord(0, -1),   // Trên
        new HexCoord(-1, 0),   // Trái
        new HexCoord(-1, 1),   // Trái dưới
        new HexCoord(0, 1)     // Dưới
    };

    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    // Tính tọa độ s (được sử dụng trong một số tính toán)
    public int S => -q - r;

    // Cộng hai tọa độ hex
    public static HexCoord operator +(HexCoord a, HexCoord b)
        => new HexCoord(a.q + b.q, a.r + b.r);

    // Trừ hai tọa độ hex
    public static HexCoord operator -(HexCoord a, HexCoord b)
        => new HexCoord(a.q - b.q, a.r - b.r);

    // Tính khoảng cách giữa hai hex
    public int DistanceTo(HexCoord other)
    {
        var vec = this - other;
        return (Mathf.Abs(vec.q) + Mathf.Abs(vec.r) + Mathf.Abs(vec.S)) / 2;
    }

    // Lấy các hex lân cận
    public HexCoord[] GetNeighbors()
    {
        var neighbors = new HexCoord[6];
        for (int i = 0; i < 6; i++)
        {
            neighbors[i] = this + Directions[i];
        }
        return neighbors;
    }

    public override string ToString() => $"({q}, {r})";
}
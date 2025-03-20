using UnityEngine;

public struct HexCoord
{
    public int q { get; private set; } // trục q
    public int r { get; private set; } // trục r
    
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

    // Phép cộng hai tọa độ hex
    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q + b.q, a.r + b.r);
    }

    // Trừ hai tọa độ hex
    public static HexCoord operator -(HexCoord a, HexCoord b)
        => new HexCoord(a.q - b.q, a.r - b.r);

    // Tính khoảng cách Manhattan giữa hai tọa độ hex
    public int DistanceTo(HexCoord other)
    {
        // Trong hệ tọa độ offset, s = -q-r
        int s1 = -q - r;
        int s2 = -other.q - other.r;
        
        return (Mathf.Abs(q - other.q) + 
                Mathf.Abs(r - other.r) + 
                Mathf.Abs(s1 - s2)) / 2;
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

    public override bool Equals(object obj)
    {
        if (obj is HexCoord other)
        {
            return q == other.q && r == other.r;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return q.GetHashCode() ^ (r.GetHashCode() << 2);
    }
}
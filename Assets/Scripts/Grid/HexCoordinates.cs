using UnityEngine;

public struct HexCoordinates
{
    public readonly int Q; // Trục q trong hệ tọa độ axial
    public readonly int R; // Trục r trong hệ tọa độ axial
    
    // Tính S từ Q và R (q + r + s = 0 trong hệ tọa độ cube)
    public int S => -Q - R;

    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    // Chuyển đổi từ world position sang hex coordinates
    public static HexCoordinates FromPosition(Vector3 position, float hexSize)
    {
        float q = (Mathf.Sqrt(3)/3 * position.x - 1f/3 * position.y) / hexSize;
        float r = (2f/3 * position.y) / hexSize;
        
        return FromFloatCoordinates(q, r);
    }

    // Chuyển đổi từ hex coordinates sang world position
    public Vector3 ToPosition(float hexSize)
    {
        float x = (Mathf.Sqrt(3) * Q + Mathf.Sqrt(3)/2 * R) * hexSize;
        float y = (3f/2 * R) * hexSize;
        return new Vector3(x, y, 0);
    }

    // Làm tròn từ float coordinates sang int coordinates
    private static HexCoordinates FromFloatCoordinates(float q, float r)
    {
        int qi = Mathf.RoundToInt(q);
        int ri = Mathf.RoundToInt(r);
        int si = Mathf.RoundToInt(-q - r);

        float q_diff = Mathf.Abs(qi - q);
        float r_diff = Mathf.Abs(ri - r);
        float s_diff = Mathf.Abs(si - (-q - r));

        if (q_diff > r_diff && q_diff > s_diff)
            qi = -ri - si;
        else if (r_diff > s_diff)
            ri = -qi - si;

        return new HexCoordinates(qi, ri);
    }

    // Tính khoảng cách Manhattan giữa 2 hex
    public int DistanceTo(HexCoordinates other)
    {
        return Mathf.Max(Mathf.Abs(Q - other.Q), 
                        Mathf.Abs(R - other.R),
                        Mathf.Abs(S - other.S));
    }

    public override string ToString()
    {
        return $"({Q}, {R})";
    }
} 
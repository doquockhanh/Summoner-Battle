using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 0.7f;
    public const float gridToWorldRatio = outerRadius * 1.732f;
    public const float innerRadius = outerRadius * 0.866025404f; // sqrt(3)/2
    public const float squashFactor = 0.8f; // Hệ số co giãn theo chiều dọc

    // Các điểm góc của hex trong không gian 2D, bắt đầu từ góc phải
    public static readonly Vector2[] Corners = {
        new Vector2(outerRadius, 0f),
        new Vector2(0.5f * outerRadius, innerRadius * squashFactor),
        new Vector2(-0.5f * outerRadius, innerRadius * squashFactor),
        new Vector2(-outerRadius, 0f),
        new Vector2(-0.5f * outerRadius, -innerRadius * squashFactor),
        new Vector2(0.5f * outerRadius, -innerRadius * squashFactor)
    };

    // Chuyển đổi từ world position sang hex coordinates
    public static HexCoord WorldToHex(Vector3 position)
    {
        float q = (2f / 3f * position.x) / outerRadius;
        float r = (-1f / 3f * position.x + Mathf.Sqrt(3f) / 3f * position.y / squashFactor) / outerRadius;

        return RoundToHex(q, r);
    }

    // Chuyển đổi từ hex coordinates sang world position
    public static Vector3 HexToWorld(HexCoord hexCoord)
    {
        float x = (3f / 2f * hexCoord.q) * outerRadius;
        float y = (Mathf.Sqrt(3f) / 2f * hexCoord.q + Mathf.Sqrt(3f) * hexCoord.r) * outerRadius * squashFactor;
        return new Vector3(x, y, 0f); // Z luôn = 0 vì là game 2D
    }

    // Làm tròn tọa độ hex
    private static HexCoord RoundToHex(float q, float r)
    {
        float s = -q - r;

        int qi = Mathf.RoundToInt(q);
        int ri = Mathf.RoundToInt(r);
        int si = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(qi - q);
        float rDiff = Mathf.Abs(ri - r);
        float sDiff = Mathf.Abs(si - s);

        if (qDiff > rDiff && qDiff > sDiff)
        {
            qi = -ri - si;
        }
        else if (rDiff > sDiff)
        {
            ri = -qi - si;
        }

        return new HexCoord(qi, ri);
    }

    public static float GridToWorldRadius(int radius) {
        return radius * gridToWorldRatio;
    }
}
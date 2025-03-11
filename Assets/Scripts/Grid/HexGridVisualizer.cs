using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class HexGridVisualizer : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private bool showCoordinates = true;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    
    private void OnDrawGizmos()
    {
        if (hexGrid == null) return;

        float hexSize = hexGrid.HexSize;
        
        // Vẽ các ô hex
        Gizmos.color = gridColor;
        
        // Vẽ từng ô trong grid
        for (int q = -7; q < 7; q++)
        {
            for (int r = -14; r < 14; r++)
            {
                HexCoordinates coordinates = new HexCoordinates(q, r);
                if (hexGrid.IsValidCoordinates(coordinates))
                {
                    DrawHexagon(coordinates.ToPosition(hexSize), hexSize);
                    
                    if (showCoordinates)
                    {
                        Handles.Label(coordinates.ToPosition(hexSize), coordinates.ToString());
                    }
                }
            }
        }
    }

    private void DrawHexagon(Vector3 center, float size)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            corners[i] = center + new Vector3(
                size * Mathf.Cos(angle),
                size * Mathf.Sin(angle),
                0
            );
        }

        for (int i = 0; i < 6; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
        }
    }
}
#endif 
using UnityEngine;
using UnityEngine.UI;

public class HexGridUI : MonoBehaviour 
{
    [Header("Grid Settings")]
    [SerializeField] private GameObject hexTilePrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private float hexSize = 1f;
    [SerializeField] private Color playerSideColor = new Color(0.8f, 0.9f, 1f, 0.3f);
    [SerializeField] private Color enemySideColor = new Color(1f, 0.8f, 0.8f, 0.3f);
    [SerializeField] private Color neutralColor = new Color(0.9f, 0.9f, 0.9f, 0.2f);

    private HexCell[,] hexCells;

    private void Start()
    {
        CreateHexGrid();
    }

    private void CreateHexGrid()
    {
        float width = HexMetrics.outerRadius * 2f;
        float height = HexMetrics.innerRadius * 2f;
        
        // Tạo grid 14x28
        for (int r = 0; r < 28; r++)
        {
            int qStart = -r/2;
            int qEnd = 14 - r/2;
            
            for (int q = qStart; q < qEnd; q++)
            {
                CreateHexTile(new HexCoord(q, r));
            }
        }
    }

    private void CreateHexTile(HexCoord coord)
    {
        Vector3 position = HexMetrics.HexToWorld(coord);
        GameObject hexGO = Instantiate(hexTilePrefab, gridParent);
        
        // Đặt vị trí local trong UI
        RectTransform rectTransform = hexGO.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        
        // Set màu sắc dựa vào vị trí
        Image hexImage = hexGO.GetComponent<Image>();
        if (coord.q < -1) // Phe player
        {
            hexImage.color = playerSideColor;
        }
        else if (coord.q > 1) // Phe enemy
        {
            hexImage.color = enemySideColor;
        }
        else // Vùng trung lập
        {
            hexImage.color = neutralColor;
        }

        // Thêm component HexTileUI
        var hexTileUI = hexGO.AddComponent<HexTileUI>();
        hexTileUI.Initialize(coord);
    }
} 
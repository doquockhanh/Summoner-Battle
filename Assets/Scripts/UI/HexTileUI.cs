using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HexTileUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private HexCoord coordinates;
    private Image hexImage;
    private Color originalColor;

    public void Initialize(HexCoord coord)
    {
        coordinates = coord;
        hexImage = GetComponent<Image>();
        originalColor = hexImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight khi hover
        hexImage.color = new Color(
            originalColor.r + 0.2f,
            originalColor.g + 0.2f,
            originalColor.b + 0.2f,
            originalColor.a
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Trả về màu gốc
        hexImage.color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Xử lý click event
        HexCell cell = HexGrid.Instance.GetCell(coordinates);
        if (cell != null)
        {
            Debug.Log($"Clicked hex at {coordinates}");
            // Thêm logic xử lý click tùy theo game của bạn
        }
    }
} 
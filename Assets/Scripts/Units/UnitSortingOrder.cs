using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitSortingOrder : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private const string SORTING_Y_LAYER = "Sorting Y";
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sortingLayerName == SORTING_Y_LAYER)
        {
            UpdateSortingOrder();
        }
    }
    
    private void Update()
    {
        if (spriteRenderer.sortingLayerName == SORTING_Y_LAYER)
        {
            UpdateSortingOrder();
        }
    }
    
    private void UpdateSortingOrder()
    {
        // Chuyển đổi vị trí Y thành order in layer
        // Nhân với -100 để có độ chi tiết cao hơn và đảo ngược (y càng cao thì order càng thấp)
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }
} 
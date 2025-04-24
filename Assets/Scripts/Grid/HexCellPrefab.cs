using UnityEngine;

public class HexCellPrefab : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Thiết lập các thuộc tính mặc định cho SpriteRenderer
        spriteRenderer.sortingOrder = 0; // Có thể điều chỉnh để kiểm soát thứ tự vẽ
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
} 
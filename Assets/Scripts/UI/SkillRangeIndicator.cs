using UnityEngine;

public class SkillRangeIndicator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetRadius(float radius)
    {
        transform.localScale = Vector3.one * (radius * 2);
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            color.a = 0.3f; // Độ trong suốt
            spriteRenderer.color = color;
        }
    }
} 
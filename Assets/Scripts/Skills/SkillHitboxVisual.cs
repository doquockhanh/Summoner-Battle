using UnityEngine;

public class SkillHitboxVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer circleRenderer;
    [SerializeField] private Color aoeColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color singleTargetColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color buffColor = new Color(0f, 0f, 1f, 0.3f);
    
    private void Awake()
    {
        if (circleRenderer == null)
        {
            circleRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    public void ShowHitbox(TargetType targetType, float radius, Vector3 position)
    {
        transform.position = position;
        transform.localScale = new Vector3(radius * 2, radius * 2, 1); // *2 vì sprite là đường kính 1 unit
        
        switch (targetType)
        {
            case TargetType.AOE:
                circleRenderer.color = aoeColor;
                break;
            case TargetType.SingleTarget:
                circleRenderer.color = singleTargetColor;
                break;
            case TargetType.Ally:
            case TargetType.AllAllies:
                circleRenderer.color = buffColor;
                break;
        }
        
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
} 
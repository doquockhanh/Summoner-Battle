using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float lifeTime = 1f;
    
    private float currentLifeTime;
    private Color textColor;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Tự động tìm component TextMeshProUGUI
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textMesh == null)
        {
            Debug.LogError("Không tìm thấy TextMeshProUGUI trong FloatingText!");
        }
    }
    
    public void Initialize(string text, Color color)
    {
        if (textMesh == null)
        {
            Debug.LogError("TextMeshProUGUI chưa được thiết lập trong FloatingText!");
            return;
        }
        
        textMesh.text = text;
        textMesh.color = color;
        textColor = color;
        currentLifeTime = lifeTime;
        
        // Thiết lập kích thước và vị trí ban đầu
        rectTransform.sizeDelta = new Vector2(100f, 30f);
    }
    
    private void Update()
    {
        // Di chuyển text lên trên trong world space
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        // Giảm dần độ trong suốt
        currentLifeTime -= Time.deltaTime;
        // float alpha = currentLifeTime / lifeTime;
        // textColor.a = alpha;
        textMesh.color = textColor;
        
        if (currentLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
} 
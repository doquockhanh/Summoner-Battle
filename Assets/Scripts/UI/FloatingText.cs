using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float outLineWidth = 0.3f;
    [SerializeField] private Color outlineColor = Color.black;

    private float currentLifeTime;
    private Color textColor;
    private RectTransform rectTransform;
    private Vector3 bounceDirection;
    
    private float verticalVelocity;
    private float bounceForce;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh == null)
        {
            Debug.LogError("Không tìm thấy TextMeshProUGUI trong FloatingText!");
        }
        textMesh.outlineWidth = outLineWidth; // Độ dày viền
        textMesh.outlineColor = outlineColor; // Màu viền
    }

    public void Initialize(string text, Color color, Vector3 bounceDir, float force)
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

        // Thiết lập hướng và vận tốc nảy ban đầu
        bounceDirection = bounceDir.normalized;
        bounceForce = force;
        verticalVelocity = bounceForce;

        // Thiết lập kích thước và vị trí ban đầu
    }

    private void Update()
    {
        // Áp dụng chuyển động nảy
        verticalVelocity -= gravity * Time.deltaTime;
        Vector3 movement = bounceDirection * bounceForce * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;
        transform.position += movement;

        // Giảm dần độ trong suốt
        currentLifeTime -= Time.deltaTime;
        float alpha = currentLifeTime / (lifeTime / 3);
        textColor.a = alpha;
        textMesh.color = textColor;

        if (currentLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
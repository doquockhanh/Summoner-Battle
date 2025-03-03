using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }
    
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    
    [Header("Bounce Settings")]
    [SerializeField] [Range(-90f, 90f)] public float minBounceAngle = -30f;
    [SerializeField] [Range(-90f, 90f)] public float maxBounceAngle = 30f;
    [SerializeField] [Range(0f, 100f)] public float bounceForce = 1f;
    [SerializeField] [Range(0f, 100f)] public float scale = 1f;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        // Tạo canvas nếu chưa có
        if (worldSpaceCanvas == null)
        {
            GameObject canvasObj = new GameObject("Floating Text Canvas");
            worldSpaceCanvas = canvasObj.AddComponent<Canvas>();
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            
            // Thêm canvas scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;
            
            worldSpaceCanvas.worldCamera = Camera.main;
        }
    }
    
    public void ShowFloatingText(string text, Vector3 position, Color color)
    {
        // Áp dụng offset khi tạo floating text
        Vector3 spawnPosition = position + spawnOffset;
        
        // Tạo góc ngẫu nhiên trong khoảng minBounceAngle đến maxBounceAngle
        float randomAngle = Random.Range(minBounceAngle, maxBounceAngle);
        // Tạo vector hướng nảy lên dựa trên góc ngẫu nhiên
        Vector3 bounceDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.up;
        
        GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity, worldSpaceCanvas.transform);
        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        
        // Khởi tạo với hướng nảy và các thông số khác, truyền thêm bounceForce
        floatingText.Initialize(text, color, bounceDirection, bounceForce);
        
        // Điều chỉnh scale để text hiển thị đúng kích thước
        textObj.transform.localScale = Vector3.one * scale / 1000f;
    }
} 
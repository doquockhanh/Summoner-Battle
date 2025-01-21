using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }
    
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    
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
        GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity, worldSpaceCanvas.transform);
        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        floatingText.Initialize(text, color);
        
        // Điều chỉnh scale để text hiển thị đúng kích thước
        textObj.transform.localScale = Vector3.one * 0.01f;
    }
} 
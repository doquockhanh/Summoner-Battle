using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    private static HealthBarManager instance;
    public static HealthBarManager Instance => instance;
    
    [SerializeField] private Canvas worldSpaceCanvas;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SetupCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupCanvas()
    {
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        worldSpaceCanvas.worldCamera = Camera.main;
    }
    
    public Transform GetCanvasTransform()
    {
        return worldSpaceCanvas.transform;
    }
    
    public Canvas GetCanvas()
    {
        return worldSpaceCanvas;
    }
} 
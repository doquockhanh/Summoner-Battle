using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;      // Component điều khiển việc cuộn
    [SerializeField] private float scrollSpeed = 0.1f;   // Tốc độ cuộn
} 
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothness = 5f;
    [SerializeField] private float dragSpeed = 20f; // Tăng tốc độ kéo camera

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Giới hạn")]
    [SerializeField] private HomeData homeData;
    [Tooltip("use when homedata is null")]
    [SerializeField] private int boundaryX;
    [Tooltip("use when homedata is null")]
    [SerializeField] private int boundaryY;

    private Vector3 targetPosition;
    private float targetZoom;
    private Camera mainCamera;

    private Vector3 dragOrigin; // Vị trí bắt đầu kéo
    private bool isDragging = false; // Trạng thái đang kéo

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetPosition = transform.position;
        targetZoom = mainCamera.orthographicSize;

        // Đặt camera ở giữa map
        if (HomeManager.Instance == null)
        {
            targetPosition = new Vector3(0, 0, -10);
        }
        else
        {
            Vector3 camPos = HomeManager.Instance.GetCamPos();
            targetPosition = new Vector3(camPos.x, camPos.y, -10);
        }

        transform.position = targetPosition;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleDrag();
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        // Xử lý input
        if (Input.GetKey(KeyCode.W)) movement.y += 1;
        if (Input.GetKey(KeyCode.S)) movement.y -= 1;
        if (Input.GetKey(KeyCode.A)) movement.x -= 1;
        if (Input.GetKey(KeyCode.D)) movement.x += 1;

        // Tính toán vị trí mới
        targetPosition += movement * moveSpeed * Time.deltaTime;

        if (homeData == null)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, -boundaryX, boundaryX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -boundaryY, boundaryY);
        }
        else
        {
            // Giới hạn vị trí
            targetPosition.x = Mathf.Clamp(targetPosition.x, -homeData.cameraBoundaryX, homeData.cameraBoundaryX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -homeData.cameraBoundaryY, homeData.cameraBoundaryY);
        }

        // Di chuyển mượt
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothness);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed, minZoom, maxZoom);
        }

        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetZoom,
            Time.deltaTime * smoothness
        );
    }

    private void HandleDrag()
    {
        // Bắt đầu kéo khi nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
        }

        // Kết thúc kéo khi thả chuột trái
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Xử lý kéo camera
        if (isDragging)
        {
            Vector3 difference = dragOrigin - Input.mousePosition;
            difference = difference * dragSpeed * Time.deltaTime;
            
            // Chuyển đổi từ screen space sang world space và nhân thêm hệ số để tăng tốc độ
            Vector3 worldDifference = mainCamera.ScreenToWorldPoint(difference) - mainCamera.ScreenToWorldPoint(Vector3.zero);
            worldDifference *= 2f; // Nhân thêm hệ số để tăng tốc độ di chuyển
            
            // Cập nhật vị trí đích
            targetPosition += worldDifference;
            
            // Cập nhật vị trí bắt đầu kéo
            dragOrigin = Input.mousePosition;
        }
    }

    /// <summary>
    /// Di chuyển camera đến vị trí targetPosition một cách mượt mà
    /// </summary>
    /// <param name="targetPosition">Vị trí cần focus</param>
    public void FocusOnPosition(Vector3 targetPosition)
    {
        // Giữ nguyên giá trị z của camera
        targetPosition.z = this.targetPosition.z;
        
        // Giới hạn vị trí theo boundary
        if (homeData == null)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, -boundaryX, boundaryX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -boundaryY, boundaryY);
        }
        else
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, -homeData.cameraBoundaryX, homeData.cameraBoundaryX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -homeData.cameraBoundaryY, homeData.cameraBoundaryY);
        }

        this.targetPosition = targetPosition;
    }
}
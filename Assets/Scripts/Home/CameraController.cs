using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothness = 5f;
    
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;
    
    [Header("Giới hạn")]
    [SerializeField] private HomeData homeData;

    private Vector3 targetPosition;
    private float targetZoom;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetPosition = transform.position;
        targetZoom = mainCamera.orthographicSize;

        // Đặt camera ở giữa map
        Vector3 camPos = HomeManager.Instance.GetCamPos();
        targetPosition = new Vector3(camPos.x, camPos.y, -10);
        transform.position = targetPosition;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
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
        
        // Giới hạn vị trí
        targetPosition.x = Mathf.Clamp(targetPosition.x, -homeData.cameraBoundaryX, homeData.cameraBoundaryX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -homeData.cameraBoundaryY, homeData.cameraBoundaryY);

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
} 
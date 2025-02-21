using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    public static UnitSelector Instance { get; private set; }
    
    [SerializeField] private GameObject selectionCirclePrefab;
    [SerializeField] private UnitStatsPanel statsPanel;
    [SerializeField] private LayerMask unitLayer; // Layer chứa các Unit
    
    private Unit selectedUnit;
    private GameObject selectionCircle;
    private Camera mainCamera;
    
    // Thêm biến xử lý double click
    private float lastClickTime;
    private Unit lastClickedUnit;
    private const float doubleClickTime = 0.3f; // Thời gian giữa 2 lần click để tính là double click
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
            
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleUnitSelection();
        }
        
        // Kiểm tra Unit được chọn còn tồn tại không
        if (selectedUnit != null && selectedUnit.IsDead)
        {
            ClearSelection();
            statsPanel.Hide();
        }
    }
    
    private void HandleUnitSelection()
    {
        Unit clickedUnit = GetClickedUnit();

        if (clickedUnit != null)
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            
            if (clickedUnit == lastClickedUnit && timeSinceLastClick <= doubleClickTime)
            {
                // Double click - hiển thị stats panel
                statsPanel.ShowStats(clickedUnit);
            }
            else
            {
                // Single click - chỉ xử lý selection và targeting
                HandleUnitClick(clickedUnit);
            }
            
            lastClickTime = Time.time;
            lastClickedUnit = clickedUnit;
        }
        else
        {
            // Click ra ngoài
            ClearSelection();
            statsPanel.Hide();
            lastClickedUnit = null;
        }
    }

    private Unit GetClickedUnit()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, unitLayer);
        
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Unit>();
        }
        
        return null;
    }

    private void HandleUnitClick(Unit clickedUnit)
    {
        if (selectedUnit != null && selectedUnit.IsPlayerUnit)
        {
            if (!clickedUnit.IsPlayerUnit)
            {
                // Nếu đang chọn unit phe ta và click vào unit địch
                selectedUnit.GetComponent<UnitTargeting>().SetTarget(clickedUnit);
            }
            else
            {
                // Click vào unit phe ta khác
                SelectUnit(clickedUnit);
            }
        }
        else
        {
            // Chưa có unit nào được chọn hoặc unit được chọn không phải phe ta
            SelectUnit(clickedUnit);
        }
    }
    
    private void SelectUnit(Unit unit)
    {
        ClearSelection();
        
        if (unit.IsPlayerUnit)
        {
            selectedUnit = unit;
            CreateSelectionCircle(unit);
        }
    }

    private void CreateSelectionCircle(Unit unit)
    {
        if (selectionCirclePrefab != null)
        {
            selectionCircle = Instantiate(selectionCirclePrefab, unit.transform.position, Quaternion.identity);
            selectionCircle.transform.SetParent(unit.transform);
            selectionCircle.transform.localPosition = new Vector3(0, 1f, 0);
        }
    }
    
    private void ClearSelection()
    {
        if (selectionCircle != null)
        {
            Destroy(selectionCircle);
            selectionCircle = null;
        }
        selectedUnit = null;
    }

    // Phương thức public để components khác có thể kiểm tra unit đang được chọn
    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
} 
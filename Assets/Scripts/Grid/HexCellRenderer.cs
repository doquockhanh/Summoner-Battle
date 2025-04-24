using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class HexCellRenderer : MonoBehaviour
{
    [System.Serializable]
    public class CellSpriteSet
    {
        public Sprite[] sprites; // Mảng các sprite có thể sử dụng cho cell
        [Range(0f, 1f)]
        public float weight = 1f; // Trọng số xuất hiện của set này
    }

    [Header("Sprite Settings")]
    [SerializeField] private List<CellSpriteSet> spriteSets = new List<CellSpriteSet>();
    [SerializeField] private GameObject cellPrefab; // Prefab chứa SpriteRenderer
    [SerializeField] private Transform cellsContainer; // Transform chứa tất cả các cell

    // Sử dụng ReadOnlyDictionary để tránh thay đổi không mong muốn từ bên ngoài
    private Dictionary<HexCoord, SpriteRenderer> cellRenderers;
    private ObjectPool<GameObject> cellPool;
    private bool isInitialized = false;

    private void Awake()
    {
        cellRenderers = new Dictionary<HexCoord, SpriteRenderer>();
        InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        if (cellPrefab == null) return;

        // Tạo pool với kích thước dự đoán dựa trên grid
        int estimatedSize = HexGrid.Instance != null ? 
            HexGrid.Instance.Width * HexGrid.Instance.Height : 100;

        cellPool = new ObjectPool<GameObject>(
            createFunc: () => {
                var obj = Instantiate(cellPrefab, cellsContainer);
                obj.SetActive(false);
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: estimatedSize
        );
    }

    private void Start()
    {
        if (HexGrid.Instance != null && !isInitialized)
        {
            InitializeCells();
            isInitialized = true;
        }
    }

    public void InitializeCells()
    {
        if (isInitialized)
        {
            ClearCells();
        }

        if (HexGrid.Instance == null) return;

        foreach (var cell in HexGrid.Instance.GetAllCells())
        {
            CreateCellRenderer(cell);
        }

        isInitialized = true;
    }

    private void CreateCellRenderer(HexCell cell)
    {
        if (cellPrefab == null || cellsContainer == null) return;

        // Lấy object từ pool thay vì tạo mới
        GameObject cellObj = cellPool.Get();
        cellObj.transform.position = cell.WorldPosition;

        SpriteRenderer renderer = cellObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Sprite selectedSprite = GetRandomSprite();
            if (selectedSprite != null)
            {
                renderer.sprite = selectedSprite;
            }
            cellRenderers[cell.Coordinates] = renderer;
        }
    }

    private Sprite GetRandomSprite()
    {
        if (spriteSets == null || spriteSets.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var set in spriteSets)
        {
            totalWeight += set.weight;
        }

        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var set in spriteSets)
        {
            currentWeight += set.weight;
            if (random <= currentWeight && set.sprites != null && set.sprites.Length > 0)
            {
                return set.sprites[Random.Range(0, set.sprites.Length)];
            }
        }

        return null;
    }

    public void UpdateCellSprite(HexCoord coord, Sprite newSprite)
    {
        if (cellRenderers.TryGetValue(coord, out SpriteRenderer renderer))
        {
            renderer.sprite = newSprite;
        }
    }

    public void ClearCells()
    {
        foreach (var renderer in cellRenderers.Values)
        {
            if (renderer != null)
            {
                // Trả object về pool thay vì destroy
                cellPool.Release(renderer.gameObject);
            }
        }
        cellRenderers.Clear();
    }

    // Phương thức để thay đổi sprite set trong runtime
    public void SetSpriteSets(List<CellSpriteSet> newSets)
    {
        spriteSets = newSets;
        InitializeCells(); // Vẽ lại toàn bộ grid với sprite set mới
    }

    private void OnDestroy()
    {
        ClearCells();
        cellPool?.Clear();
    }
}

// Object Pool để tái sử dụng GameObject
public class ObjectPool<T>
{
    private readonly System.Func<T> createFunc;
    private readonly System.Action<T> actionOnGet;
    private readonly System.Action<T> actionOnRelease;
    private readonly System.Action<T> actionOnDestroy;
    private readonly int maxSize;
    private readonly Stack<T> pool;

    public ObjectPool(
        System.Func<T> createFunc,
        System.Action<T> actionOnGet = null,
        System.Action<T> actionOnRelease = null,
        System.Action<T> actionOnDestroy = null,
        int defaultCapacity = 10,
        int maxSize = 10000)
    {
        this.createFunc = createFunc;
        this.actionOnGet = actionOnGet;
        this.actionOnRelease = actionOnRelease;
        this.actionOnDestroy = actionOnDestroy;
        this.maxSize = maxSize;
        this.pool = new Stack<T>(defaultCapacity);
    }

    public T Get()
    {
        T item = pool.Count > 0 ? pool.Pop() : createFunc();
        actionOnGet?.Invoke(item);
        return item;
    }

    public void Release(T item)
    {
        if (pool.Count < maxSize)
        {
            actionOnRelease?.Invoke(item);
            pool.Push(item);
        }
        else
        {
            actionOnDestroy?.Invoke(item);
        }
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            actionOnDestroy?.Invoke(pool.Pop());
        }
    }
} 
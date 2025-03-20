using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Map Settings")]
    [SerializeField] private float mapWidth = 20f;
    [SerializeField] private float mapHeight = 10f;

    // Property để các class khác có thể truy cập
    public float MapWidth => mapWidth;
    public float MapHeight => mapHeight;

    [SerializeField] private Transform playerSpawnStart;
    [SerializeField] private Transform playerSpawnEnd;
    [SerializeField] private Transform enemySpawnStart;
    [SerializeField] private Transform enemySpawnEnd;
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private Transform enemyCardContainer;
    [Header("Card Prefabs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float playerBaseHP = 1000f;
    [SerializeField] private float enemyBaseHP = 1000f;

    [Header("Player Cards")]
    [SerializeField] private List<Card> playerCards;
    [SerializeField] private List<Card> enemyCards;
    public bool spawnOnce = false;

    private List<CardController> activeCards = new List<CardController>();
    private float currentPlayerHP;
    private float currentEnemyHP;

    [SerializeField] private UnitPoolManager unitPoolManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Đảm bảo có SkillEffectHandler
        if (SkillEffectHandler.Instance == null)
        {
            GameObject skillEffectHandler = new GameObject("SkillEffectHandler");
            skillEffectHandler.AddComponent<SkillEffectHandler>();
        }

        // Đảm bảo có MaterialManager
        if (MaterialManager.Instance == null)
        {
            GameObject materialManager = new GameObject("MaterialManager");
            var manager = materialManager.AddComponent<MaterialManager>();
            // Gán material từ Resources nếu cần
            manager.Initialize();
        }
    }

    private void Start()
    {
        // Khởi tạo pools trước khi bắt đầu trận đấu
        unitPoolManager.InitializePools(playerCards, enemyCards);
        StartBattle(playerCards, enemyCards);
        currentPlayerHP = playerBaseHP;
        currentEnemyHP = enemyBaseHP;
    }

    public void StartBattle(List<Card> playerCards, List<Card> enemyCards)
    {
        SpawnPlayerCards();
        SpawnEnemyCards();
    }

    private void SpawnPlayerCards()
    {
        foreach (Card card in playerCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, playerCardContainer);
            CardController controller = cardObj.GetComponent<CardController>();
            // Khởi tạo card với skill
            controller.Initialize(card, true);

            if (spawnOnce)
            {
                controller.spawnCooldown = 1000f;
            }
        }
    }

    private void SpawnEnemyCards()
    {
        foreach (Card card in enemyCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, enemyCardContainer);
            CardController controller = cardObj.GetComponent<CardController>();
            controller.Initialize(card, false);

            if (spawnOnce)
            {
                controller.spawnCooldown = 1000f;
            }
        }
    }

    public Vector3 GetSpawnPosition(bool isPlayer)
    {
        HexGrid grid = HexGrid.Instance;
        if (isPlayer)
        {
            // Spawn bên trái (q nhỏ)
            int r = Random.Range(0, grid.Height);
            int q = Random.Range(0, grid.Width / 4 - 1);
            HexCoord coord = new HexCoord(r, q);
            HexCell cell = HexGrid.Instance.GetCell(coord);
            return cell.WorldPosition;
        }
        else
        {
            // Spawn bên phải (q lớn)
            int r = Random.Range(0, grid.Height);
            int q = Random.Range(3 * grid.Width / 4, grid.Width - 1);
            var coord = new HexCoord(q, r);
            HexCell cell = HexGrid.Instance.GetCell(coord);
            return cell.WorldPosition;
        }
    }

    public void EndGame(bool playerWon)
    {
        // Hiển thị màn hình victory/defeat
        Debug.Log(playerWon ? "Player Won!" : "Enemy Won!");
    }

    private void OnDrawGizmos()
    {
        if (playerSpawnStart != null && playerSpawnEnd != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerSpawnStart.position, playerSpawnEnd.position);
            Gizmos.DrawSphere(playerSpawnStart.position, 0.2f);
            Gizmos.DrawSphere(playerSpawnEnd.position, 0.2f);
        }

        if (enemySpawnStart != null && enemySpawnEnd != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(enemySpawnStart.position, enemySpawnEnd.position);
            Gizmos.DrawSphere(enemySpawnStart.position, 0.2f);
            Gizmos.DrawSphere(enemySpawnEnd.position, 0.2f);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Player Cards")]
    [SerializeField] private List<Card> playerCards;
    [SerializeField] private List<Vector2> playerSpawnPositions;
    [SerializeField] private List<Card> enemyCards;
    [SerializeField] private List<Vector2> enemiesSpawnPositions;
    public bool spawnOnce = false;

    private List<CardController> activeCards = new List<CardController>();
    public List<CardController> ActiveCards => activeCards;

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
        PrepareData();
        unitPoolManager.InitializePools(playerCards, enemyCards);
        StartBattle();
    }

    private void PrepareData()
    {
        BattleDataManager battleDataManager = BattleDataManager.Instance;
        if (battleDataManager == null) return;
        
        if (battleDataManager.attackerIDs.Count >= 0)
        {
            List<Card> attackerCards = battleDataManager.GetAttackerCards();
            if (attackerCards.Count > 0)
            {
                playerCards = attackerCards;
            }
        }

        if (battleDataManager.defenderIDs.Count >= 0)
        {
            List<Card> defenderCards = battleDataManager.GetDefenderCards();
            if (defenderCards.Count > 0)
            {
                enemyCards = defenderCards;
            }
        }
    }

    public void StartBattle()
    {
        SpawnCards(playerCards, playerSpawnPositions, true);
        SpawnCards(enemyCards, enemiesSpawnPositions, false);
    }

    private void SpawnCards(List<Card> card, List<Vector2> spawnPoss, bool isPlayer)
    {
        for (int i = 0; i < card.Count; i++)
        {
            HexCoord toCoord = new HexCoord((int)spawnPoss[i].x, (int)spawnPoss[i].y);
            HexCell hexCell = HexGrid.Instance.GetCell(toCoord);
            Vector3 spawnPos = hexCell.WorldPosition;
            GameObject cardObj = Instantiate(card[i].SummonerPrefab, spawnPos, Quaternion.identity);
            CardController controller = cardObj.GetComponent<CardController>();

            // chiếm hexcell ở hexgrid
            if (HexGrid.Instance.OccupyCell(hexCell, controller))
            {
                controller.occupiedHex = hexCell;
            }
            else
            {
                Debug.LogError("Summoner ko thể chiếm ô hiện tại, xin kiểm tra lại");
            }


            // Khởi tạo card với skill
            controller.Initialize(card[i], isPlayer);
            activeCards.Add(controller);

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
            int q = Random.Range(0, grid.Width / 4 - 1);
            int r = Random.Range(0 - q / 2, grid.Height - q / 2);
            HexCoord coord = new HexCoord(q, r);
            HexCell cell = HexGrid.Instance.GetCell(coord);
            return cell.WorldPosition;
        }
        else
        {
            // Spawn bên phải (q lớn)
            int q = Random.Range(3 * grid.Width / 4, grid.Width - 1);
            int r = Random.Range(0 - q / 2, grid.Height - q / 2);
            HexCoord coord = new HexCoord(q, r);
            HexCell cell = HexGrid.Instance.GetCell(coord);
            return cell.WorldPosition;
        }
    }

    public void EndGame(bool playerWon)
    {
        // Hiển thị màn hình victory/defeat
        Debug.Log(playerWon ? "Player Won!" : "Enemy Won!");
    }

    public void RemoveFromActiveCards(CardController card)
    {
        if (activeCards.Contains(card))
        {
            activeCards.Remove(card);
        }
    }

    public List<Unit> GetAllUnits()
    {
        return activeCards.SelectMany(card => card.GetActiveUnits()).ToList();
    }

    public List<Unit> GetAllUnitInteam(bool isPlayer)
    {
        List<Unit> units = activeCards
             .Where(c => c.IsPlayer == isPlayer)
             .SelectMany(c => c.GetActiveUnits())
             .ToList();

        return units;
    }
}

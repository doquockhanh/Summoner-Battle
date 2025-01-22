using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    
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
    
    private List<Card> playerDeck = new List<Card>();
    private List<Card> enemyDeck = new List<Card>();
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
        playerDeck = playerCards;
        enemyDeck = enemyCards;
        
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
            
            // Skill sẽ tự động được setup thông qua CardController
        }
    }
    
    private void SpawnEnemyCards()
    {
        foreach (Card card in enemyDeck)
        {
            GameObject cardObj = Instantiate(cardPrefab, enemyCardContainer);
            CardController controller = cardObj.GetComponent<CardController>();
            controller.Initialize(card, false);
        }
    }
    
    public Vector3 GetSpawnPosition(bool isPlayer)
    {
        if (isPlayer)
        {
            return GetRandomPositionOnLine(playerSpawnStart.position, playerSpawnEnd.position);
        }
        else
        {
            return GetRandomPositionOnLine(enemySpawnStart.position, enemySpawnEnd.position);
        }
    }

    private Vector3 GetRandomPositionOnLine(Vector3 start, Vector3 end)
    {
        float randomT = Random.Range(0f, 1f);
        return Vector3.Lerp(start, end, randomT);
    }

    public void DamageBase(float damage, bool isPlayer)
    {
        if (isPlayer)
            currentPlayerHP -= damage;
        else
            currentEnemyHP -= damage;

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (currentPlayerHP <= 0)
            EndGame(false);
        else if (currentEnemyHP <= 0)
            EndGame(true);
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

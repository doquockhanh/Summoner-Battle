using System.Collections.Generic;
using UnityEngine;

public class BattleStatsManager : MonoBehaviour
{
    public static BattleStatsManager Instance { get; private set; }

    [System.Serializable]
    public class CardBattleStats
    {
        public string cardId;
        public CardController cardController;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public List<UnitBattleStats> unitStats = new List<UnitBattleStats>();
    }

    [System.Serializable]
    public class UnitBattleStats
    {
        public string unitId;
        public Unit unit;
        public float totalDamageDealt;
        public float totalDamageTaken;
    }

    private Dictionary<string, CardBattleStats> cardStatsDict = new Dictionary<string, CardBattleStats>();
    private Dictionary<Unit, UnitBattleStats> unitStatsDict = new Dictionary<Unit, UnitBattleStats>();

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
    }

    // Đăng ký card khi bắt đầu trận
    public void RegisterCard(string cardId, CardController cardController)
    {
        if (!cardStatsDict.ContainsKey(cardId))
        {
            var stats = new CardBattleStats
            {
                cardId = cardId,
                cardController = cardController,
                totalDamageDealt = 0,
                totalDamageTaken = 0
            };
            cardStatsDict[cardId] = stats;
        }
    }

    // Đăng ký unit khi được triệu hồi
    public void RegisterUnit(Unit unit, string unitId, CardController ownerCard)
    {
        if (!unitStatsDict.ContainsKey(unit))
        {
            var stats = new UnitBattleStats
            {
                unitId = unitId,
                unit = unit,
                totalDamageDealt = 0,
                totalDamageTaken = 0
            };
            unitStatsDict[unit] = stats;
            // Gắn vào card chủ quản
            if (ownerCard != null && cardStatsDict.TryGetValue(ownerCard.name, out var cardStats))
            {
                cardStats.unitStats.Add(stats);
            }
        }
    }

    // Ghi nhận damage gây ra cho card
    public void AddDamageDealtToCard(string cardId, float amount)
    {
        if (cardStatsDict.TryGetValue(cardId, out var stats))
        {
            stats.totalDamageDealt += amount;
        }
    }

    // Ghi nhận damage nhận vào cho card
    public void AddDamageTakenToCard(string cardId, float amount)
    {
        if (cardStatsDict.TryGetValue(cardId, out var stats))
        {
            stats.totalDamageTaken += amount;
        }
    }

    // Ghi nhận damage gây ra cho unit
    public void AddDamageDealtToUnit(Unit unit, float amount)
    {
        if (unitStatsDict.TryGetValue(unit, out var stats))
        {
            stats.totalDamageDealt += amount;
        }
    }

    // Ghi nhận damage nhận vào cho unit
    public void AddDamageTakenToUnit(Unit unit, float amount)
    {
        if (unitStatsDict.TryGetValue(unit, out var stats))
        {
            stats.totalDamageTaken += amount;
        }
    }

    // Lấy tổng hợp chỉ số cho UI
    public List<CardBattleStats> GetAllCardStats()
    {
        return new List<CardBattleStats>(cardStatsDict.Values);
    }

    public void ResetStats()
    {
        cardStatsDict.Clear();
        unitStatsDict.Clear();
    }
} 
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
        public float totalDamageReduced;
        public float totalHealShield;
    }

    private Dictionary<string, CardBattleStats> cardStatsDict = new Dictionary<string, CardBattleStats>();

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

    private void OnEnable()
    {
        UnitEvents.Combat.OnTakeRawDamage += HandleDamage;
        UnitEvents.Combat.OnDamageReduced += HandleDamageReduced;
        UnitEvents.Combat.OnShieldDamage += HandleHealAndShield;
        UnitEvents.Combat.OnHealing += HandleHealAndShield;
    }
    private void OnDisable()
    {
        UnitEvents.Combat.OnTakeRawDamage -= HandleDamage;
        UnitEvents.Combat.OnDamageReduced -= HandleDamageReduced;
        UnitEvents.Combat.OnShieldDamage -= HandleHealAndShield;
        UnitEvents.Combat.OnHealing -= HandleHealAndShield;
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

    // Lắng nghe event gây/nhận damage
    private void HandleDamage(Unit source, Unit target, float amount)
    {
        if (source != null && source.OwnerCard != null && source.OwnerCard.CardData != null)
        {
            var cardId = source.OwnerCard.CardData.id;
            if (cardStatsDict.TryGetValue(cardId, out var cardStats))
            {
                cardStats.totalDamageDealt += amount;
            }
        }
        if (target != null && target.OwnerCard != null && target.OwnerCard.CardData != null)
        {
            var cardId = target.OwnerCard.CardData.id;
            if (cardStatsDict.TryGetValue(cardId, out var cardStats))
            {
                cardStats.totalDamageTaken += amount;
            }
        }
    }

    private void HandleDamageReduced(Unit source, float amount)
    {
        if (source != null && source.OwnerCard != null && source.OwnerCard.CardData != null)
        {
            var cardId = source.OwnerCard.CardData.id;
            if (cardStatsDict.TryGetValue(cardId, out var cardStats))
            {
                cardStats.totalDamageReduced += amount;
            }
        }
    }


    private void HandleHealAndShield(Unit source, float amount)
    {
        if (source != null && source.OwnerCard != null && source.OwnerCard.CardData != null)
        {
            var cardId = source.OwnerCard.CardData.id;
            if (cardStatsDict.TryGetValue(cardId, out var cardStats))
            {
                cardStats.totalHealShield += amount;
            }
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
    }
}
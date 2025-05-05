using TMPro;
using UnityEngine;

public class CardStatItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI damageDealtText;
    [SerializeField] private TextMeshProUGUI damageTakenText;

    public void Set(BattleStatsManager.CardBattleStats stats)
    {
        cardNameText.text = stats.cardController != null ? stats.cardController.name : stats.cardId;
        damageDealtText.text = $"Gây ra: {stats.totalDamageDealt}";
        damageTakenText.text = $"Nhận vào: {stats.totalDamageTaken}";
    }
} 
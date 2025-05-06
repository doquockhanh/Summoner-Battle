using TMPro;
using UnityEngine;

public class CardStatItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI damageDealtText;
    [SerializeField] private TextMeshProUGUI damageTakenText;
    [SerializeField] private TextMeshProUGUI damageReducedText;
    [SerializeField] private TextMeshProUGUI healAndShieldText;

    public void Set(BattleStatsManager.CardBattleStats stats)
    {
        cardNameText.text = stats.cardId;
        damageDealtText.text = $"{stats.totalDamageDealt:F0}";
        damageTakenText.text = $"{stats.totalDamageTaken:F0}";
        damageReducedText.text = $"{stats.totalDamageReduced:F0}";
        healAndShieldText.text = $"{stats.totalHealShield:F0}";
    }
}
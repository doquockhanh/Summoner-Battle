using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInventoryView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    
    public void Setup(Card card)
    {
        cardImage.sprite = card.cardImage;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        
        // Hiển thị thông tin stats của card
        statsText.text = $"Mana: {card.maxMana}\n" +
                        $"Hồi mana/giây: {card.manaRegen}\n" +
                        $"Hồi mana từ sát thương gây ra: {card.manaGainFromDamageDealt}%\n" +
                        $"Hồi mana từ sát thương nhận: {card.manaGainFromDamageTaken}%\n" +
                        $"Thời gian hồi chiêu: {card.spawnCooldown}s";
    }
} 
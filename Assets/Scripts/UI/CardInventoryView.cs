using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInventoryView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsTextMana;
    [SerializeField] private TextMeshProUGUI statsTextDamge;
    
    public void Setup(Card card)
    {
        cardImage.sprite = card.cardImage;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        
        // Hiển thị thông tin stats của card
        statsTextMana.text = $"{card.maxMana}";
        statsTextDamge.text =   $"{card.summonUnit.physicalDamage}";            
    }
} 
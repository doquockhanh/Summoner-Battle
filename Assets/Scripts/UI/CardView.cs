using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Image manaBar;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public void Setup(Card card, CardController controller)
    {
        cardImage.sprite = card.cardImage;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        
        UpdateUI(0, 1);
    }
    
    public void UpdateUI(float manaPercent, float cooldownPercent)
    {
        manaBar.fillAmount = manaPercent;
    }
} 
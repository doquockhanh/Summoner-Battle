using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image manaBar;
    [SerializeField] private SkillUI skillUI;
    
    private CardController controller;
    
    public void Setup(Card cardData, CardController cardController)
    {
        controller = cardController;
        
        cardImage.sprite = cardData.cardImage;
        cardNameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        
        if (skillUI != null && cardData.skill != null)
        {
            skillUI.Setup(cardData.skill);
        }
    }
    
    public void UpdateUI(float manaPercent, float cooldownPercent)
    {
        manaBar.fillAmount = manaPercent;
            
        if (skillUI != null)
        {
            skillUI.UpdateUI(manaPercent);
        }
    }
} 
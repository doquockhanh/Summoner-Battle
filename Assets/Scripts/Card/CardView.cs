using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image rageBar;
    [SerializeField] private Image spawnCooldownBar;
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
    
    public void UpdateUI(float ragePercent, float cooldownPercent)
    {
        rageBar.fillAmount = ragePercent;
        spawnCooldownBar.fillAmount = cooldownPercent;
        
        if (skillUI != null)
        {
            skillUI.UpdateUI(cooldownPercent, ragePercent);
        }
    }
} 
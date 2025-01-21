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
    
    private CardController controller;
    
    public void Setup(Card cardData, CardController cardController)
    {
        controller = cardController;
        
        cardImage.sprite = cardData.cardImage;
        cardNameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
    }
    
    public void UpdateUI(float ragePercent, float cooldownPercent)
    {
        rageBar.fillAmount = ragePercent;
        spawnCooldownBar.fillAmount = cooldownPercent;
    }
} 
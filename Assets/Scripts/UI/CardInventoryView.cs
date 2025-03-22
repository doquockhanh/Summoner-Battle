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
    //[SerializeField] private TextMeshProUGUI statsTextDamge;
    
    private Card cardData;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnCardClick);
    }
    
    public void Setup(Card card)
    {
        cardData = card;
        cardImage.sprite = card.cardImage;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        
        // Hiển thị thông tin stats của card
        statsTextMana.text = $"{card.maxMana}";
        //statsTextDamge.text =   $"{card.summonUnit.physicalDamage}";            
    }
    
    private void OnCardClick()
    {
        CardDetailPanel.Instance.Show(cardData);
    }
} 
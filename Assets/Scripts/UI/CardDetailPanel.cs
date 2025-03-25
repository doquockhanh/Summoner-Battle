using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailPanel : MonoBehaviour
{
    public static CardDetailPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Main Info")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Stats Container")]
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statItemPrefab;
    
    [Header("Stat Icons")]
    [SerializeField] private Sprite manaIcon;
    [SerializeField] private Sprite manaRegenIcon;
    [SerializeField] private Sprite cooldownIcon;
    [SerializeField] private Sprite damageIcon;
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite armorIcon;
    [SerializeField] private Sprite speedIcon;
    
    [Header("Skill Info")]
    [SerializeField] private GameObject skillContainer;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private Image skillIcon;
    [SerializeField] private CardInventoryUI cardInventoryUI;
    
    [Header("UI Controls")]
    [SerializeField] private GameObject closeButton;
    
    private void Start()
    {
        gameObject.SetActive(false);
        closeButton.SetActive(false);
    }
    
    public void Show(Card card)
    {
        gameObject.SetActive(true);
        closeButton.SetActive(true);  
        cardInventoryUI.gameObject.SetActive(false); 
        // Hiển thị thông tin cơ bản
        cardImage.sprite = card.cardImage;
        cardNameText.text = card.cardName;
        descriptionText.text = card.description;
        
        // Xóa stats cũ
        foreach (Transform child in statsContainer) {
            Destroy(child.gameObject);
        }
        
        // Hiển thị các stats có giá trị khác mặc định
        DisplayStatIfNotDefault("Mana tối đa", card.maxMana, 100, manaIcon);
        DisplayStatIfNotDefault("Hồi mana/giây", card.manaRegen, 1, manaRegenIcon);
        DisplayStatIfNotDefault("Thời gian hồi chiêu", card.spawnCooldown, 0, cooldownIcon, "s");
        
        if (card.summonUnit != null)
        {
            DisplayStatIfNotDefault("Physical Damage", card.summonUnit.physicalDamage, 0, damageIcon);
            DisplayStatIfNotDefault("Máu", card.summonUnit.maxHp, 0, healthIcon);
            DisplayStatIfNotDefault("Giáp", card.summonUnit.armor, 0, armorIcon);
            DisplayStatIfNotDefault("Tốc độ di chuyển", card.summonUnit.moveSpeed, 0, speedIcon);
        }
        
        // Hiển thị thông tin skill
        if (card.skill != null)
        {
            skillContainer.SetActive(true);
            skillNameText.text = card.skill.skillName;
            skillDescriptionText.text = card.skill.description;
            if (card.skill.skillIcon != null)
                skillIcon.sprite = card.skill.skillIcon;
        }
        else
        {
            skillContainer.SetActive(false);
        }
    }
    
    private void DisplayStatIfNotDefault(string statName, float value, float defaultValue, Sprite icon, string suffix = "")
    {
        if (value != defaultValue)
        {
            GameObject statItem = Instantiate(statItemPrefab, statsContainer);
            StatItemView statView = statItem.GetComponent<StatItemView>();
            statView.Setup(statName, value.ToString() + suffix, icon);
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        closeButton.SetActive(false); 
        cardInventoryUI.gameObject.SetActive(true);   
    }
} 
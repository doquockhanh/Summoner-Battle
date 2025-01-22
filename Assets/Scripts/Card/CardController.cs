using UnityEngine;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private CardView cardView;
    [SerializeField] private SkillController skillController;
    
    private Card cardData;
    private float currentMana;
    private float currentRage;
    private float spawnTimer;
    private bool isPlayer;
    
    public void Initialize(Card card, bool isPlayer = true)
    {
        this.isPlayer = isPlayer;
        cardData = card;
        currentMana = 0; // Bắt đầu với 0 mana
        currentRage = 0;
        spawnTimer = 0;
        
        cardView.Setup(cardData, this);
        
        if (skillController != null && cardData.skill != null)
        {
            Debug.Log($"[Card] {cardData.cardName} được khởi tạo với kỹ năng {cardData.skill.skillName}");
            skillController.Initialize(cardData, isPlayer);
        }
        else
        {
            Debug.LogWarning($"[Card] {cardData.cardName} thiếu SkillController hoặc Skill Data!");
        }
    }
    
    private void Start()
    {
        spawnTimer = cardData.spawnCooldown;
        SpawnUnit();
    }
    
    private void Update()
    {
        // Hồi mana theo thời gian
        currentMana += cardData.manaRegen * Time.deltaTime;
        currentMana = Mathf.Min(currentMana, cardData.maxMana);
        
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnUnit();
            }
        }
        
        currentRage += cardData.manaRegen * Time.deltaTime;
        currentRage = Mathf.Min(currentRage, 100f);
        
        if (skillController != null && cardData.skill != null)
        {
            if (currentMana >= cardData.skill.manaCost)
            {
                skillController.UseSkill();
                currentMana -= cardData.skill.manaCost;
            }
        }
        
        cardView.UpdateUI(currentMana/cardData.maxMana, spawnTimer/cardData.spawnCooldown);
    }
    
    private void SpawnUnit()
    {
        if (cardData.summonUnit == null)
        {
            Debug.LogError("Summon Unit Data is not assigned!");
            return;
        }

        Vector3 spawnPos = BattleManager.Instance.GetSpawnPosition(isPlayer);
        
        Unit unit = UnitPoolManager.Instance.GetUnit(cardData.summonUnit, isPlayer, this);
        unit.transform.position = spawnPos;
        unit.Initialize(cardData.summonUnit, isPlayer, this);
        
        spawnTimer = cardData.spawnCooldown;
    }
    
    // Thêm phương thức để nhận mana từ sát thương
    public void GainManaFromDamage(float damage, bool isDamageTaken)
    {
        float manaGain;
        if (isDamageTaken)
        {
            manaGain = damage * cardData.manaGainFromDamageTaken;
        }
        else
        {
            manaGain = damage * cardData.manaGainFromDamageDealt;
        }
        
        currentMana += manaGain;
        currentMana = Mathf.Min(currentMana, cardData.maxMana);
    }
}

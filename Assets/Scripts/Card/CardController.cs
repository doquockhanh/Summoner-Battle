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
        currentMana = cardData.mana;
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
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnUnit();
            }
        }
        
        currentRage += cardData.rageGainRate * Time.deltaTime;
        currentRage = Mathf.Min(currentRage, 100f);
        
        if (skillController != null && cardData.skill != null)
        {
            if (skillController.CanUseSkill(currentRage))
            {
                Debug.Log($"[Card] {cardData.cardName} đủ rage ({currentRage}) để dùng kỹ năng!");
                skillController.UseSkill();
                currentRage -= cardData.skill.rageCost;
                Debug.Log($"[Card] Rage còn lại: {currentRage}");
            }
        }
        
        cardView.UpdateUI(currentRage/100f, spawnTimer/cardData.spawnCooldown);
    }
    
    private void SpawnUnit()
    {
        if (cardData.summonUnit == null)
        {
            Debug.LogError("Summon Unit Data is not assigned!");
            return;
        }

        Vector3 spawnPos = BattleManager.Instance.GetSpawnPosition(isPlayer);
        
        // Lấy unit từ pool thay vì Instantiate
        Unit unit = UnitPoolManager.Instance.GetUnit(cardData.summonUnit, isPlayer);
        if (unit != null)
        {
            unit.transform.position = spawnPos;
            unit.transform.SetParent(null);
        }
        
        spawnTimer = cardData.spawnCooldown;
    }
}

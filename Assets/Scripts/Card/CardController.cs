using UnityEngine;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private CardView cardView;
    
    private Card cardData;
    private float currentMana;
    private float spawnTimer;
    private bool isPlayer;
    public bool IsPlayer => isPlayer;
    
    private bool isWaitingForUnit = false;
    private bool canActivateSkill = true;
    public UnitData Unit => cardData.summonUnit;
    
    public void Initialize(Card card, bool isPlayer = true)
    {
        this.isPlayer = isPlayer;
        cardData = card;
        currentMana = 0;
        spawnTimer = 0;
        isWaitingForUnit = false;
        canActivateSkill = true;
        
        if (cardData != null && cardData.skill != null)
        {
            cardData.skill.ownerCard = this;
        }
        
        cardView.Setup(cardData, this);
    }
    
    private void Start()
    {
        spawnTimer = cardData.spawnCooldown;
        SpawnUnit();
    }
    
    private void Update()
    {
        currentMana += cardData.manaRegen * Time.deltaTime;
        currentMana = Mathf.Min(currentMana, cardData.maxMana);
        
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnUnit();
                
                if (isWaitingForUnit)
                {
                    TryActivateSkill();
                }
            }
        }
        
        if (canActivateSkill && cardData.skill != null && cardData.skill.CanActivate(currentMana))
        {
            TryActivateSkill();
        }
        
        cardView.UpdateUI(currentMana/cardData.maxMana, spawnTimer/cardData.spawnCooldown);
    }
    
    private void TryActivateSkill()
    {
        if (cardData == null || cardData.skill == null) return;
        
        if (cardData.skill.CanActivate(currentMana))
        {
            cardData.skill.ownerCard = this;
            
            if (cardData.skill.skillType == SkillType.OnSummon)
            {
                cardData.skill.ApplyToSummon(null);
            }
            else
            {
                cardData.skill.ApplyToUnit(null);
            }
        }
    }
    
    public void OnSkillActivated()
    {
        currentMana -= cardData.skill.manaCost;
        isWaitingForUnit = false;
        
        FloatingTextManager.Instance.ShowFloatingText(
            "Kích hoạt " + cardData.skill.skillName,
            transform.position,
            Color.cyan
        );
    }
    
    public void OnSkillFailed()
    {
        isWaitingForUnit = true;
        
        FloatingTextManager.Instance.ShowFloatingText(
            "Chờ unit để kích hoạt kỹ năng",
            transform.position,
            Color.yellow
        );
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
        if (unit.OwnerCard.cardData.skill is BloodstormSkill bloodstormSkill)
        {
            bloodstormSkill.ApplyToSummon(unit);
        }
        
        spawnTimer = cardData.spawnCooldown;
        
        if (isWaitingForUnit)
        {
            TryActivateSkill();
        }
    }
}

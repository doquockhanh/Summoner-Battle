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
    
    public void Initialize(Card card, bool isPlayer = true)
    {
        this.isPlayer = isPlayer;
        cardData = card;
        currentMana = 0;
        spawnTimer = 0;
        
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
    
    public void GainManaFromDamage(float damage, float targetMaxHealth, bool isDamageTaken)
    {
        if(damage <= 0) return;
        float healthPercentage = damage / targetMaxHealth;
        float manaGain;
        
        if (isDamageTaken)
        {
            manaGain = healthPercentage * cardData.manaGainFromDamageTaken;
        }
        else
        {
            manaGain = healthPercentage * cardData.manaGainFromDamageDealt;
        }
        currentMana += manaGain;
        currentMana = Mathf.Min(currentMana, cardData.maxMana);
    }
}

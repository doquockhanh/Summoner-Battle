using UnityEngine;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private CardView cardView;
    
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
        
        cardView.UpdateUI(currentRage/100f, spawnTimer/cardData.spawnCooldown);
    }
    
    private void SpawnUnit()
    {
        Debug.Log($"Spawning unit at position: {BattleManager.Instance.GetSpawnPosition(isPlayer)}");
        if (unitPrefab == null)
        {
            Debug.LogError("Unit Prefab is not assigned!");
            return;
        }

        Vector3 spawnPos = BattleManager.Instance.GetSpawnPosition(isPlayer);
        GameObject unitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        
        unitObj.transform.SetParent(null);
        
        Unit unit = unitObj.GetComponent<Unit>();
        if (unit != null)
        {
            unit.Initialize(cardData.summonUnit, isPlayer);
        }
        
        spawnTimer = cardData.spawnCooldown;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class UnitPoolManager : MonoBehaviour
{
    private const int UNITS_PER_TYPE = 5;
    
    public static UnitPoolManager Instance { get; private set; }
    
    // Dictionary lưu trữ pool cho từng loại unit
    private Dictionary<string, Queue<Unit>> poolDictionary = new Dictionary<string, Queue<Unit>>();
    private Dictionary<string, GameObject> unitPrefabs = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void InitializePools(List<Card> playerCards, List<Card> enemyCards)
    {
        // Tạo pool cho units của player
        foreach (Card card in playerCards)
        {
            CreatePool(card.summonUnit);
        }
        
        // Tạo pool cho units của enemy
        foreach (Card card in enemyCards)
        {
            CreatePool(card.summonUnit);
        }
    }
    
    private void CreatePool(UnitData unitData)
    {
        string unitId = unitData.name;
        
        // Kiểm tra nếu pool đã tồn tại
        if (poolDictionary.ContainsKey(unitId))
            return;
            
        // Tạo parent object để organize hierarchy
        GameObject poolParent = new GameObject($"Pool_{unitId}");
        poolParent.transform.SetParent(transform);
        
        Queue<Unit> pool = new Queue<Unit>();
        GameObject unitPrefab = unitData.unitPrefab;
        unitPrefabs[unitId] = unitPrefab;
        
        // Tạo sẵn các unit và đưa vào pool
        for (int i = 0; i < UNITS_PER_TYPE; i++)
        {
            GameObject obj = Instantiate(unitPrefab, poolParent.transform);
            Unit unit = obj.GetComponent<Unit>();
            obj.SetActive(false);
            pool.Enqueue(unit);
        }
        
        poolDictionary[unitId] = pool;
    }
    
    public Unit GetUnit(UnitData unitData, bool isPlayer)
    {
        string unitId = unitData.name;
        
        if (!poolDictionary.ContainsKey(unitId))
        {
            Debug.LogWarning($"Pool for unit {unitId} doesn't exist!");
            return null;
        }
        
        Queue<Unit> pool = poolDictionary[unitId];
        
        // Nếu pool hết unit, tạo thêm unit mới
        if (pool.Count == 0)
        {
            GameObject poolParent = transform.Find($"Pool_{unitId}")?.gameObject;
            GameObject obj = Instantiate(unitPrefabs[unitId], poolParent.transform);
            Unit unit = obj.GetComponent<Unit>();
            unit.Initialize(unitData, isPlayer);
            return unit;
        }
        
        // Lấy unit từ pool
        Unit pooledUnit = pool.Dequeue();
        pooledUnit.gameObject.SetActive(true);
        pooledUnit.Initialize(unitData, isPlayer);
        
        return pooledUnit;
    }
    
    public void ReturnToPool(Unit unit)
    {
        string unitId = unit.GetUnitData().name;
        
        if (!poolDictionary.ContainsKey(unitId))
        {
            Debug.LogWarning($"Pool for unit {unitId} doesn't exist!");
            return;
        }
        
        unit.gameObject.SetActive(false);
        poolDictionary[unitId].Enqueue(unit);
    }
} 
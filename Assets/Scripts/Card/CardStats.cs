using UnityEngine;
using System.Collections.Generic;

public class CardStats : MonoBehaviour, IStats
{
    [SerializeField] private UnitData data; // Tạm thời dùng chung UnitData
    
    private float currentHp;
    private Dictionary<StatType, StatModifier> modifiers = new Dictionary<StatType, StatModifier>();

    private void Awake()
    {
        InitializeModifiers();
        currentHp = GetMaxHp();
    }

    private void InitializeModifiers()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            modifiers[type] = new StatModifier();
        }
    }

    public void TakeDamage(float amount, DamageType damageType)
    {
        float finalDamage = StatsCalculator.CalculateFinalDamage(amount, damageType, this);
        currentHp = Mathf.Max(0, currentHp - finalDamage);
    }

    // Implement IStats interface
    public float GetMaxHp() => modifiers[StatType.MaxHp].Calculate(data.maxHp);
    public float GetCurrentHp() => currentHp;
    public float GetPhysicalDamage() => modifiers[StatType.PhysicalDamage].Calculate(data.physicalDamage);

    float IStats.GetMaxHp()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetCurrentHp()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetPhysicalDamage()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetMagicDamage()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetArmor()
    {
        Debug.Log("auwdhaiwd");
        throw new System.NotImplementedException();
    }

    float IStats.GetMagicResist()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetMoveSpeed()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetAttackSpeed()
    {
        throw new System.NotImplementedException();
    }

    int IStats.GetRange()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetLifeSteal()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetDamageReduction()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetHealingReceived()
    {
        throw new System.NotImplementedException();
    }

    float IStats.GetTotalShield()
    {
        throw new System.NotImplementedException();
    }
    // ... implement other getters ...
} 
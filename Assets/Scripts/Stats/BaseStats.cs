using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseStats : MonoBehaviour, IStats
{
    protected Dictionary<StatType, StatModifier> modifiers = new Dictionary<StatType, StatModifier>();
    protected float currentHp;
    protected float currentShield;
    protected List<ShieldLayer> shieldLayers = new List<ShieldLayer>();

    protected virtual void Awake()
    {
        InitializeModifiers();
    }

    void FixedUpdate()
    {
        for(int i = shieldLayers.Count - 1; i >= 0; i--) {
            shieldLayers[i].UpdateDuration(Time.fixedDeltaTime);
            if(shieldLayers[i].IsExpired) {
                shieldLayers.RemoveAt(i);
                OnShieldChanged?.Invoke(GetTotalShield());
            }
        }
    }

    protected void InitializeModifiers()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            modifiers[type] = new StatModifier();
        }
    }

    protected void ResetModifiers()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            modifiers[type].Reset();
        }
    }

    public virtual void ModifyStat(StatType type, float flatBonus, float percentBonus = 0)
    {
        if (flatBonus != 0) modifiers[type].AddFlat(flatBonus);
        if (percentBonus != 0) modifiers[type].AddPercent(percentBonus);
    }

    protected float GetModifiedStat(StatType type, float baseValue)
    {
        return modifiers[type].Calculate(baseValue);
    }

    protected float GetModifiedPercentStat(StatType type, float baseValue)
    {
        return modifiers[type].CalculateForPercentStat(baseValue);
    }

    public virtual float CalculateFinalDamage(float rawDamage, DamageType damageType)
    {
        return StatsCalculator.CalculateFinalDamage(rawDamage, damageType, this);
    }

    protected virtual float ProcessShieldDamage(float damage)
    {
        if (shieldLayers.Count == 0) return damage;

        float remainingDamage = damage;
        var orderedShields = shieldLayers
            .Where(s => s.RemainingValue > 0)
            .OrderBy(s => s.Duration)
            .ToList();

        foreach (var shield in orderedShields)
        {
            if (remainingDamage <= 0) break;
            remainingDamage = shield.AbsorbDamage(remainingDamage);
        }

        OnShieldChanged?.Invoke(GetTotalShield());
        return remainingDamage;
    }

    public virtual void AddShield(ShieldLayer shield)
    {
        shieldLayers.Add(shield);
        OnShieldChanged?.Invoke(GetTotalShield());
    }

     public void AddShield(float amount, float duration)
    {
        var shield = new ShieldLayer(amount, duration, GetComponent<Unit>());
        AddShield(shield);
    }

    public float GetTotalShield() => shieldLayers.Sum(s => s.RemainingValue);

    // Events
    public event System.Action<float> OnShieldChanged;

    // IStats implementation
    public abstract float GetMaxHp();
    public abstract float GetCurrentHp();
    public abstract float GetPhysicalDamage();
    public abstract float GetMagicDamage();
    public abstract float GetArmor();
    public abstract float GetMagicResist();
    public abstract float GetMoveSpeed();
    public abstract float GetAttackSpeed();
    public abstract int GetRange();
    public abstract float GetLifeSteal();
    public abstract float GetDamageReduction();
    public abstract float GetHealingReceived();
} 
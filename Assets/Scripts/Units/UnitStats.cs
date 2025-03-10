using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    private UnitData data;

    // Current Stats
    private float currentHp;
    private float currentShield;

    // Stat Modifiers
    private StatModifier physicalDamageModifier = new StatModifier();
    private StatModifier magicDamageModifier = new StatModifier();
    private StatModifier armorModifier = new StatModifier();
    private StatModifier magicResistModifier = new StatModifier();
    private StatModifier speedModifier = new StatModifier();
    private StatModifier healingReceivedModifier = new StatModifier();
    private StatModifier lifeStealModifier = new StatModifier();
    private StatModifier damageReductionModifier = new StatModifier();
    private StatModifier attackSpeedModifier = new StatModifier();
    private StatModifier maxHpModifier = new StatModifier();

    // Cached calculated values
    private float cachedPhysicalDamage;
    private float cachedMagicDamage;
    private bool isPhysicalDamageModified;
    private bool isMagicDamageModified;

    private float hpRegenTimer;
    private const float HP_REGEN_INTERVAL = 2f;

    private List<ShieldLayer> shieldLayers = new List<ShieldLayer>();

    // Properties
    public float CurrentHP => currentHp;
    public bool IsDead => currentHp <= 0;
    public float CurrentHealthPercent => currentHp / GetMaxHp();
    public UnitData Data => data;

    // Events
    public event System.Action<float> OnHealthChanged;
    public event System.Action<float> OnShieldChanged;
    public event System.Action<float, Unit> OnTakeDamage;
    public event System.Action<Unit> OnTakeLethalityDamage;
    public static event System.Func<float, Unit, Unit, DamageType, float> OnModifyRawDamage;
    public event System.Action OnDeath;

    private void Awake()
    {
    }

    public void Initialize(UnitData unitData)
    {
        data = unitData;
        currentHp = unitData.maxHp;
        currentShield = 0;
        ResetModifiers();
    }

    public void TakeDamage(float rawDamage, DamageType damageType, Unit source = null)
    {
        if (IsDead) return;

        rawDamage = OnModifyRawDamage?.Invoke(rawDamage, source, GetComponent<Unit>(), damageType)?? rawDamage;

        float finalDamage = CalculateFinalDamage(rawDamage, damageType);

        // Xử lý shield trước
        float remainingDamage = ProcessShieldDamage(finalDamage);

        // Xử lý HP
        if (remainingDamage > 0)
        {
            float damageApplied = ProcessHealthDamage(remainingDamage, damageType);
            OnTakeDamage?.Invoke(damageApplied, source);

            // Sửa lại phần xử lý hút máu
            if (source != null)
            {
                UnitStats sourceStats = source.GetComponent<UnitStats>();
                if (sourceStats.GetLifesteal() > 0)
                {
                    float lifestealAmount = sourceStats.CalculateLifestealAmount(damageApplied);
                    if (lifestealAmount > 0)
                    {
                        sourceStats.Heal(lifestealAmount);
                    }
                }
            }

            CheckDeath();
        }
    }

    public float CalculateFinalDamage(float rawDamage, DamageType damageType)
    {
        float damage = rawDamage;

        // Áp dụng giáp/kháng phép
        if (damageType == DamageType.Physical)
        {
            float armor = GetArmor();
            damage *= 100f / (100f + armor);
        }
        else
        {
            float magicResist = GetMagicResist();
            damage *= 100f / (100f + magicResist);
        }

        // Áp dụng giảm sát thương chung
        damage = CalculateDamageAfterReduction(damage);
        return damage;
    }

    private float ProcessShieldDamage(float damage)
    {
        if (shieldLayers.Count == 0) return damage;

        float remainingDamage = damage;

        // Sắp xếp shield theo thời gian (shield ngắn hạn trước)
        var orderedShields = shieldLayers
            .Where(s => s.RemainingValue > 0)
            .OrderBy(s => s.Duration)
            .ToList();

        foreach (var shield in orderedShields)
        {
            if (remainingDamage <= 0) break;
            remainingDamage = shield.AbsorbDamage(remainingDamage);
        }

        // Cập nhật UI
        OnShieldChanged?.Invoke(GetTotalShield());

        // Hiển thị số sát thương được hấp thụ
        float absorbedDamage = damage - remainingDamage;
        if (absorbedDamage > 0)
        {
            FloatingTextManager.Instance.ShowFloatingText(
                absorbedDamage.ToString("F0"),
                transform.position,
                Color.white
            );
        }

        return remainingDamage;
    }

    private float ProcessHealthDamage(float damage, DamageType damageType)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        if (currentHp <= 0)
        {
            OnTakeLethalityDamage?.Invoke(GetComponent<Unit>());
        }
        FloatingTextManager.Instance.ShowFloatingText(
          damage.ToString("F0"),
          transform.position,
          damageType == DamageType.Physical ? Color.red : Color.blue
       );
        OnHealthChanged?.Invoke(currentHp);
        return Mathf.Min(damage, currentHp);
    }

    public void Heal(float amount, Unit source = null)
    {
        if (IsDead) return;

        float healingMultiplier = healingReceivedModifier.Calculate(data.healingReceivedPercent / 100);
        float finalHealing = amount * healingMultiplier;

        if (finalHealing >= 1)
        {
            FloatingTextManager.Instance.ShowFloatingText(
                        finalHealing.ToString("F0"),
                        transform.position,
                        Color.green
                    );
        }


        currentHp = Mathf.Min(data.maxHp, currentHp + finalHealing);
        OnHealthChanged?.Invoke(currentHp);
    }

    public void AddShield(float amount, float duration)
    {
        var shield = new ShieldLayer(amount, duration, GetComponent<Unit>());
        AddShield(shield);
    }

    public void AddShield(ShieldLayer shield)
    {
        shieldLayers.Add(shield);
        OnShieldChanged?.Invoke(GetTotalShield());
    }

    private float GetTotalShield()
    {
        return shieldLayers.Sum(s => s.RemainingValue);
    }

    private void CheckDeath()
    {
        if (currentHp <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void SetCurrentHp(float hp)
    {
        currentHp = hp;
        OnHealthChanged?.Invoke(currentHp);
    }

    #region Stat Getters
    public float GetPhysicalDamage()
    {
        if (!isPhysicalDamageModified)
        {
            cachedPhysicalDamage = physicalDamageModifier.Calculate(data.physicalDamage);
            isPhysicalDamageModified = true;
        }
        return cachedPhysicalDamage;
    }

    public float GetMagicDamage()
    {
        if (!isMagicDamageModified)
        {
            cachedMagicDamage = magicDamageModifier.Calculate(data.magicDamage);
            isMagicDamageModified = true;
        }
        return cachedMagicDamage;
    }
    public float GetArmor() => armorModifier.Calculate(data.armor);
    public float GetMagicResist() => magicResistModifier.Calculate(data.magicResist);
    public float GetMoveSpeed() => speedModifier.Calculate(data.moveSpeed);
    public float GetAttackSpeed() => attackSpeedModifier.Calculate(data.attackSpeed);
    public float GetRange() => data.range;
    public float GetDetectRange() => data.detectRange;
    public float GetLifesteal() => lifeStealModifier.Calculate(data.lifestealPercent);
    public float GetCriticalChance() => data.criticalChance;
    public float GetCriticalDamage() => data.criticalDamage;
    public float GetArmorPenetration() => data.armorPenetration;
    public float GetMagicPenetration() => data.magicPenetration;
    public float GetDamageAmplification() => data.damageAmplification;
    public float GetTenacity() => data.tenacity;
    public float GetHPRegen() => data.hpRegen;
    public float GetHealingReceived() => data.healingReceivedPercent;
    public float GetDamageReduction() => damageReductionModifier.CalculateForPercentStat(data.damageReduction);
    public float GetMaxHp() => maxHpModifier.Calculate(data.maxHp);
    #endregion

    #region Stat Modifiers
    public void ModifyPhysicalDamage(float flatBonus, float percentBonus = 0)
    {
        physicalDamageModifier.AddFlat(flatBonus);
        if (percentBonus != 0) physicalDamageModifier.AddPercent(percentBonus);
        isPhysicalDamageModified = false;
    }

    public void ModifyMagicDamage(float flatBonus, float percentBonus = 0)
    {
        magicDamageModifier.AddFlat(flatBonus);
        if (percentBonus != 0) magicDamageModifier.AddPercent(percentBonus);
        isMagicDamageModified = false;
    }

    public void ModifyArmor(float flatBonus, float percentBonus = 0)
    {
        armorModifier.AddFlat(flatBonus);
        if (percentBonus != 0) armorModifier.AddPercent(percentBonus);
    }

    public void ModifyMagicResist(float flatBonus, float percentBonus = 0)
    {
        magicResistModifier.AddFlat(flatBonus);
        if (percentBonus != 0) magicResistModifier.AddPercent(percentBonus);
    }

    public void ModifySpeed(float flatBonus, float percentBonus = 0)
    {
        speedModifier.AddFlat(flatBonus);
        if (percentBonus != 0) speedModifier.AddPercent(percentBonus);
    }

    public void ModifyHealingReceived(float percentBonus)
    {
        healingReceivedModifier.AddPercent(percentBonus);
    }

    public void ModifyLifeSteal(float flatBonus)
    {
        lifeStealModifier.AddFlat(flatBonus);
    }

    public void ModifyDamageReduction(float percentBonus)
    {
        damageReductionModifier.AddPercent(percentBonus);
    }

    public void ModifyAttackSpeed(float flatBonus, float percentBonus = 0)
    {
        if (flatBonus != 0) attackSpeedModifier.AddFlat(flatBonus);
        if (percentBonus != 0) attackSpeedModifier.AddPercent(percentBonus);
    }

    public void ModifyMaxHp(float flatBonus)
    {
        maxHpModifier.AddFlat(flatBonus);
    }

    #endregion

    #region Reset Methods
    public void ResetModifiers()
    {
        physicalDamageModifier.Reset();
        magicDamageModifier.Reset();
        armorModifier.Reset();
        magicResistModifier.Reset();
        speedModifier.Reset();
        healingReceivedModifier.Reset();
        isPhysicalDamageModified = false;
        isMagicDamageModified = false;
        lifeStealModifier.Reset();
    }

    public void ResetHealingReceived()
    {
        healingReceivedModifier.Reset();
    }
    #endregion

    #region Combat Utility Methods
    public bool RollForCritical()
    {
        return Random.value < GetCriticalChance() / 100f;
    }

    public float CalculateCriticalDamage(float damage)
    {
        return damage * GetCriticalDamage() / 100f;
    }

    public float CalculateLifestealAmount(float damageDealt)
    {
        return damageDealt * GetLifesteal() / 100f;
    }

    public float CalculateDamageAfterReduction(float damage)
    {
        return damage * (1f - GetDamageReduction() / 100f);
    }

    public float GetCrowdControlDuration(float baseDuration)
    {
        return baseDuration * (1f - GetTenacity());
    }
    #endregion

    private void FixedUpdate()
    {
        // Heath Regen
        if (!IsDead && currentHp < GetMaxHp())
        {
            hpRegenTimer += Time.fixedDeltaTime;
            if (hpRegenTimer >= HP_REGEN_INTERVAL)
            {
                float regenAmount = GetHPRegen();
                if (regenAmount > 0)
                {
                    Heal(regenAmount);
                }
                hpRegenTimer = 0f;
            }
        }

        // Update shield duration
        for (int i = shieldLayers.Count - 1; i >= 0; i--)
        {
            var shield = shieldLayers[i];
            shield.UpdateDuration(Time.fixedDeltaTime);

            if (shield.IsExpired)
            {
                shieldLayers.RemoveAt(i);
                OnShieldChanged?.Invoke(GetTotalShield());
            }
        }
    }

    public List<ShieldLayer> GetShieldLayers()
    {
        return new List<ShieldLayer>(shieldLayers);
    }
}

// Helper class để quản lý modifier
public class StatModifier
{
    private float flatBonus;
    private float percentBonus = 1;

    public void AddFlat(float value) => flatBonus += value;
    public void AddPercent(float value) => percentBonus += value;
    public void Reset() { flatBonus = 0; percentBonus = 1; }

    public float Calculate(float baseValue)
    {
        return (baseValue + flatBonus) * percentBonus;
    }

    public float CalculateForPercentStat(float baseValue)
    {
        return baseValue + percentBonus;
    }
}

public enum DamageType
{
    Physical,
    Magic,
    True
}
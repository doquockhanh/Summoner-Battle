using UnityEngine;

public class UnitStats : BaseStats
{
    private UnitData data;
    public UnitData Data => data;
    public bool IsDead => currentHp <= 0;
    public float CurrentHP => currentHp;
    public float CurrentHealthPercent => currentHp / GetMaxHp();

    // Events đặc biệt cho Unit
    public event System.Action<Unit> OnTakeLethalityDamage;
    public static event System.Func<float, Unit, Unit, DamageType, float> OnModifyRawDamage;

    public event System.Action<float, Unit> OnTakeDamage;
    public event System.Action OnDeath;
    public event System.Action<float> OnHealthChanged;

    public void Initialize(UnitData unitData)
    {
        data = unitData;
        currentHp = unitData.maxHp;
        currentShield = 0;
        // ResetModifiers();
    }

    public void TakeDamage(float amount, DamageType damageType, Unit source = null)
    {
        float finalDamage = CalculateFinalDamage(amount, damageType);

        OnModifyRawDamage?.Invoke(finalDamage, source, GetComponent<Unit>(), damageType);

        float remainingDamage = ProcessShieldDamage(finalDamage);
        ProcessHealthDamage(remainingDamage);

        OnTakeDamage?.Invoke(finalDamage, source);
    }

    private float ProcessHealthDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        if (currentHp <= 0)
        {
            OnTakeLethalityDamage?.Invoke(GetComponent<Unit>());
        }

        FloatingTextManager.Instance.ShowFloatingText(
            damage.ToString("F0"),
            transform.position,
            Color.red
        );

        OnHealthChanged?.Invoke(currentHp);
        CheckDeath();
        return damage;
    }

    private void CheckDeath()
    {
        if (currentHp <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        float healing = StatsCalculator.CalculateHealing(amount, this);
        currentHp = Mathf.Min(currentHp + healing, GetMaxHp());
        OnHealthChanged?.Invoke(currentHp);
    }

    public void SetCurrentHp(float hp)
    {
        currentHp = hp;
        OnHealthChanged?.Invoke(currentHp);
    }

    public bool RollForCritical()
    {
        return Random.value < GetCriticalChance() / 100f;
    }

    public float CalculateCriticalDamage(float damage)
    {
        return damage * GetCriticalDamage() / 100f;
    }


    // IStats Implementation
    public override float GetMaxHp() => GetModifiedStat(StatType.MaxHp, data.maxHp);
    public override float GetCurrentHp() => currentHp;
    public override float GetPhysicalDamage() => GetModifiedStat(StatType.PhysicalDamage, data.physicalDamage);
    public override float GetMagicDamage() => GetModifiedStat(StatType.MagicDamage, data.magicDamage);
    public override float GetArmor() => GetModifiedStat(StatType.Armor, data.armor);
    public override float GetMagicResist() => GetModifiedStat(StatType.MagicResist, data.magicResist);
    public override float GetMoveSpeed() => GetModifiedStat(StatType.MoveSpeed, data.moveSpeed);
    public override float GetAttackSpeed() => GetModifiedStat(StatType.AttackSpeed, data.attackSpeed);
    public override int GetRange() => (int)GetModifiedStat(StatType.Range, data.range);
    public override float GetLifeSteal() => GetModifiedStat(StatType.LifeSteal, data.lifestealPercent);
    public override float GetDamageReduction() => GetModifiedPercentStat(StatType.DamageReduction, data.damageReduction);
    public override float GetHealingReceived() => GetModifiedPercentStat(StatType.HealingReceived, data.healingReceivedPercent);

    // Additional Unit specific stats
    public float GetCriticalChance() => data.criticalChance;
    public float GetCriticalDamage() => data.criticalDamage;
    public float GetArmorPenetration() => data.armorPenetration;
    public float GetMagicPenetration() => data.magicPenetration;
    public float GetDamageAmplification() => data.damageAmplification;
    public float GetTenacity() => data.tenacity;
    public float GetHPRegen() => data.hpRegen;
    public int GetDetectRange() => data.detectRange;
}

public enum DamageType
{
    Physical,
    Magic,
    True
}
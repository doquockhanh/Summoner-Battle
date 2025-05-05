using UnityEngine;
using System.Collections.Generic;

public class CardStats : BaseStats, IStats
{
    [SerializeField] private Card data; // Tạm thời dùng chung UnitData
    public float CurrentHp => currentHp;
    public event System.Action<float> OnHealthChanged;

    public void Initialize(Card cardData)
    {
        data = cardData;
        currentHp = cardData.maxHp;
        currentShield = 0;
    }

    public void TakeDamage(float amount, DamageType damageType)
    {
        float finalDamage = StatsCalculator.CalculateFinalDamage(amount, damageType, this);

        currentHp = Mathf.Max(0, currentHp - finalDamage);
        OnHealthChanged?.Invoke(currentHp);
        FloatingTextManager.Instance.ShowFloatingText(
           finalDamage.ToString("F0"),
           transform.position,
           Color.red
       );
    }

    public override float GetMaxHp() => GetModifiedStat(StatType.MaxHp, data.maxHp);
    public override float GetCurrentHp() => currentHp;
    public override float GetPhysicalDamage() => GetModifiedStat(StatType.PhysicalDamage, data.physicalDamage);
    public override float GetArmor() => GetModifiedStat(StatType.Armor, data.armor);
    public override float GetMagicResist() => GetModifiedStat(StatType.MagicResist, data.magicResist);
    public override float GetAttackSpeed() => GetModifiedStat(StatType.AttackSpeed, data.attackSpeed);
    public override int GetRange() => (int)GetModifiedStat(StatType.Range, data.range);

    public override float GetMagicDamage()
    {
        throw new System.NotImplementedException();
    }

    public override float GetMoveSpeed()
    {
        throw new System.NotImplementedException();
    }

    public override float GetLifeSteal()
    {
        throw new System.NotImplementedException();
    }

    public override float GetDamageReduction() => 1f;

    public override float GetHealingReceived()
    {
        throw new System.NotImplementedException();
    }

}
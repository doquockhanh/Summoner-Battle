
using UnityEngine;

public class GuardianAuraEffect : BaseStatusEffect
{
    private readonly float armorBoost;
    private readonly float magicResistBoost;
    private UnitStats stats;

    public GuardianAuraEffect(
        float duration,
        float armorBoost,
        float magicResistBoost
    ) : base(duration)
    {
        this.armorBoost = armorBoost;
        this.magicResistBoost = magicResistBoost;
        type = StatusEffectType.DefenseBuff;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        
        stats = this.owner.GetComponent<UnitStats>();
        if (stats != null)
        {
            stats.ModifyStat(StatType.Armor, armorBoost);
            stats.ModifyStat(StatType.MagicResist, magicResistBoost);
        }
    }

    public override void Remove()
    {
        base.Remove();
        if (stats != null)
        {
            stats.ModifyStat(StatType.Armor, -armorBoost);
            stats.ModifyStat(StatType.MagicResist, -magicResistBoost);
        }
    }
}
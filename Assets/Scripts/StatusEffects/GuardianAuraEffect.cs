
using UnityEngine;

public class GuardianAuraEffect : BaseStatusEffect
{
    private readonly float armorBoost;
    private readonly float magicResistBoost;
    private readonly UnitStats stats;

    public GuardianAuraEffect(
        Unit target,
        float duration,
        float armorBoost,
        float magicResistBoost
    ) : base(target, duration)
    {
        this.armorBoost = armorBoost;
        this.magicResistBoost = magicResistBoost;
        this.stats = target.GetComponent<UnitStats>();
        type = StatusEffectType.DefenseBuff;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
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
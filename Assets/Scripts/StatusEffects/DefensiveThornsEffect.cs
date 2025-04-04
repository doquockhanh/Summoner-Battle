
using UnityEngine;

public class DefensiveThornsEffect : BaseStatusEffect
{
    private readonly float damageReduction;
    private readonly float thornsDamagePercent;
    private readonly UnitStats stats;

    public DefensiveThornsEffect(Unit target, float duration, float damageReduction, float thornsDamagePercent)
        : base(target, duration)
    {
        this.damageReduction = damageReduction;
        this.thornsDamagePercent = thornsDamagePercent;
        this.stats = target.GetComponent<UnitStats>();
        type = StatusEffectType.DefenseBuff;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        if (stats != null)
        {
            stats.ModifyStat(StatType.DamageReduction, 0, damageReduction);
            UnitEvents.Combat.OnTakeRawDamage += HandleDamageTaken;
        }
    }

    private void HandleDamageTaken(Unit source, Unit target, float damage)
    {
        // source là kẻ tấn công
        // target là kẻ chịu đòn
        // target != this.target loại bỏ kẻ chịu đòn ko phải chủ sở hữu trạng thái
        if (source == null || target == null || target != this.target) return;
        float thornsDamage = damage * (thornsDamagePercent / 100f);
        source.TakeDamage(thornsDamage, DamageType.ThornsDamage);
    }

    public override void Remove()
    {
        base.Remove();
        if (stats != null)
        {
            stats.ModifyStat(StatType.DamageReduction, 0, -damageReduction);
        }
        UnitEvents.Combat.OnTakeRawDamage -= HandleDamageTaken;
    }
}

using UnityEngine;

public class DefensiveThornsEffect : BaseStatusEffect
{
    private readonly float damageReduction;
    private readonly float thornsDamagePercent;
    private UnitStats stats;

    public DefensiveThornsEffect(float duration, float damageReduction, float thornsDamagePercent)
        : base(duration)
    {
        this.damageReduction = damageReduction;
        this.thornsDamagePercent = thornsDamagePercent;
        type = StatusEffectType.DefenseBuff;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        this.stats = this.owner.GetComponent<UnitStats>();
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
        // target != this.owner loại bỏ kẻ chịu đòn ko phải chủ sở hữu trạng thái
        if (source == null || source.IsDead || target == null || target != owner) return;
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
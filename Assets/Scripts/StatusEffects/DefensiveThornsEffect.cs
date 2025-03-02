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
            stats.ModifyDamageReduction(damageReduction);
            stats.OnTakeDamage += HandleDamageTaken;
        }
    }

    private void HandleDamageTaken(float damage, Unit source)
    {
        if (source == null) return;
        float thornsDamage = damage * (thornsDamagePercent / 100f);
        source.TakeDamage(thornsDamage, DamageType.Magic);
    }

    public override void Remove()
    {
        base.Remove();
        if (stats != null)
        {
            stats.ModifyDamageReduction(-damageReduction);
            stats.OnTakeDamage -= HandleDamageTaken;
        }
    }
} 
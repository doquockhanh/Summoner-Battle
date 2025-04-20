using UnityEngine;

public class BurningEffect : BaseStatusEffect
{
    private readonly float maxHealthPercent;
    private readonly float healingReduction;
    private float damageTimer;
    private const float DAMAGE_INTERVAL = 1f;
    private UnitStats stats;

    public BurningEffect(float duration, float maxHealthPercent, float healingReduction)
        : base(duration)
    {
        this.maxHealthPercent = maxHealthPercent;
        this.healingReduction = healingReduction;
        type = StatusEffectType.Burning;
        damageTimer = DAMAGE_INTERVAL;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        this.stats = this.owner.GetComponent<UnitStats>();
        if (stats != null)
        {
            stats.ModifyStat(StatType.HealingReceived, -healingReduction);
        }
    }

    public override void Tick()
    {
        base.Tick();

        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0)
        {
            // Gây sát thương theo % máu tối đa
            float damage = stats.GetMaxHp() * maxHealthPercent;
            owner.TakeDamage(damage, DamageType.Magic);

            damageTimer = DAMAGE_INTERVAL;
        }
    }

    public override void Remove()
    {
        base.Remove();
        if (stats != null)
        {
            stats.ModifyStat(StatType.HealingReceived, healingReduction);
        }
    }
}
using UnityEngine;

public class BurningEffect : BaseStatusEffect
{
    private readonly float maxHealthPercent;
    private readonly float healingReduction;
    private float damageTimer;
    private const float DAMAGE_INTERVAL = 1f;
    private UnitStats stats;

    public BurningEffect(Unit target, float duration, float maxHealthPercent, float healingReduction) 
        : base(target, duration)
    {
        this.maxHealthPercent = maxHealthPercent;
        this.healingReduction = healingReduction;
        this.stats = target.GetComponent<UnitStats>();
        type = StatusEffectType.Burning;
        damageTimer = DAMAGE_INTERVAL;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        if (stats != null)
        {
            stats.ModifyHealingReceived(-healingReduction);
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
            target.TakeDamage(damage, DamageType.Magic);
            
            damageTimer = DAMAGE_INTERVAL;
        }
    }

    public override void Remove()
    {
        base.Remove();
        if (stats != null)
        {
            stats.ResetHealingReceived();
        }
    }
} 
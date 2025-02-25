using UnityEngine;

public class AssassinStealthEffect : BaseStatusEffect
{
    private const float STEALTH_DURATION = 2f;

    public AssassinStealthEffect(Unit target) 
        : base(target, float.PositiveInfinity)
    {
        type = StatusEffectType.StealthOnKill;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        UnitEvents.Combat.OnDamageDealt += HandleKill;
    }

    private void HandleKill(Unit source, Unit target, float amount)
    {
        if (source == this.target && target.IsDead)
        {
            var statusEffects = source.GetComponent<UnitStatusEffects>();
            if (statusEffects != null)
            {
                var tempStealth = new TemporaryStealthEffect(source, STEALTH_DURATION);
                statusEffects.AddEffect(tempStealth);
            }
        }
    }

    public override void Remove()
    {
        base.Remove();
        UnitEvents.Combat.OnDamageDealt -= HandleKill;
    }
} 
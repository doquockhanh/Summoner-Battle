using UnityEngine;

public class AssassinStealthEffect : BaseStatusEffect
{
    private float untargetableDuration = 1f;

    public AssassinStealthEffect(float untargetableDuration)
        : base(float.PositiveInfinity)
    {
        type = StatusEffectType.StealthOnKill;
        this.untargetableDuration = untargetableDuration;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        UnitEvents.Combat.OnDeath += HandleKill;
    }

    private void HandleKill(Unit source, Unit target)
    {
        if (source == owner)
        {
            var statusEffects = source.GetComponent<UnitStatusEffects>();
            if (statusEffects != null)
            {
                UntargetableEffect untargetable = new UntargetableEffect(untargetableDuration);
                statusEffects.AddEffect(untargetable);
            }
        }
    }

    public override void Remove()
    {
        base.Remove();
        UnitEvents.Combat.OnDeath -= HandleKill;
    }
}
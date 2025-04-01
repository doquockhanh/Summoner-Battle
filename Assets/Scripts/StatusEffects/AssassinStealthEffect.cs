using UnityEngine;

public class AssassinStealthEffect : BaseStatusEffect
{
    private float untargetableDuration = 1f;
    private Unit caster;

    public AssassinStealthEffect(Unit caster, float untargetableDuration)
        : base(caster, float.PositiveInfinity)
    {
        type = StatusEffectType.StealthOnKill;
        this.caster = caster;
        this.untargetableDuration = untargetableDuration;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        UnitEvents.Combat.OnDeath += HandleKill;
    }

    private void HandleKill(Unit source, Unit target)
    {
        if (source == caster)
        {
            var statusEffects = source.GetComponent<UnitStatusEffects>();
            if (statusEffects != null)
            {
                Debug.Log("apply untargetable");
                UntargetableEffect untargetable = new UntargetableEffect(source, untargetableDuration);
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
using UnityEngine;

public class StunEffect : BaseStatusEffect
{
    private UnitCombat combat;

    public StunEffect(Unit target, float duration) : base(target, duration)
    {
        type = StatusEffectType.Stun;
        combat = target.GetComponent<UnitCombat>();
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        if (combat != null) combat.enabled = false;;
    }

    public override void Remove()
    {
        base.Remove();
        if (combat != null) combat.enabled = true;
    }
} 
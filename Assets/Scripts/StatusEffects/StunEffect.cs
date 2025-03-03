using UnityEngine;

public class StunEffect : BaseStatusEffect
{
    private UnitMovement movement;
    private UnitCombat combat;

    public StunEffect(Unit target, float duration) : base(target, duration)
    {
        type = StatusEffectType.Stun;
        movement = target.GetComponent<UnitMovement>();
        combat = target.GetComponent<UnitCombat>();
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        if (movement != null) movement.enabled = false;;
        if (combat != null) combat.enabled = false;;
    }

    public override void Remove()
    {
        base.Remove();
        if (movement != null) movement.enabled = true;
        if (combat != null) combat.enabled = true;
    }
} 
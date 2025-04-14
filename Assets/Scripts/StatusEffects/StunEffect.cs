using UnityEngine;

public class StunEffect : BaseStatusEffect
{
    private UnitCombat combat;

    public StunEffect(float duration) : base(duration)
    {
        type = StatusEffectType.Stun;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        combat = this.owner.GetComponent<UnitCombat>();
        if (combat != null) combat.enabled = false;;
    }

    public override void Remove()
    {
        base.Remove();
        if (combat != null) combat.enabled = true;
    }
} 
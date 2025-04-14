using UnityEngine;

public class BloodstormStatusEffect : BaseStatusEffect
{
    private readonly BloodstormSkill skill;
    private readonly int absorbedSouls;
    private float originalMoveSpeed;

    public BloodstormStatusEffect(Unit target, BloodstormSkill skill, int souls) 
        : base(float.PositiveInfinity) // Vĩnh viễn cho đến khi chết
    {
        this.skill = skill;
        this.absorbedSouls = souls;
        type = StatusEffectType.Bloodstorm;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        
    }

    public override void Remove()
    {
        base.Remove();
        
    }
} 
using UnityEngine;

public class BloodstormStatusEffect : BaseStatusEffect
{
    private readonly BloodstormSkill skill;
    private readonly int absorbedSouls;
    private readonly UnitMovement movement;
    private float originalMoveSpeed;

    public BloodstormStatusEffect(Unit target, BloodstormSkill skill, int souls) 
        : base(target, float.PositiveInfinity) // Vĩnh viễn cho đến khi chết
    {
        this.skill = skill;
        this.absorbedSouls = souls;
        this.movement = target.GetComponent<UnitMovement>();
        type = StatusEffectType.Bloodstorm;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        
        if (movement != null)
        {

        }
    }

    public override void Remove()
    {
        base.Remove();
        
        if (movement != null)
        {
   
        }
    }
} 
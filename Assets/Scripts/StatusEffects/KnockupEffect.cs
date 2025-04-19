using UnityEngine;

public class KnockupEffect : BaseStatusEffect
{
    private UnitCombat combat;
    private int knockupHeight;
    public KnockupEffect(float duration, int knockupHeight = 2) : base(duration)
    {
        type = StatusEffectType.Knockup;
        this.knockupHeight = knockupHeight;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        combat = this.owner.GetComponent<UnitCombat>();
        if (combat != null) combat.TurnOffAutoCombat();
    }

    public override void Tick()
    {
        base.Tick();

        if (owner != null)
        {
            // Calculate the upward movement based on remainingTime / duration
            float normalizedTime = remainingTime / (duration/ 2) - 1; // Value between -1 and 1
            float upwardMovement = knockupHeight * normalizedTime * Time.fixedDeltaTime;

            // Move the owner upward
            owner.transform.Translate(Vector3.up * upwardMovement);
        }

    }

    public override void Remove()
    {
        base.Remove();
        if (combat != null) combat.TurnOnAutoCombat();
    }
}
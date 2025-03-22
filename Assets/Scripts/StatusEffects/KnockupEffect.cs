public class KnockupEffect : BaseStatusEffect
{
    private UnitMovement movement;

    public KnockupEffect(Unit target, float duration) : base(target, duration)
    {
        type = StatusEffectType.Knockup;
        movement = target.GetComponent<UnitMovement>();
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        if (movement != null)
        {
            // movement.Knockup(duration);
        }
    }

    public override void Remove()
    {
        base.Remove();
        // Cleanup sẽ được xử lý trong UnitMovement
    }
} 
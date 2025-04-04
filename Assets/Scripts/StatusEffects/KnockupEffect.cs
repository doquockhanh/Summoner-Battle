public class KnockupEffect : BaseStatusEffect
{

    public KnockupEffect(Unit target, float duration) : base(target, duration)
    {
        type = StatusEffectType.Knockup;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
    }

    public override void Remove()
    {
        base.Remove();
        // Cleanup sẽ được xử lý trong UnitMovement
    }
} 
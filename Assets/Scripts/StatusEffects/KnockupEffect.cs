public class KnockupEffect : BaseStatusEffect
{

    public KnockupEffect(float duration) : base(duration)
    {
        type = StatusEffectType.Knockup;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
    }

    public override void Remove()
    {
        base.Remove();
        // Cleanup sẽ được xử lý trong UnitMovement
    }
} 
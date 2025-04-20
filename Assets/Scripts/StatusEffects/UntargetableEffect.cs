using UnityEngine;

public class UntargetableEffect : BaseStatusEffect
{
    public UntargetableEffect(float duration)
        : base(duration)
    {
        type = StatusEffectType.Untargetable;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        var view = this.owner.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(0.2f);
        }
    }

    public override void Remove()
    {
        base.Remove();
        // Khôi phục hiển thị unit
        var view = this.owner.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(1f);
        }
    }
}
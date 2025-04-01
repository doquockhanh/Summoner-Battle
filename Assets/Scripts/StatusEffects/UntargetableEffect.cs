using UnityEngine;

public class UntargetableEffect : BaseStatusEffect
{
    private Unit caster;
    public UntargetableEffect(Unit target, float duration) 
        : base(target, duration)
    {
        type = StatusEffectType.Untargetable;
        caster = target;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        var view = caster.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(0.2f);
        }
    }

    public override void Remove()
    {
        base.Remove();
        // Khôi phục hiển thị unit
        var view = caster.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(1f);
        }
    }
} 
using UnityEngine;

public class TemporaryStealthEffect : BaseStatusEffect
{
    public TemporaryStealthEffect(Unit target, float duration) 
        : base(target, duration)
    {
        type = StatusEffectType.StealthOnKill;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        // Thêm logic ẩn unit ở đây (ví dụ: đổi alpha)
        var view = target.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(0.2f);
        }
    }

    public override void Remove()
    {
        base.Remove();
        // Khôi phục hiển thị unit
        var view = target.GetComponent<UnitView>();
        if (view != null)
        {
            view.SetAlpha(1f);
        }
    }
} 
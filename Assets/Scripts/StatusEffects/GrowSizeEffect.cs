using UnityEngine;

public class GrowSizeEffect : BaseStatusEffect
{
    private float sizeMultiplier = 1.5f;
    private Vector3 originalScale; // Lưu scale ban đầu

    public GrowSizeEffect(
      Unit target,
      float duration,
      float sizeMultiplier) : base(target, duration)
    {
        this.sizeMultiplier = sizeMultiplier;
        type = StatusEffectType.GrowSize;
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        originalScale = target.transform.localScale;
        // Tăng kích thước lên theo hệ số
        target.transform.localScale = originalScale * sizeMultiplier;
    }

    public override void Remove()
    {
        base.Remove();
        target.transform.localScale = originalScale;
    }
}
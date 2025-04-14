using UnityEngine;

public class GrowSizeEffect : BaseStatusEffect
{
    private float sizeMultiplier = 1.5f;
    private Vector3 originalScale; // Lưu scale ban đầu

    public GrowSizeEffect(
      float duration,
      float sizeMultiplier) : base(duration)
    {
        this.sizeMultiplier = sizeMultiplier;
        type = StatusEffectType.GrowSize;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        originalScale = this.owner.transform.localScale;
        // Tăng kích thước lên theo hệ số
        this.owner.transform.localScale = originalScale * sizeMultiplier;
    }

    public override void Remove()
    {
        base.Remove();
        owner.transform.localScale = originalScale;
    }
}
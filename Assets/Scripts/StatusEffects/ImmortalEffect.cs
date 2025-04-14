using UnityEngine;

public class ImmortalEffect : BaseStatusEffect
{
    public event System.Action OnImmortalEffectEnded;

    private UnitStats stats;

    public ImmortalEffect(float duration) : base(duration)
    {
        type = StatusEffectType.Immortal;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        stats = this.owner.GetComponent<UnitStats>();
        if (stats != null)
        {
            // Đăng ký sự kiện trước khi nhận sát thương kết liễu
            stats.OnTakeLethalityDamage += PreventLethalDamage;
        }
    }

    private void PreventLethalDamage(Unit unit)
    {
        if (stats == null) return;

        if (unit.GetComponent<UnitStats>().CurrentHP <= 0)
        {
            unit.GetComponent<UnitStats>().SetCurrentHp(1);
        }
    }

    public override void Remove()
    {
        if (stats != null)
        {
            stats.OnTakeLethalityDamage -= PreventLethalDamage;
        }

        // Kích hoạt sự kiện khi kết thúc
        OnImmortalEffectEnded?.Invoke();

        base.Remove();
    }
}
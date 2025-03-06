using UnityEngine;

public class ImmortalEffect : BaseStatusEffect
{
    public event System.Action OnImmortalEffectEnded;

    private UnitStats stats;

    public ImmortalEffect(Unit target, float duration) : base(target, duration)
    {
        type = StatusEffectType.Immortal;
        stats = target.GetComponent<UnitStats>();
    }

    public override void Apply(Unit target)
    {
        base.Apply(target);
        
        if (stats != null)
        {
            // Đăng ký sự kiện trước khi nhận sát thương kết liễu
            stats.OnTakeLethalityDamage += PreventLethalDamage;
        }
    }

    private void PreventLethalDamage(Unit unit)
    {
        if (stats == null) return;

        if (unit.GetComponent<UnitStats>().CurrentHP <= 0) {
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
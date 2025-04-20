using UnityEngine;

public class EmpoweredAttacksEffect : BaseStatusEffect
{
    private readonly float damageMultiplier;
    private int remainingAttacks;
    private UnitCombat combat;

    public EmpoweredAttacksEffect(float duration, float damageMultiplier = 1f, int attackCount = 1)
        : base(duration)
    {
        this.damageMultiplier = damageMultiplier;
        this.remainingAttacks = attackCount;
        this.type = StatusEffectType.DamageBuff;
    }

    public override void Apply(Unit owner)
    {
        base.Apply(owner);
        this.combat = this.owner.GetComponent<UnitCombat>();
        if (combat != null)
        {
            UnitEvents.Combat.OnDamageDealt += HandleAttack;
            owner.GetUnitStats().ModifyStat(StatType.PhysicalDamage, 0, damageMultiplier);
        }
    }

    private void HandleAttack(Unit source, Unit target, float damage)
    {
        if (source != this.owner) return;

        remainingAttacks--;

        // Kiểm tra điều kiện kết thúc
        if (remainingAttacks <= 0)
        {
            Remove();
        }
    }

    public override void Remove()
    {
        if (combat != null)
        {
            UnitEvents.Combat.OnDamageDealt -= HandleAttack;
            owner.GetUnitStats().ModifyStat(StatType.PhysicalDamage, 0, -damageMultiplier);
        }
        base.Remove();
    }

    public override void Tick()
    {
        base.Tick();

        // Tự động remove khi hết thời gian
        if (IsExpired)
        {
            Remove();
        }
    }

    // Thêm phương thức để kiểm tra số đòn đánh còn lại
    public int GetRemainingAttacks()
    {
        return remainingAttacks;
    }
}
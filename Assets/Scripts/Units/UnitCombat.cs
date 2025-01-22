using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitView view;
    private float attackTimer;

    public void Initialize(Unit unit, UnitStats stats, UnitView view)
    {
        this.unit = unit;
        this.stats = stats;
        this.view = view;
        attackTimer = 0;
    }

    public void TryAttack(Unit target)
    {
        if (attackTimer <= 0)
        {
            target.TakeDamage(stats.GetModifiedDamage());
            view.PlayAttackEffect();
            attackTimer = 1f / stats.Data.attackSpeed;
        }
        attackTimer -= Time.deltaTime;
    }

    public void AttackBase(Base baseTarget)
    {
        if (attackTimer <= 0)
        {
            baseTarget.TakeDamage(stats.GetModifiedDamage());
            view.PlayAttackEffect();
            attackTimer = 1f / stats.Data.attackSpeed;
        }
        attackTimer -= Time.deltaTime;
    }
} 
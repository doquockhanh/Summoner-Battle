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
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        Attack(target);
        attackTimer = 1f / stats.Data.attackSpeed;
    }

    public void AttackBase(Base enemyBase)
    {
        enemyBase.TakeDamage(stats.GetModifiedDamage());
        view.PlayAttackEffect();
        attackTimer = 1f / stats.Data.attackSpeed;
    }

    private void Attack(Unit target)
    {
        target.TakeDamage(stats.GetModifiedDamage());
        view.PlayAttackEffect();
    }
} 
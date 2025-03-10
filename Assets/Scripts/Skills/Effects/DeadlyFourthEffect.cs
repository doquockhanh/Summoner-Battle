using System.Linq;
using UnityEngine;

public class DeadlyFourthEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private DeadlyFourthSkill skillData;
    private int attackCount;
    private UnitCombat combat;
    private UnitTargeting targeting;
    private Unit currentTarget;

    public void Initialize(Unit caster, DeadlyFourthSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.attackCount = 0;
        this.combat = caster.GetComponent<UnitCombat>();
        this.targeting = caster.GetComponent<UnitTargeting>();
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        // Đăng ký sự kiện cho execute mechanic
        UnitEvents.Combat.OnDamageDealt += HandleAttackCount;
    }

    private void HandleAttackCount(Unit attacker, Unit target, float damage)
    {
        if (attacker != caster) return;

        attackCount++;
        if (attackCount >= 4)
        {
            attackCount = 0;
            FindAndAttackWeakestTarget();
        }
    }

    private void FindAndAttackWeakestTarget()
    {
        Unit weakestEnemy = null;

        float lowestHealthPercent = float.MaxValue;

        var enemiesInRange = targeting.GetUnitsInRange(skillData.fourthShotRange)
                                        .Where(unit => unit.IsPlayerUnit != caster.IsPlayerUnit)
                                        .ToArray();
        foreach (var enemy in enemiesInRange)
        {
            float healthPercent = enemy.GetCurrentHP() / enemy.GetUnitStats().GetMaxHp();
            if (healthPercent < lowestHealthPercent)
            {
                lowestHealthPercent = healthPercent;
                weakestEnemy = enemy;
            }
        }



        if (weakestEnemy != null)
        {
            EmpoweredAttacksEffect empoweredEffect = new EmpoweredAttacksEffect(
                target: caster,
                duration: 5f,
                damageMultiplier: 1f,
                attackCount: 1
            );

            // Thêm effect cường hóa đòn đánh
            caster.GetComponent<UnitStatusEffects>().AddEffect(empoweredEffect);

            // Hết số đòn cường hóa thì tấn công mục tiêu cũ nếu còn sống
            empoweredEffect.OnExpired += () =>
            {
                if (currentTarget != null)
                {
                    targeting.AssignTarget(currentTarget);
                };
            };

            // Tạm dừng targeting để tấn công mục tiêu yếu nhất
            targeting.PauseTargeting();
            currentTarget = targeting.CurrentTarget;
            targeting.AssignTarget(weakestEnemy);
            targeting.ResumeTargeting();
        }
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("DeadlyFourth: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        if (combat != null)
        {
            UnitEvents.Combat.OnDamageDealt -= HandleAttackCount;
        }
        Destroy(this);
    }
}
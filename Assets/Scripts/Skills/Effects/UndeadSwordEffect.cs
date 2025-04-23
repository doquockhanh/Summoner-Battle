using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndeadSwordEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private GiantSwordSkill skillData;
    private HexCell targetPos;


    public void Initialize(Unit caster, GiantSwordSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;
        this.targetPos = HexGrid.Instance.GetCellAtPosition(targetPos);

        HandleUndeadSwordSkill();
    }

    public void HandleUndeadSwordSkill()
    {
        Unit target = targetPos.OccupyingUnit;

        if (target != null)
        {
            target.TakeDamage(
                caster.GetUnitStats().GetPhysicalDamage() * (skillData.mainTargetPercent / 100),
                DamageType.Physical,
                caster
            );
            var knockupEffect = new KnockupEffect(skillData.knockUpDuration, 2);
            target.GetComponent<UnitStatusEffects>().AddEffect(knockupEffect);
        }


        List<Unit> enemiesNearBy = HexGrid.Instance.GetUnitsInRange(targetPos.Coordinates, skillData.skillRange, !caster.IsPlayerUnit);
        foreach (Unit enemy in enemiesNearBy)
        {
            if (enemy == null || enemy == target) continue;

            enemy.TakeDamage(
                caster.GetUnitStats().GetPhysicalDamage() * (skillData.areaPercent / 100),
                DamageType.Physical,
                caster
            );
        }

        Cleanup();
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("UndeadSword: Invalid setup");
            return false;
        }
        return true;

    }


    public void Cleanup()
    {
        Destroy(this);
    }

}

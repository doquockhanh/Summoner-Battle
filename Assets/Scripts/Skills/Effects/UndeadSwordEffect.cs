using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndeadSwordEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private GiantSwordSkill skillData;

    public void Initialize(Unit caster, GiantSwordSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        /**
        1. turn off auto combat for a while
        2. add growSize effect
        3. run anim
        4. wait until action
        **/
        caster.GetUnitCombat().TurnOffAutoCombatTemporarily(skillData.animationDuration);
        GrowSizeEffect growSizeEffect = new(5f, 1.3f);
        caster.GetComponent<UnitView>().PlaySkillAnimation();
        caster.GetComponent<UnitStatusEffects>().AddEffect(growSizeEffect);
        this.StartCoroutineSafely(HandleUndeadSwordSkill());
    }

    public IEnumerator HandleUndeadSwordSkill()
    {
        yield return new WaitForSeconds(skillData.doSkillActionAt);

        Unit target;
        if (caster.Targeting.IsInAttackRange(caster.Targeting.CurrentTarget))
        {
            target = caster.Targeting.CurrentTarget;
        }
        else
        {
            target = HexGrid.Instance.FindNearestUnitCell(caster.OccupiedCell.Coordinates, skillData.maxDashDistance, !caster.IsPlayerUnit)?.OccupyingUnit;

            if (target != null)
            {
                List<HexCell> path = caster.Combat.pathFinder.FindPathIgnoreOccupied(caster.OccupiedCell, target.OccupiedCell);
                HexCell newCell = path.Where(cell => cell.IsOccupied == false).LastOrDefault();

                if (newCell != null)
                {
                    // Lưu vị trí hiện tại
                    Vector3 startPos = caster.transform.position;
                    Vector3 endPos = newCell.WorldPosition;

                    // Tính toán hướng dash
                    bool faceRight = endPos.x > startPos.x;
                    caster.GetComponent<UnitView>().FlipSprite(faceRight);

                    // Tạo coroutine để thực hiện dash
                    float elapsedTime = 0;
                    Vector3 direction = (endPos - startPos).normalized;

                    while (elapsedTime < Vector3.Distance(startPos, endPos) / skillData.dashSpeed)
                    {
                        caster.transform.position += direction * skillData.dashSpeed * Time.deltaTime;
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    HexGrid.Instance.OccupyCell(newCell, caster);
                    caster.Targeting.FindNewTarget();
                    caster.Combat.SetRegisteredCell(newCell);
                }
            }
        }

        if (target == null)
        {
            Debug.Log("Không thể tìm thấy target");
            yield break;
        }

        HexCell cell = HexGrid.Instance.GetCellAtPosition(target.transform.position);
        List<Unit> enemiesNearBy = HexGrid.Instance.GetUnitsInRange(cell.Coordinates, skillData.skillRange, !caster.IsPlayerUnit);
        foreach (Unit enemy in enemiesNearBy)
        {
            if (enemy == null || enemy == target) continue;

            enemy.TakeDamage(
                caster.GetUnitStats().GetPhysicalDamage() * (skillData.areaPercent / 100),
                DamageType.Physical,
                caster
            );
        }


        target.TakeDamage(
                      caster.GetUnitStats().GetPhysicalDamage() * (skillData.mainTargetPercent / 100),
                      DamageType.Physical,
                      caster
                  );

        var knockupEffect = new KnockupEffect(skillData.knockUpDuration, 2);
        target.GetComponent<UnitStatusEffects>().AddEffect(knockupEffect);

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

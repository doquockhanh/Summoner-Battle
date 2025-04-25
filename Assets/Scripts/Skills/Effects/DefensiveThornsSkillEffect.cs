using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefensiveThornsSkillEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private DefensiveThornsSkill skillData;

    public void Initialize(Unit caster, DefensiveThornsSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;


        GrowSizeEffect growSizeEffect = new(5f, 1.3f);
        caster.GetComponent<UnitView>().PlaySkillAnimation();
        caster.GetComponent<UnitStatusEffects>().AddEffect(growSizeEffect);
        caster.GetUnitCombat().TurnOffAutoCombatTemporarily(skillData.animationDuration);

        this.StartCoroutineSafely(HandleDefensiveThornsSkill());
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("ForgeShieldEffect: Invalid setup");
            return false;
        }
        return true;
    }

    public IEnumerator HandleDefensiveThornsSkill()
    {
        yield return new WaitForSeconds(skillData.doSkillActionAt);

        if (caster == null) yield break;

        if (skillData.tauntEffectPrefab != null)
        {
            GameObject tauntEffect = Instantiate(skillData.tauntEffectPrefab,
                caster.transform.position, Quaternion.identity);
            Destroy(tauntEffect, 1f);
        }
        else if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.ShowRangeIndicator(caster.OccupiedCell, skillData.tauntRadius, Color.yellow, 0.2f);
        }

        // Thêm hiệu ứng gai và giảm sát thương
        var statusEffects = caster.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            // status này cho giảm sát thương và phản sát thương trong vài giây
            var thornsEffect = new DefensiveThornsEffect(
                skillData.thornsDuration,
                skillData.damageReduction,
                skillData.thornsDamagePercent
            );
            statusEffects.AddEffect(thornsEffect);
        }

        // Tìm và khiêu khích kẻ địch xung quanh
        List<Unit> enemies = HexGrid.Instance.GetUnitsInRange(caster.OccupiedCell.Coordinates, skillData.tauntRadius, !caster.IsPlayerUnit);
        foreach (Unit enemy in enemies)
        {
            if (enemy != null)
            {
                var targeting = enemy.GetComponent<UnitTargeting>();
                if (targeting != null)
                {
                    targeting.SetTarget(caster);
                }
            }
        }

        // Hiệu ứng visual
        if (skillData.thornsEffectPrefab != null)
        {
            GameObject thornsEffect = Instantiate(
                skillData.thornsEffectPrefab,
                caster.transform.position,
                Quaternion.identity,
                caster.transform
            );
            Destroy(thornsEffect, skillData.thornsDuration);
        }
        Cleanup();
    }

    public void Cleanup()
    {
        Destroy(this);
    }
}
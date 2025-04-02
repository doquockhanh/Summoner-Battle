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

        ApplyInitialEffects();
        HandleDefensiveThornsSkill();
    }


    private void ApplyInitialEffects()
    {
        if (skillData.tauntEffectPrefab != null)
        {
            GameObject chargeEffect = Instantiate(skillData.tauntEffectPrefab,
                caster.transform.position, Quaternion.identity);
            Destroy(chargeEffect, 1f);
        } else if (SkillEffectHandler.Instance != null) {
            SkillEffectHandler.Instance.ShowRangeIndicator(caster.OccupiedCell, skillData.tauntRadius, Color.yellow, 0.2f);
        }
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

    public void HandleDefensiveThornsSkill()
    {
        if (caster == null) return;

        // Thêm hiệu ứng gai và giảm sát thương
        var statusEffects = caster.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            // status này cho giảm sát thương và phản sát thương trong vài giây
            var thornsEffect = new DefensiveThornsEffect(
                caster,
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
    }

    public void Cleanup() {
        Destroy(this);
    }

}
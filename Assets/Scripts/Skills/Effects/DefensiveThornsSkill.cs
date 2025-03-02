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

        StartCoroutine(ApplyInitialEffects());
        HandleDefensiveThornsSkill();
    }


    private IEnumerator ApplyInitialEffects()
    {
        if (skillData.tauntEffectPrefab != null)
        {
            GameObject chargeEffect = Instantiate(skillData.tauntEffectPrefab,
                caster.transform.position, Quaternion.identity);
            Destroy(chargeEffect, 1f);
        } else if (SkillEffectHandler.Instance != null) {
            int indicatorId =SkillEffectHandler.Instance.ShowRangeIndicator(caster.transform.position, skillData.tauntRadius, Color.yellow);
            yield return new WaitForSeconds(1f);
            SkillEffectHandler.Instance.HideRangeIndicator(indicatorId);
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
            var thornsEffect = new DefensiveThornsEffect(
                caster,
                skillData.thornsDuration,
                skillData.damageReduction,
                skillData.thornsDamagePercent
            );
            statusEffects.AddEffect(thornsEffect);
        }

        // Tìm và khiêu khích kẻ địch xung quanh
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, skillData.tauntRadius);
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (enemy != null && enemy.IsPlayerUnit != caster.IsPlayerUnit)
            {
                var targeting = enemy.GetComponent<UnitTargeting>();
                if (targeting != null)
                {
                    targeting.AssignTarget(caster);
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
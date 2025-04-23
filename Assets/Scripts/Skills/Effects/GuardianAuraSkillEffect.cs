using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianAuraSkillEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private List<Unit> allies;
    private GuardianAuraSkill skillData;

    public void Initialize(Unit caster, List<Unit> allies, GuardianAuraSkill skillData)
    {
        this.caster = caster;
        this.allies = allies;
        this.skillData = skillData;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        StartCoroutine(GuardianAuraCoroutine(caster, allies, skillData));
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("GuardianAuraSkill: Invalid setup");
            return false;
        }
        return true;
    }

    private IEnumerator GuardianAuraCoroutine(Unit caster, List<Unit> allies, GuardianAuraSkill skill)
    {
        // Hiển thị vòng tròn AOE
        int indicatorId =
                SkillEffectHandler.Instance
                .ShowRangeIndicator(caster.OccupiedCell, HexMetrics.GridToWorldRadius(skill.auraRadius), Color.cyan);

        // Áp dụng buff cho caster và allies
        foreach (Unit ally in allies)
        {
            if (ally != null)
            {
                ApplySkillEffects(ally, skill);
            }
        }

        yield return new WaitForSeconds(0.5f);
        SkillEffectHandler.Instance.HideRangeIndicator(indicatorId);
        Cleanup();
    }

    private void ApplySkillEffects(Unit unit, GuardianAuraSkill skill)
    {
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            var auraEffect = new GuardianAuraEffect(
                skill.auraDuration,
                skill.armorBoost,
                skill.magicResistBoost
            );
            statusEffects.AddEffect(auraEffect);
        }


        if (skill.auraEffectPrefab != null)
        {
            GameObject auraEffect = Instantiate(
                skill.auraEffectPrefab,
                unit.transform.position,
                Quaternion.identity,
                caster.transform
            );
            Destroy(auraEffect, skill.auraDuration);
        }
    }

    public void Cleanup()
    {
        StopAllCoroutines();
        Destroy(this);
    }
}
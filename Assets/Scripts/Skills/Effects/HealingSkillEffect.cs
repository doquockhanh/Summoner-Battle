using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingSkillEffect : MonoBehaviour, ISkillEffect
{
    private Unit target;
    private HealingSkill skillData;

    public void Initialize(Unit target, HealingSkill skillData)
    {
        this.target = target;
        this.skillData = skillData;
    }

    public void Execute(Vector3 _)
    {
        if (!ValidateExecution()) return;

        HandleHealingSkill(target, skillData);
    }

    private bool ValidateExecution()
    {
        if (target == null || skillData == null)
        {
            Debug.LogError("HealingSkill: Invalid setup");
            return false;
        }
        return true;
    }

    public void HandleHealingSkill(Unit target, HealingSkill skill)
    {
        if (target == null) return;

        // Tính lượng máu hồi phục
        float healAmount = target.GetUnitStats().GetMaxHp() * (skill.healPercent / 100f);
        target.GetUnitStats().Heal(healAmount);

        // Hồi mana cho card sở hữu unit
        if (target.OwnerCard != null)
        {
            target.OwnerCard.AddMana(skill.manaRestore);
        }

        // Hiệu ứng hồi máu
        if (skill.healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(
                skill.healEffectPrefab,
                target.transform.position,
                Quaternion.identity
            );
            Destroy(healEffect, 1f);
        }

        Cleanup();
    }

    public void Cleanup()
    {
        Destroy(this);
    }
}
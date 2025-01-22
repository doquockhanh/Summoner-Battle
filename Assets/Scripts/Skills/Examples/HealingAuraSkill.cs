using UnityEngine;

[CreateAssetMenu(fileName = "HealingAura", menuName = "Game/Skills/HealingAura")]
public class HealingAuraSkill : SkillData
{
    private void OnEnable()
    {
        skillName = "Healing Aura";
        description = "Hồi máu cho tất cả đồng minh trong phạm vi";
        targetType = TargetType.AOE;
        targetRadius = 3f;
        manaCost = 70f;
        healing = 30f;
    }
} 
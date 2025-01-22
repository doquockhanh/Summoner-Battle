using UnityEngine;

[CreateAssetMenu(fileName = "PowerBuff", menuName = "Game/Skills/PowerBuff")]
public class PowerBuffSkill : SkillData
{
    private void OnEnable()
    {
        skillName = "Power Buff";
        description = "Tăng sức mạnh cho một đồng minh";
        targetType = TargetType.Ally;
        targetRadius = 2f;
        rageCost = 50f;
        cooldown = 10f;
        buffAmount = 1.5f;
        buffDuration = 5f;
    }
} 
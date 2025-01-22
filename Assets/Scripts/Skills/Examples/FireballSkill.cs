using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Game/Skills/Fireball")]
public class FireballSkill : SkillData
{
    private void OnEnable()
    {
        skillName = "Fireball";
        description = "Gây sát thương diện rộng cho kẻ địch";
        targetType = TargetType.AOE;
        targetRadius = 2f;
        manaCost = 60f;
        damage = 50f;
    }
}
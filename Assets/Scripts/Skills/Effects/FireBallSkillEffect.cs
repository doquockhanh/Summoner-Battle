using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSkillEffect : MonoBehaviour, ISkillEffect
{
    private HexCell targetCell;
    private FireballSkill skillData;

    public void Initialize(HexCell targetCell, FireballSkill skillData)
    {
        this.targetCell = targetCell;
        this.skillData = skillData;
    }

    public void Execute(Vector3 _)
    {
        if (!ValidateExecution()) return;

        StartCoroutine(FireballCoroutine(targetCell, skillData, skillData.ownerCard.IsPlayer));
    }

    private bool ValidateExecution()
    {
        if (targetCell == null || skillData == null)
        {
            Debug.LogError("HealingSkill: Invalid setup");
            return false;
        }
        return true;
    }

    private IEnumerator FireballCoroutine(HexCell targetCell, FireballSkill skill, bool isFromPlayer)
    {
        // Hiển thị vòng tròn AOE và lưu ID
        int indicatorId = SkillEffectHandler.Instance.ShowRangeIndicator(targetCell, skill.effectRadius);

        // Gây sát thương và áp dụng hiệu ứng thiêu đốt
        List<Unit> enemies = HexGrid.Instance.GetUnitsInRange(targetCell.Coordinates, skill.effectRadius, !isFromPlayer);
        foreach (Unit enemy in enemies)
        {
            if (enemy != null)
            {
                // Gây sát thương phép
                float magicDamage = skill.ownerCard.Unit.magicDamage *
                                  (skill.magicDamagePercent / 100f);
                enemy.TakeDamage(magicDamage, DamageType.Magic);

                // Áp dụng hiệu ứng thiêu đốt
                var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                if (statusEffects != null)
                {
                    var burningEffect = new BurningEffect(
                        enemy,
                        skill.burnDuration,
                        skill.burnDamagePercent,
                        skill.healingReduction
                    );
                    statusEffects.AddEffect(burningEffect);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        // Xóa đúng vòng tròn AOE theo ID
        SkillEffectHandler.Instance.HideRangeIndicator(indicatorId);
        Cleanup();
    }

    public void Cleanup()
    {
        Destroy(this);
    }
}

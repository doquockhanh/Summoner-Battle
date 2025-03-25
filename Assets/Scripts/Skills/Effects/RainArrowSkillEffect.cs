using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainArrowSkillEffect : MonoBehaviour, ISkillEffect
{
    private HexCell targetCell;
    private RainArrowSkill skillData;
    private GameObject rainArrowEffectPrefab;
    private bool isPlayer;

    public void Initialize(HexCell targetCell, RainArrowSkill skillData, GameObject rainArrowEffectPrefab, bool isPlayer)
    {
        this.targetCell = targetCell;
        this.skillData = skillData;
        this.rainArrowEffectPrefab = rainArrowEffectPrefab;
        this.isPlayer = isPlayer;
    }

    public void Execute(Vector3 _)
    {
        if (!ValidateExecution()) return;

        StartCoroutine(RainArrowCoroutine(targetCell, skillData, rainArrowEffectPrefab, isPlayer));
    }

    private bool ValidateExecution()
    {
        if (targetCell == null || skillData == null)
        {
            Debug.LogError("RainArrowSkill: Invalid setup");
            return false;
        }
        return true;
    }

    private IEnumerator RainArrowCoroutine(HexCell targetCell, RainArrowSkill skill, GameObject rainArrowEffectPrefab, bool isPlayer)
    {
        // Hiển thị vòng tròn AOE và lưu ID
        int indicatorId = SkillEffectHandler.Instance
                            .ShowRangeIndicator(targetCell, HexMetrics.GridToWorldRadius(skill.effectRadius));

        // Tạo hiệu ứng mưa tên với callback
        GameObject effectObj = Instantiate(rainArrowEffectPrefab);
        RainArrowEffect effect = effectObj.GetComponent<RainArrowEffect>();
        if (effect != null)
        {
            effect.Initialize(skill, targetCell, () =>
            {
                List<Unit> enemies = HexGrid.Instance.GetUnitsInRange(targetCell.Coordinates, skill.effectRadius, !isPlayer);

                foreach (Unit enemy in enemies)
                {
                    if (enemy != null)
                    {
                        float damage = skill.ownerCard.Unit.physicalDamage *
                                     (skill.damagePerWavePercent / 100f);
                        enemy.TakeDamage(damage, DamageType.Physical);
                    }
                }
            });
        }

        yield return new WaitForSeconds(1.1f);
        // Xóa đúng vòng tròn AOE theo ID
        SkillEffectHandler.Instance.HideRangeIndicator(indicatorId);
        Cleanup();
    }

    public void Cleanup()
    {
        StopAllCoroutines();
        Destroy(this);
    }
}
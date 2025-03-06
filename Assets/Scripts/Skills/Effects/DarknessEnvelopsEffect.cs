using UnityEngine;
using System.Collections;

public class DarknessEnvelopsEffect : MonoBehaviour, ISkillEffect
{
    private Unit target;
    private DarknessEnvelopsSkill skillData;
    private float damageTimer;
    private UnitStats stats;
    private float originalMagicResist;

    public void Initialize(Unit target, DarknessEnvelopsSkill skillData)
    {
        this.target = target;
        this.skillData = skillData;
        this.stats = target.GetUnitStats();
        this.damageTimer = 0f;
        
        if (stats != null)
        {
            originalMagicResist = stats.GetMagicResist();
            // Giảm kháng phép
            stats.ModifyMagicResist(-skillData.magicResistReduction);
        }
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        // Tạo hiệu ứng visual
        if (skillData.darknessPrefab != null)
        {
            GameObject effect = Instantiate(
                skillData.darknessPrefab,
                target.transform.position,
                Quaternion.identity,
                target.transform
            );
            Destroy(effect, skillData.activeDuration);
        }

        // Bắt đầu gây sát thương theo thời gian
        StartCoroutine(DamageOverTimeCoroutine());
    }

    private IEnumerator DamageOverTimeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < skillData.activeDuration)
        {
            if (target == null || target.IsDead) break;

            // Gây sát thương cơ bản + % máu tối đa
            float damage = skillData.baseDamage + 
                         (stats.MaxHp * skillData.maxHealthDamagePercent / 100f);
            target.TakeDamage(damage, DamageType.Magic);

            elapsedTime += 1f;
            yield return new WaitForSeconds(1f);
        }

        Cleanup();
    }

    private bool ValidateExecution()
    {
        if (target == null || skillData == null)
        {
            Debug.LogError("DarknessEnvelops: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        if (stats != null)
        {
            // Khôi phục kháng phép
            stats.ModifyMagicResist(originalMagicResist);
        }
        Destroy(this);
    }
} 
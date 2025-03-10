using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class HolyAuraEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private HolyAuraSkill skillData;
    private HashSet<Unit> protectedAllies;
    private float auraTimer;
    private UnitTargeting unitTargeting;

    public void Initialize(Unit caster, HolyAuraSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.protectedAllies = new HashSet<Unit>();
        this.auraTimer = 5f; // Thời gian tồn tại hào quang

    }

    public void Execute(Vector3 targetPos)
    {
        unitTargeting = caster.GetComponent<UnitTargeting>();
        if (!ValidateExecution()) return;

        // Tăng chỉ số vĩnh viễn cho caster
        ApplyPermanentBuffs();

        // Tạo hiệu ứng hào quang
        if (skillData.auraEffectPrefab != null)
        {
            GameObject auraEffect = Instantiate(
                skillData.auraEffectPrefab,
                caster.transform.position,
                Quaternion.identity,
                caster.transform
            );
            Destroy(auraEffect, auraTimer);
        }

        // Đăng ký sự kiện nhận sát thương.
        UnitStats.OnModifyRawDamage += HandleAuraProtection;

        // Bắt đầu theo dõi đồng minh trong tầm
        this.StartCoroutineSafely(FindAlliesInRange());
    }

    private IEnumerator FindAlliesInRange()
    {
        while (true)
        {
            protectedAllies.Clear();
            protectedAllies = unitTargeting.GetUnitsInRange(skillData.auraRadius).ToHashSet();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private float HandleAuraProtection(float rawDamage, Unit source, Unit target, DamageType damageType)
    {
        if (protectedAllies.Contains(source))
        {
            float damageCanDeal = target.GetUnitStats().CalculateFinalDamage(rawDamage, damageType);
            float healing = damageCanDeal * (skillData.damageSharePercent / 100f);
            caster.GetUnitStats().Heal(healing);
        }

        if (protectedAllies.Contains(target))
        {
            float sharedDamage = rawDamage * (skillData.damageSharePercent / 100f);
            caster.TakeDamage(sharedDamage, damageType);
            return rawDamage - sharedDamage;
        }
        else
        {
            return rawDamage;
        }
    }

    private void ApplyPermanentBuffs()
    {
        var stats = caster.GetUnitStats();
        float boost = skillData.permanentStatsBoost / 100f;

        // Tăng máu tối đa và hồi máu
        float maxHpIncrease = stats.GetMaxHp() * boost;
        stats.ModifyMaxHp(maxHpIncrease);
        stats.Heal(maxHpIncrease);

        // Tăng giáp và kháng phép
        stats.ModifyArmor(stats.GetArmor() * boost);
        stats.ModifyMagicResist(stats.GetMagicResist() * boost);
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null || unitTargeting == null)
        {
            Debug.LogError("HolyAuraEffect: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        UnitStats.OnModifyRawDamage -= HandleAuraProtection;
        Destroy(this);
    }
}
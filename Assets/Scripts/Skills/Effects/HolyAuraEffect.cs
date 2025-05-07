using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class HolyAuraEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private HolyAuraSkill skillData;
    private HashSet<Unit> protectedAllies;
    private UnitTargeting unitTargeting;

    public void Initialize(Unit caster, HolyAuraSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.protectedAllies = new HashSet<Unit>();
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
            Destroy(auraEffect, skillData.auraTimer);
        }

        // Đăng ký sự kiện nhận sát thương.
        UnitStats.OnModifyRawDamage += HandleAuraProtection;

        // Bắt đầu theo dõi đồng minh trong tầm
        this.StartCoroutineSafely(FindAlliesInRange());
    }

    private IEnumerator FindAlliesInRange()
    {
        float timer = 0f;
        while (true)
        {
            protectedAllies.Clear();
            protectedAllies = HexGrid.Instance.GetUnitsInRange(caster.OccupiedCell.Coordinates, skillData.auraRadius, caster.IsPlayerUnit).ToHashSet();
            protectedAllies.Remove(caster);

            float checkInterval = 0.5f;
            yield return new WaitForSeconds(checkInterval);
            timer += checkInterval;
            if (timer > skillData.auraTimer)
            {
                Cleanup();
                yield break;
            }
        }
    }

    private float HandleAuraProtection(float rawDamage, Unit source, Unit target, DamageType damageType)
    {
        // ko take dame/heal bởi nguồn của bản thân
        // ko share qua lại damageType == DamageType.SharedDamage
        if (source == caster || target == caster || damageType == DamageType.SharedDamage || damageType == DamageType.SelfExplore) return rawDamage; 

        if (protectedAllies.Contains(source))
        {
            float damageCanDeal = target.GetUnitStats().CalculateFinalDamage(rawDamage, damageType);
            float healing = damageCanDeal * (skillData.damageSharePercent / 100f);
            caster.GetUnitStats().Heal(healing);
        }

        if (target != caster && protectedAllies.Contains(target))
        {
            float sharedDamage = rawDamage * (skillData.damageSharePercent / 100f);
            caster.TakeDamage(sharedDamage, DamageType.SharedDamage);
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
        // giới hạn 500 cho mỗi lần
        stats.ModifyStat(StatType.MaxHp, Mathf.Min(maxHpIncrease, 500));
        stats.Heal(maxHpIncrease);

        // Tăng giáp và kháng phép
        stats.ModifyStat(StatType.Armor, stats.GetArmor() * boost);
        stats.ModifyStat(StatType.MagicResist, stats.GetMagicResist() * boost);
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
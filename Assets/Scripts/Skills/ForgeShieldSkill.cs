using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ForgeShield", menuName = "Game/Skills/ForgeShield")]
public class ForgeShieldSkill : Skill
{
    [Header("Thông số khiên")]
    [Range(0f, 100f)]
    [Tooltip("Phần trăm máu tối đa làm khiên (30% = 30)")]
    public float shieldHealthPercent = 30f;

    [Range(0f, 100f)]
    [Tooltip("Phần trăm khiên chia sẻ (50% = 50)")]
    public float sharedShieldPercent = 50f;

    [Range(1f, 10f)]
    public float shareRadius = 3f;

    [Header("Hiệu ứng")]
    public GameObject forgeEffectPrefab;
    public GameObject shieldEffectPrefab;

    private int shieldID;
    private Unit strongestSmith;
    private ShieldLayer shield;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Không sử dụng vì đây là kỹ năng OnSummon
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null) return;

        // Tìm thợ rèn mạnh nhất dựa trên chỉ số phòng thủ
        strongestSmith = ownerCard.GetActiveUnits().OrderBy(unit => CalculateUnitScore(unit)).FirstOrDefault();
        if (strongestSmith == null) return;
        strongestSmith.GetComponent<UnitView>().PlaySkillAnimation();

        // Tính lượng khiên dựa trên máu tối đa
        float shieldAmount = strongestSmith.GetUnitStats().GetMaxHp() * (shieldHealthPercent / 100f);

        // Áp dụng khiên sharing
        if (SkillEffectHandler.Instance != null)
        {
            shield = new ShieldLayer(shieldAmount, duration, strongestSmith);
            strongestSmith.GetUnitStats().AddShield(shield);
            shield.OnShieldBroken += HandleShareShield;
            shield.OnShieldExpired += HandleShareShield;
            shieldID = shield.GetOwnerSkillID();
            ownerCard.OnSkillActivated();

            // Tạo hiệu ứng rèn
            if (forgeEffectPrefab != null)
            {
                GameObject effect = Instantiate(forgeEffectPrefab,
                    strongestSmith.transform.position,
                    Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            ownerCard.OnSkillFailed();
        }
    }

    private void HandleShareShield(int id)
    {
        shield.OnShieldBroken -= HandleShareShield;
        shield.OnShieldExpired -= HandleShareShield;

        if (id == shieldID)
        {
            List<Unit> allies = BattleManager.Instance
                                .GetAllUnitInteam(ownerCard.IsPlayer)
                                .Where(ally => strongestSmith.GetComponent<UnitTargeting>().IsValidAlly(ally)
                                                    && ally != strongestSmith)
                                .OrderBy(ally => strongestSmith.OccupiedCell.Coordinates.DistanceTo(ally.OccupiedCell.Coordinates))
                                .Take(2)
                                .ToList();

            foreach (var ally in allies)
            {
                if (ally != null)
                {
                    float shieldAmount = strongestSmith.GetUnitStats().GetMaxHp() * (shieldHealthPercent / 100f) * (sharedShieldPercent / 100f);
                    ally.GetUnitStats().AddShield(shieldAmount, duration);
                }
            }
        }
    }

    private float CalculateUnitScore(Unit unit)
    {
        if (unit == null || unit.IsDead) return -1;

        float score = 0;

        // 1. Unit còn sống (điều kiện bắt buộc, đã check ở trên)
        score += 1;

        // 2. Độ gần với 60% máu
        var stats = unit.GetUnitStats();
        float healthPercent = stats.CurrentHP / stats.GetMaxHp();
        float healthScore = 1 - Mathf.Abs(60f / 100f - healthPercent);
        score += healthScore;

        // 3. Đang tấn công đối phương
        UnitTargeting targeting = unit.GetComponent<UnitTargeting>();

        if (targeting != null && targeting.CurrentTarget != null && targeting.IsInAttackRange(targeting.CurrentTarget))
        {
            score += 1;
        }

        return score;
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
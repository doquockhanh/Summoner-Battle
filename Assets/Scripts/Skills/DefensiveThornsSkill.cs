using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DefensiveThorns", menuName = "Game/Skills/DefensiveThorns")]
public class DefensiveThornsSkill : Skill
{
    [Header("Cài đặt Gai Phòng Thủ")]
    [Range(0f, 100f)]
    [Tooltip("Giảm sát thương (50% = 50)")]
    public float damageReduction = 50f;

    [Range(0f, 100f)]
    [Tooltip("Phần trăm sát thương phản lại (50% = 50)")]
    public float thornsDamagePercent = 50f;

    [Range(0f, 10f)]
    public int tauntRadius = 3;

    [Range(0f, 10f)]
    public float thornsDuration = 3f;

    [Header("Hiệu ứng")]
    public GameObject thornsEffectPrefab;
    public GameObject tauntEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // not used
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null) return;

        // Tìm unit mạnh nhất dựa trên chỉ số phòng thủ
        Unit strongestUnit = ownerCard.GetActiveUnits()
            .Select(unit => new
            {
                Unit = unit,
                Score = CalculateUnitScore(unit)
            })
            .OrderByDescending(x => x.Score)
            .First()
            .Unit;

        if (strongestUnit == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        var effect = strongestUnit.gameObject.AddComponent<DefensiveThornsSkillEffect>();
        effect.Initialize(strongestUnit, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
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
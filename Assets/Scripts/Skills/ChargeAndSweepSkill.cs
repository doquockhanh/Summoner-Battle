using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "ChargeAndSweep", menuName = "Game/Skills/ChargeAndSweep")]
public class ChargeAndSweepSkill : Skill
{
    [Header("Thông số Xung Phong")]
    public float chargeDamageMultiplier = 1.2f;
    public float pullbackDistance = 1f;
    public float sweepRadius = 1.5f;
    public float lifestealPercent = 20f;
    public int sweepAttackCount = 2;

    public GameObject chargeEffectPrefab;
    public GameObject sweepEffectPrefab;

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

        // Tìm kỵ binh mạnh nhất
        Unit strongestUnit = ownerCard.GetActiveUnits()
            .Select(unit => new { Unit = unit, Score = CalculateUnitScore(unit) })
            .OrderByDescending(x => x.Score)
            .First()
            .Unit;

        if (strongestUnit == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Thêm effect xử lý kỹ năng
        var effect = strongestUnit.gameObject.AddComponent<ChargeAndSweepEffect>();
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
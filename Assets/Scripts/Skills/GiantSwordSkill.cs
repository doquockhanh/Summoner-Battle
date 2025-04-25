using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UndeadSwordSummon", menuName = "Game/Skills/UndeadSwordSummon")]
public class GiantSwordSkill : Skill
{
    public int skillRange = 2;
    public float mainTargetPercent = 200f;
    public float areaPercent = 100f;
    public float knockUpDuration = 2f;
    public int maxDashDistance = 7;
    public float dashSpeed = 30f;
    private Unit strongestUnit;
    private Vector3 targetPos;
    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null || ownerCard.GetActiveUnits().Count == 0)
        {
            Debug.Log("undead: ownerCard is null || no active unit");
            ownerCard.OnSkillFailed();
            return;
        }

        List<Unit> activeUnits = ownerCard.GetActiveUnits();

        strongestUnit = activeUnits.Select(unit => new
        {
            Unit = unit,
            Score = CalculateUnitScore(unit)
        })
        .OrderByDescending(x => x.Score)
        .FirstOrDefault()
        ?.Unit;

        if (strongestUnit == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        ownerCard.OnSkillActivated();
        UndeadSwordEffect effect = strongestUnit.gameObject.AddComponent<UndeadSwordEffect>();
        effect.Initialize(strongestUnit, this);
        effect.Execute(targetPos);
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        throw new System.NotImplementedException();
    }

    private float CalculateUnitScore(Unit unit)
    {
        if (unit == null || unit.IsDead) return -1;

        float score = 0;

        // 2. Độ gần với 60% máu
        var stats = unit.GetUnitStats();
        float healthPercent = stats.CurrentHP / stats.GetMaxHp();
        float healthScore = 1 - Mathf.Abs(60f / 100f - healthPercent);
        score += healthScore;

        // 3. Đang tấn công đối phương
        UnitTargeting targeting = unit.GetComponent<UnitTargeting>();
        if (targeting != null && targeting.CurrentTarget != null && targeting.IsInAttackRange(targeting.CurrentTarget))
        {
            score += 2;
        }

        return score;
    }
}

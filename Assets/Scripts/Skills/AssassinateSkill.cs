using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Assassinate", menuName = "Game/Skills/Assassinate")]
public class AssassinateSkill : Skill
{
    [Header("Cài đặt Assassinate")]
    public int jumpRange = 4;
    public float untargetableDuration = 1f;

    private Unit assassin;
    private Unit target;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {

    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null || ownerCard.GetActiveUnits().Count <= 0) return;

        // Tìm assassin mạnh nhất
        assassin = ownerCard.GetActiveUnits()
            .Select(unit => new
            {
                Unit = unit,
                Score = CalculateUnitScore(unit)
            })
            .OrderByDescending(x => x.Score)
            .First()
            .Unit;

        if (assassin == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Tìm mục tiêu yếu nhất trong tầm
        target = FindWeakestTargetInRange(assassin.OccupiedCell.Coordinates, jumpRange);
        if (target == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        ownerCard.OnSkillActivated();
        var effect = assassin.gameObject.AddComponent<AssassinateSkillEffect>();
        effect.Initialize(assassin, target, this);
        effect.Execute(Vector3.zero);
    }

    private Unit FindWeakestTargetInRange(HexCoord center, int range)
    {
        Unit weakest = null;
        float lowestHealth = 10000;

        List<Unit> enemies = HexGrid.Instance.GetUnitsInRange(center, range, !ownerCard.IsPlayer);
        foreach (Unit enemy in enemies)
        {
            if (enemy != null)
            {
                float health = enemy.GetCurrentHP();
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    weakest = enemy;
                }
            }
        }

        return weakest;
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
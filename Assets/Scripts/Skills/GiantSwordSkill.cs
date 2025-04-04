using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UndeadSwordSummon", menuName = "Game/Skills/UndeadSwordSummon")]

public class GiantSwordSkill : Skill
{
  public int skillRange = 2;
  public float mainTargetPercent = 200f;
  public float areaPercent = 100f;

  private Unit strongestUnit;
  private Vector3 targetPos;
  public override void ApplyPassive(Unit summonedUnit)
  {
    throw new System.NotImplementedException();
  }

  public override void ApplyToSummon(Unit summonedUnit)
  {
    if (ownerCard == null)
    {
      Debug.Log("ChargeSkill: ownerCard is null!");
      return;
    }

    List<Unit> activeUnits = ownerCard.GetActiveUnits();
    if (activeUnits.Count == 0)
    {
      ownerCard.OnSkillFailed();
      return;
    }

    strongestUnit = activeUnits.Select(unit => new
    {
      Unit = unit,
      Score = CalculateUnitScore(unit)
    })
    .OrderByDescending(x => x.Score)
    .FirstOrDefault()
    .Unit;

    if (strongestUnit != null)
    {
      UnitTargeting targeting = strongestUnit.GetComponent<UnitTargeting>();
      if (targeting == null || targeting.CurrentTarget == null)
      {
        ownerCard.OnSkillFailed();
        return;
      }
      else
      {
        if (targeting.CurrentTarget.OccupiedCell != null)
          targetPos = targeting.CurrentTarget.OccupiedCell.WorldPosition;
        else
          targetPos = targeting.CurrentTarget.transform.position;
      }

      GrowSizeEffect growSizeEffect = new(strongestUnit, 5f, 1.3f);
      strongestUnit.GetComponent<UnitView>().PlaySkillAnimation(CastSkill);
      strongestUnit.GetComponent<UnitStatusEffects>().AddEffect(growSizeEffect);
      ownerCard.OnSkillActivated();
    }
  }

  public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
  {
    throw new System.NotImplementedException();
  }

  public void CastSkill()
  {
    var effect = strongestUnit.gameObject.AddComponent<UndeadSwordEffect>();
    effect.Initialize(strongestUnit, this);
    effect.Execute(targetPos);
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FuriousCavalryCharge", menuName = "Game/Skills/FuriousCavalryCharge")]
public class FuriousCavalryCharge : Skill
{
    [Header("Charge Properties")]
    public float chargeSpeed = 10f;
    public float damageMultiplier = 1f;
    public float knockupDuration = 1f;
    public float shieldPercent = 30f;
    public float shieldDuration = 5f;
    public float lifestealPercent = 20f;

    private Unit strongestUnit;

    public GameObject chargeEffectPrefab;
    public GameObject hitEffectPrefab;

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // not use
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

        // Tìm unit phù hợp nhất dựa trên điểm số
        strongestUnit = activeUnits
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
            Debug.Log("CalculateUnitScore find strongest unit is wrong!");
            return;
        }

        Vector3 bestTargetPos = FindBestTargetPosition(strongestUnit);

        if (bestTargetPos == Vector3.zero)
        {
            Debug.Log("FindBestTargetPosition can find target position!");
            return;
        }

        var effect = strongestUnit.gameObject.AddComponent<FuriousCavalryChargeEffect>();
        effect.Initialize(strongestUnit, this);
        effect.Execute(bestTargetPos);
        ownerCard.OnSkillActivated();
    }

    private Vector3 FindBestTargetPosition(Unit caster)
    {
        if (caster == null) return Vector3.zero;

        try
        {
            // Tìm tất cả enemy trong tầm radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
            float maxDistance = 0f;
            Vector3 targetPos = Vector3.zero;

            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                Unit enemy = hit.GetComponent<Unit>();
                if (enemy != null && enemy.IsPlayerUnit != caster.IsPlayerUnit)
                {
                    float distance = Vector3.Distance(caster.transform.position, enemy.transform.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        targetPos = enemy.transform.position;
                    }
                }
            }

            return targetPos;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error finding target position: {e.Message}");
            return Vector3.zero;
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
        float healthPercent = stats.CurrentHP / stats.MaxHp;
        float healthScore = 1 - Mathf.Abs(60f / 100f - healthPercent);
        score += healthScore;

        // 3. Đang tấn công đối phương
        UnitTargeting targeting = unit.GetComponent<UnitTargeting>();

        if (targeting != null && targeting.CurrentTarget != null && targeting.IsInRange(targeting.CurrentTarget))
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
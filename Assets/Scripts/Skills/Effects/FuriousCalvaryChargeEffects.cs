using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuriousCavalryChargeEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private FuriousCavalryCharge skillData;
    private HashSet<Unit> hitUnits;
    private Vector3 targetPosition;

    public void Initialize(Unit caster, FuriousCavalryCharge skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        hitUnits = new HashSet<Unit>();
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        targetPosition = targetPos;

        ApplyInitialEffects();
        caster.GetComponent<UnitTargeting>().autoTargeting = false;
        this.StartCoroutineSafely(ChargeCoroutine());
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("FuriousCavalryCharge: Invalid setup");
            return false;
        }
        return true;
    }

    private void ApplyInitialEffects()
    {
        // Áp dụng shield và lifesteal
        float shieldAmount = caster.GetUnitStats().GetMaxHp() * (skillData.shieldPercent / 100f);
        caster.GetUnitStats().AddShield(shieldAmount, skillData.shieldDuration);
        caster.GetUnitStats().ModifyStat(StatType.LifeSteal, skillData.lifestealPercent);

        // Hiệu ứng bắt đầu lao
        if (skillData.chargeEffectPrefab != null)
        {
            GameObject chargeEffect = Instantiate(skillData.chargeEffectPrefab,
                caster.transform.position, Quaternion.identity);
            Destroy(chargeEffect, 1f);
        }
    }

    private IEnumerator ChargeCoroutine()
    {
        bool faceRight = targetPosition.x > caster.transform.position.x;
        caster.GetComponent<UnitView>().FlipSprite(faceRight);

        Vector3 startPos = caster.transform.position;
        Vector3 direction = (targetPosition - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPosition);
        float chargeTime = distance / skillData.chargeSpeed;
        float elapsedTime = 0f;

        HashSet<Unit> hitUnits = new HashSet<Unit>();

        while (elapsedTime < chargeTime)
        {
            caster.transform.position += direction * skillData.chargeSpeed * Time.deltaTime;

            HexCell cell = HexGrid.Instance.GetCellAtPosition(caster.transform.position);
            Unit hit = cell.OccupyingUnit;

            if (hit != null && hit.IsPlayerUnit != caster.IsPlayerUnit && !hitUnits.Contains(hit))
            {
                hitUnits.Add(hit);
                // Gây sát thương
                float damage = caster.GetUnitStats().GetPhysicalDamage() * skillData.damageMultiplier;
                hit.TakeDamage(damage, DamageType.Physical, caster);

                // Hiệu ứng va chạm
                if (skillData.hitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(skillData.hitEffectPrefab, hit.transform.position, Quaternion.identity);
                    Destroy(hitEffect, 0.5f);
                }

                // Áp dụng hiệu ứng knockup thông qua status effect system
                var statusEffects = hit.GetComponent<UnitStatusEffects>();
                if (statusEffects != null)
                {
                    var knockupEffect = new KnockupEffect(hit, skillData.knockupDuration);
                    statusEffects.AddEffect(knockupEffect);
                }
            }


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // Resume targeting
        caster.GetComponent<UnitTargeting>().autoTargeting = true;
        HexGrid.Instance.OccupyCell(HexGrid.Instance.GetCellAtPosition(caster.transform.position), caster);
        //    HexGrid.Instance.AssertOccupyCell(caster.transform.position, caster, 3);

        // Cleanups
        Cleanup();
    }

    public void Cleanup()
    {
        hitUnits.Clear();
        Destroy(this);
    }
}
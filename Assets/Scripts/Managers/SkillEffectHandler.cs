using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillEffectHandler : MonoBehaviour
{
    public static SkillEffectHandler Instance { get; private set; }

    [Header("Hiệu ứng")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject chargeEffectPrefab;
    [SerializeField] private GameObject rangeIndicatorPrefab;
    [SerializeField] private GameObject rainArrowEffectPrefab;

    private GameObject currentRangeIndicator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HandleChargeSkill(Unit caster, Vector3 targetPos, FuriousCavalryCharge skill)
    {
        if (caster == null)
        {
            Debug.LogError("HandleChargeSkill: caster is null!");
            return;
        }

        StartCoroutine(ChargeCoroutine(caster, targetPos, skill));
    }

    private IEnumerator ChargeCoroutine(Unit caster, Vector3 targetPos, FuriousCavalryCharge skill)
    {
        // Áp dụng shield và lifesteal cho caster
        caster.GetUnitStats().ApplyShield(skill.shieldPercent, skill.shieldDuration);
        caster.GetUnitStats().SetLifesteal(skill.lifestealPercent, skill.shieldDuration);

        bool faceRight = targetPos.x > caster.transform.position.x;
        caster.GetComponent<UnitView>().FlipSprite(faceRight);

        // Hiệu ứng bắt đầu lao
        if (chargeEffectPrefab != null)
        {
            GameObject chargeEffect = Instantiate(chargeEffectPrefab, caster.transform.position, Quaternion.identity);
            Destroy(chargeEffect, 1f);
        }

        Vector3 startPos = caster.transform.position;
        Vector3 direction = (targetPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPos);
        float chargeTime = distance / skill.chargeSpeed;
        float elapsedTime = 0f;

        HashSet<Unit> hitUnits = new HashSet<Unit>();

        while (elapsedTime < chargeTime)
        {
            caster.transform.position += direction * skill.chargeSpeed * Time.deltaTime;

            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, 0.5f);
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                Unit enemy = hit.GetComponent<Unit>();
                if (enemy != null && enemy.IsPlayerUnit != caster.IsPlayerUnit && !hitUnits.Contains(enemy))
                {
                    hitUnits.Add(enemy);

                    // Gây sát thương
                    float damage = caster.GetUnitStats().GetModifiedDamage() * skill.damageMultiplier;
                    enemy.TakeDamage(damage);

                    // Hiệu ứng va chạm
                    if (hitEffectPrefab != null)
                    {
                        GameObject hitEffect = Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                        Destroy(hitEffect, 0.5f);
                    }

                    // Áp dụng hiệu ứng knockup thông qua status effect system
                    var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                    if (statusEffects != null)
                    {
                        var knockupEffect = new KnockupEffect(enemy, skill.knockupDuration);
                        statusEffects.AddEffect(knockupEffect);
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo caster đến đúng vị trí cuối
        caster.transform.position = targetPos;
        UnitTargeting unitTargeting = caster.GetComponent<UnitTargeting>();
        Unit unit = unitTargeting.FindNearestTarget();
        unitTargeting.AssignTarget(unit);
    }

    public void HandleRainArrowSkill(Vector3 targetPos, RainArrowSkill skill)
    {
        StartCoroutine(RainArrowCoroutine(targetPos, skill));
    }

    private IEnumerator RainArrowCoroutine(Vector3 targetPos, RainArrowSkill skill)
    {
        // Hiển thị vòng tròn AOE
        ShowRangeIndicator(targetPos, skill.effectRadius);

        float totalDamage = 0;
        // Tạo hiệu ứng mưa tên với callback
        GameObject effectObj = Instantiate(rainArrowEffectPrefab);
        RainArrowEffect effect = effectObj.GetComponent<RainArrowEffect>();
        if (effect != null)
        {
            effect.Initialize(skill, targetPos, (hitPos) =>
            {
                ApplyDamageAtPosition(hitPos, skill, ref totalDamage);
            });
        }

        yield return new WaitForSeconds(1.1f);
        // Xóa vòng tròn AOE
        HideRangeIndicator();
    }

    private void ApplyDamageAtPosition(Vector3 position, RainArrowSkill skill, ref float totalDamage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, skill.effectRadius);
        int hitCount = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Unit enemy = hit.GetComponent<Unit>();
            if (enemy != null && enemy.IsPlayerUnit != skill.ownerCard.IsPlayer)
            {
                float damage = enemy.GetUnitStats().GetModifiedDamage() *
                             (skill.damagePerWavePercent / 100f);
                enemy.TakeDamage(damage);
                totalDamage += damage;
                hitCount++;
            }
        }
    }

    private void ShowRangeIndicator(Vector3 position, float radius)
    {
        if (currentRangeIndicator != null)
        {
            Destroy(currentRangeIndicator);
        }

        currentRangeIndicator = Instantiate(rangeIndicatorPrefab, position, Quaternion.identity);
        SkillRangeIndicator indicator = currentRangeIndicator.GetComponent<SkillRangeIndicator>();
        if (indicator != null)
        {
            indicator.SetRadius(radius);
            indicator.SetColor(Color.red);
        }
    }

    private void HideRangeIndicator()
    {
        if (currentRangeIndicator != null)
        {
            Destroy(currentRangeIndicator);
            currentRangeIndicator = null;
        }
    }
}
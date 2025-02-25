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

    private Dictionary<int, GameObject> activeRangeIndicators = new Dictionary<int, GameObject>();
    private int nextIndicatorId = 0;

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
        caster.GetUnitStats().AddShield(skill.shieldPercent, skill.shieldDuration);

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
                    float damage = caster.GetUnitStats().GetPhysicalDamage() * skill.damageMultiplier;
                    enemy.TakeDamage(damage, DamageType.Physical, caster);

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

    public void HandleRainArrowSkill(Vector3 targetPos, RainArrowSkill skill, bool isFromPlayer)
    {
        StartCoroutine(RainArrowCoroutine(targetPos, skill, isFromPlayer));
    }

    private IEnumerator RainArrowCoroutine(Vector3 targetPos, RainArrowSkill skill, bool isFromPlayer)
    {
        // Hiển thị vòng tròn AOE và lưu ID
        int indicatorId = ShowRangeIndicator(targetPos, skill.effectRadius);

        // Tạo hiệu ứng mưa tên với callback
        GameObject effectObj = Instantiate(rainArrowEffectPrefab);
        RainArrowEffect effect = effectObj.GetComponent<RainArrowEffect>();
        if (effect != null)
        {
            effect.Initialize(skill, targetPos, (hitPos) =>
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, skill.effectRadius);

                foreach (Collider2D hit in hits)
                {
                    if (hit == null) continue;

                    Unit enemy = hit.GetComponent<Unit>();
                    if (enemy != null && enemy.IsPlayerUnit != isFromPlayer)
                    {
                        float damage = skill.ownerCard.Unit.physicalDamage *
                                     (skill.damagePerWavePercent / 100f);
                        enemy.TakeDamage(damage, DamageType.Physical);
                    }
                }
            });
        }

        yield return new WaitForSeconds(1.1f);
        // Xóa đúng vòng tròn AOE theo ID
        HideRangeIndicator(indicatorId);
    }


    public void HandleFireballSkill(Vector3 targetPos, FireballSkill skill, bool isFromPlayer)
    {
        StartCoroutine(FireballCoroutine(targetPos, skill, isFromPlayer));
    }

    private IEnumerator FireballCoroutine(Vector3 targetPos, FireballSkill skill, bool isFromPlayer)
    {
        // Hiển thị vòng tròn AOE và lưu ID
        int indicatorId = ShowRangeIndicator(targetPos, skill.effectRadius);

        // Tạo hiệu ứng cầu lửa bay đến
        if (skill.fireballEffectPrefab != null)
        {
            GameObject fireballEffect = Instantiate(
                skill.fireballEffectPrefab,
                skill.ownerCard.transform.position,
                Quaternion.identity
            );

            // Animation cầu lửa bay đến mục tiêu
            float flightTime = 0.5f;
            float elapsedTime = 0f;
            Vector3 startPos = fireballEffect.transform.position;

            while (elapsedTime < flightTime)
            {
                fireballEffect.transform.position = Vector3.Lerp(
                    startPos,
                    targetPos,
                    elapsedTime / flightTime
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Destroy(fireballEffect);
        }

        // Gây sát thương và áp dụng hiệu ứng thiêu đốt
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, skill.effectRadius);
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (enemy != null && enemy.IsPlayerUnit != isFromPlayer)
            {
                // Gây sát thương phép
                float magicDamage = skill.ownerCard.Unit.magicDamage *
                                  (skill.magicDamagePercent / 100f);
                enemy.TakeDamage(magicDamage, DamageType.Magic);

                // Áp dụng hiệu ứng thiêu đốt
                var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                if (statusEffects != null)
                {
                    var burningEffect = new BurningEffect(
                        enemy,
                        skill.burnDuration,
                        skill.burnDamagePercent,
                        skill.healingReduction
                    );
                    statusEffects.AddEffect(burningEffect);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        // Xóa đúng vòng tròn AOE theo ID
        HideRangeIndicator(indicatorId);
    }

    public void HandleGuardianAuraSkill(Unit caster, GuardianAuraSkill skill)
    {
        StartCoroutine(GuardianAuraCoroutine(caster, skill));
    }

    private IEnumerator GuardianAuraCoroutine(Unit caster, GuardianAuraSkill skill)
    {
        // Hiển thị vòng tròn AOE
        int indicatorId = ShowRangeIndicator(caster.transform.position, skill.auraRadius, Color.cyan);

        // Tạo hiệu ứng visual
        if (skill.auraEffectPrefab != null)
        {
            GameObject auraEffect = Instantiate(
                skill.auraEffectPrefab,
                caster.transform.position,
                Quaternion.identity,
                caster.transform
            );
            Destroy(auraEffect, skill.duration);
        }

        // Áp dụng buff cho caster và allies
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, skill.auraRadius);
        foreach (Collider2D hit in hits)
        {
            Unit ally = hit.GetComponent<Unit>();
            if (ally != null && ally.IsPlayerUnit == caster.IsPlayerUnit)
            {
                var statusEffects = ally.GetComponent<UnitStatusEffects>();
                if (statusEffects != null)
                {
                    var auraEffect = new GuardianAuraEffect(
                        ally,
                        skill.duration,
                        skill.armorBoost,
                        skill.magicResistBoost
                    );
                    statusEffects.AddEffect(auraEffect);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        HideRangeIndicator(indicatorId);
    }

    public void HandleAssassinateSkill(Unit assassin, Unit target, AssassinateSkill skill)
    {
        if (assassin == null || target == null) return;

        StartCoroutine(AssassinateCoroutine(assassin, target, skill));
    }

    private IEnumerator AssassinateCoroutine(Unit assassin, Unit target, AssassinateSkill skill)
    {
        // Lưu vị trí ban đầu
        Vector3 startPos = assassin.transform.position;
        Vector3 targetPos = target.transform.position;
        
        // Animation nhảy
        float jumpTime = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < jumpTime)
        {
            float t = elapsedTime / jumpTime;
            // Thêm đường cong cho animation nhảy
            float height = Mathf.Sin(t * Mathf.PI) * 2f;
            assassin.transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * height;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Đảm bảo đến đúng vị trí
        assassin.transform.position = targetPos;
        
        // Gán target mới cho assassin
        var targeting = assassin.GetComponent<UnitTargeting>();
        if (targeting != null)
        {
            targeting.AssignTarget(target);
        }
    }

    private int ShowRangeIndicator(Vector3 position, float radius, Color? color = null)
    {
        GameObject indicator = Instantiate(rangeIndicatorPrefab, position, Quaternion.identity);
        SkillRangeIndicator rangeIndicator = indicator.GetComponent<SkillRangeIndicator>();

        if (rangeIndicator != null)
        {
            rangeIndicator.SetRadius(radius);
            rangeIndicator.SetColor(color ?? Color.red);
        }

        int indicatorId = nextIndicatorId++;
        activeRangeIndicators[indicatorId] = indicator;
        return indicatorId;
    }

    private void HideRangeIndicator(int indicatorId)
    {
        if (activeRangeIndicators.TryGetValue(indicatorId, out GameObject indicator))
        {
            Destroy(indicator);
            activeRangeIndicators.Remove(indicatorId);
        }
    }

    private void HideAllRangeIndicators()
    {
        foreach (var indicator in activeRangeIndicators.Values)
        {
            Destroy(indicator);
        }
        activeRangeIndicators.Clear();
    }
}
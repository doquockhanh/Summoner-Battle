using UnityEngine;
using System.Collections;

public class ChargeAndSweepEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private ChargeAndSweepSkill skillData;
    private int remainingSweepAttacks;
    private bool isCharging;

    public void Initialize(Unit caster, ChargeAndSweepSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.remainingSweepAttacks = skillData.sweepAttackCount;
        this.isCharging = true;

        // Đăng ký sự kiện tấn công
        var combat = caster.GetComponent<UnitCombat>();
        if (combat != null)
        {
            UnitEvents.Combat.OnDamageDealt += HandleDamageDealt;
        }
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        caster.GetUnitStats().ModifyStat(StatType.LifeSteal, skillData.lifestealPercent);

        // Tạm dừng targeting để thực hiện charge
        caster.GetComponent<UnitTargeting>().autoTargeting = false;

        // Bắt đầu charge
        StartChargeSequence();
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("ChargeAndSweep: Invalid setup");
            return false;
        }
        return true;
    }

    private void StartChargeSequence()
    {
        var target = caster.GetComponent<UnitTargeting>().CurrentTarget;
        if (target == null) return;

        // Tính toán vị trí kéo lê
        Vector3 direction = (target.transform.position - caster.transform.position).normalized;
        Vector3 pullbackPos = target.transform.position + direction * skillData.pullbackDistance;

        // Thực hiện charge
        this.StartCoroutineSafely(ChargeCoroutine(target, pullbackPos));
    }

    private IEnumerator ChargeCoroutine(Unit target, Vector3 pullbackPos)
    {
        // Animation charge
        float chargeTime = 0.5f;
        Vector3 startPos = caster.transform.position;
        Vector3 targetPos = target.transform.position;
        Vector3 targetStartPos = targetPos;
        float elapsed = 0;
        bool hasHitTarget = false;

        while (elapsed < chargeTime)
        {
            float t = elapsed / chargeTime;

            // Di chuyển caster
            caster.transform.position = Vector3.Lerp(startPos, pullbackPos, t);

            // Kiểm tra va chạm và bắt đầu kéo lê
            if (!hasHitTarget && Vector3.Distance(caster.transform.position, targetPos) < 0.5f)
            {
                hasHitTarget = true;
                // Gây sát thương charge
                float chargeDamage = caster.GetUnitStats().GetPhysicalDamage() * skillData.chargeDamageMultiplier;
                target.TakeDamage(chargeDamage, DamageType.Physical, caster);
            }

            // Nếu đã va chạm, kéo lê target
            if (hasHitTarget)
            {
                float pullT = (elapsed - (chargeTime * 0.5f)) / (chargeTime * 0.5f);
                target.transform.position = Vector3.Lerp(targetStartPos, pullbackPos, pullT);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo vị trí cuối cùng chính xác
        if (hasHitTarget)
        {
            target.transform.position = pullbackPos;
        }

        // Kết thúc charge sequence
        isCharging = false;
        caster.GetComponent<UnitTargeting>().autoTargeting = true;
    }

    private void HandleDamageDealt(Unit source, Unit target, float amount)
    {
        if (source != caster || isCharging || remainingSweepAttacks <= 0) return;

        // Thực hiện đòn quét
        PerformSweepAttack(amount);
        remainingSweepAttacks--;

        if (remainingSweepAttacks <= 0)
        {
            caster.GetUnitStats().ModifyStat(StatType.LifeSteal, -skillData.lifestealPercent);
            Cleanup();
        }
    }

    private void PerformSweepAttack(float baseDamage)
    {
        // Tìm các unit trong phạm vi
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, skillData.sweepRadius);

        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (enemy != null && enemy.IsPlayerUnit != caster.IsPlayerUnit)
            {
                // Gây sát thương và hồi máu
                enemy.TakeDamage(baseDamage, DamageType.Physical, caster);
            }
        }

        // Hiệu ứng quét
        if (skillData.sweepEffectPrefab != null)
        {
            GameObject effect = Instantiate(skillData.sweepEffectPrefab,
                caster.transform.position,
                Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    public void Cleanup()
    {
        UnitEvents.Combat.OnDamageDealt -= HandleDamageDealt;
        Destroy(this);
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
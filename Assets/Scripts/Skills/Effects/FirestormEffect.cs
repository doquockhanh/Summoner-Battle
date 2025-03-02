using UnityEngine;

public class FirestormEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private FirestormSkill skillData;
    private Vector3 currentPosition;
    private Vector3 targetPosition;
    private float elapsedTime;
    private float damageTimer;
    private bool isMoving;
    private UnitTargeting targeting;

    public void Initialize(Unit caster, FirestormSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.elapsedTime = 0f;
        this.damageTimer = 0f;
        this.isMoving = false;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        targeting = caster.GetComponent<UnitTargeting>();
        currentPosition = targeting.CurrentTarget.transform.position;
        FindFarthestTarget();

        // Tạm dừng targeting của caster
        caster.GetComponent<UnitTargeting>().PauseTargeting();

        // Bắt đầu di chuyển bão
        isMoving = true;

        // Tạo hiệu ứng visual
        if (skillData.firestormEffectPrefab != null)
        {
            GameObject stormEffect = Instantiate(
                skillData.firestormEffectPrefab,
                currentPosition,
                Quaternion.identity
            );
            stormEffect.transform.parent = transform;
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        elapsedTime += Time.fixedDeltaTime;
        damageTimer += Time.fixedDeltaTime;

        // Di chuyển bão
        currentPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            skillData.stormSpeed * Time.fixedDeltaTime
        );

        // Gây sát thương theo tick
        if (damageTimer >= skillData.tickInterval)
        {
            DealDamage(skillData.tickDamagePercent);
            damageTimer = 0f;
        }

        // Kết thúc sau 2 giây
        if (elapsedTime >= skillData.stormDuration)
        {
            Cleanup();
        }
    }

    private void DealDamage(float damagePercent)
    {
        float baseDamage = caster.GetUnitStats().GetMagicDamage();
        float damage = baseDamage * (damagePercent / 100f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(currentPosition, skillData.stormRadius);
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (targeting.IsValidTarget(enemy))
            {
                enemy.TakeDamage(damage, DamageType.Magic, caster);

                if (enemy.GetComponent<UnitStatusEffects>().GetEffect(StatusEffectType.Burning) == null)
                {
                    // Thêm hiệu ứng thiêu đốt
                    var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                    if (statusEffects != null)
                    {
                        var burningEffect = new BurningEffect(
                            enemy,
                            skillData.duration,
                            0.02f, // 1% máu tối đa mỗi giây
                            0.5f   // Giảm 50% hồi máu
                        );
                        statusEffects.AddEffect(burningEffect);
                    }
                }
            }
        }
    }

    private void FindFarthestTarget()
    {
        float maxDistance = 0f;
        Vector3 farthestPos = currentPosition;

        Collider2D[] hits = Physics2D.OverlapCircleAll(currentPosition, skillData.radius);
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (targeting.IsValidTarget(enemy))
            {
                float distance = Vector3.Distance(currentPosition, enemy.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestPos = enemy.transform.position;
                }
            }
        }

        targetPosition = farthestPos;
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("FirestormEffect: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        if (caster != null)
        {
            caster.GetComponent<UnitTargeting>().ResumeTargeting();
        }
        Destroy(this);
    }
}
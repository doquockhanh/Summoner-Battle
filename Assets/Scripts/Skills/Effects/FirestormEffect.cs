using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private GameObject indicator;

    public void Initialize( Unit caster, FirestormSkill skillData)
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
        caster.GetComponent<UnitTargeting>().autoTargeting = false;

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
        else
        {
            indicator = SkillEffectHandler.Instance
                                            .CreateRangeIndicator(targeting.CurrentTarget.OccupiedCell, HexMetrics.GridToWorldRadius(skillData.stormRadius), new Color(1.0f, 0.41f, 0.71f, 1.0f));
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

        indicator.transform.position = currentPosition;

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
            Destroy(indicator);
        }
    }

    private void DealDamage(float damagePercent)
    {
        float baseDamage = caster.GetUnitStats().GetMagicDamage();
        float damage = baseDamage * (damagePercent / 100f);

        List<Unit> enemies = BattleManager.Instance
                                .GetAllUnitInteam(!caster.IsPlayerUnit)
                                .Where(em => 
                                        em.OccupiedCell.Coordinates
                                        .DistanceTo(HexGrid.Instance.GetCellAtPosition(currentPosition).Coordinates) <= skillData.stormRadius)
                                .ToList();
        
        foreach (Unit enemy in enemies)
        {
            if (targeting.IsValidEnemy(enemy))
            {
                enemy.TakeDamage(damage, DamageType.Magic, caster);

                if (enemy.GetComponent<UnitStatusEffects>().GetEffect(StatusEffectType.Burning) == null)
                {
                    // Thêm hiệu ứng thiêu đốt
                    var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                    if (statusEffects != null)
                    {
                        var burningEffect = new BurningEffect(
                            skillData.burnDuration,
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
        List<Unit> enemies = BattleManager.Instance
                                .GetAllUnitInteam(!caster.IsPlayerUnit);

        foreach (Unit enemy in enemies)
        {
            if (targeting.IsValidEnemy(enemy))
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
            caster.GetComponent<UnitTargeting>().autoTargeting = true;
        }
        StopAllCoroutines();
        Destroy(this);
    }

    void OnDestroy()
    {
        Cleanup();
    }
}
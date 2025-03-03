using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AncientRootEffect : MonoBehaviour, ISkillEffect
{
    private Unit caster;
    private AncientRootSkill skillData;
    private List<Unit> rootedUnits;
    private float totalDrainAmount;
    private float drainPerTick;
    private int tickCount;

    public void Initialize(Unit caster, AncientRootSkill skillData)
    {
        this.caster = caster;
        this.skillData = skillData;
        this.rootedUnits = new List<Unit>();
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        // Tìm các unit trong phạm vi
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, skillData.rootRadius);
        
        // Tính toán tổng lượng máu sẽ hút
        CalculateDrainAmount(hits);
        
        // Áp dụng hiệu ứng choáng và bắt đầu hút máu
        ApplyRootEffect(hits);
        
        // Bắt đầu hút máu theo interval
        StartCoroutine(DrainHealthCoroutine());
    }

    private void CalculateDrainAmount(Collider2D[] hits)
    {
        totalDrainAmount = 0;
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (IsValidTarget(enemy))
            {
                float drainAmount = 
                    caster.GetUnitStats().GetMagicDamage() * (skillData.magicPercent / 100f) + 
                    enemy.GetUnitStats().MaxHp * (skillData.maxHealthDrainPercent / 100f);
                totalDrainAmount += drainAmount;
            }
        }

        // Tính lượng hút mỗi tick
        tickCount = Mathf.RoundToInt(skillData.duration / skillData.drainInterval);
        drainPerTick = totalDrainAmount / tickCount;
        Debug.Log("totalDrainAmount: " + totalDrainAmount);
        Debug.Log("drainPerTick: " + drainPerTick);
    }

    private void ApplyRootEffect(Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (IsValidTarget(enemy))
            {
                rootedUnits.Add(enemy);
                
                // Thêm hiệu ứng choáng
                var statusEffects = enemy.GetComponent<UnitStatusEffects>();
                if (statusEffects != null)
                {
                    var stunEffect = new KnockupEffect(enemy, skillData.stunDuration);
                    statusEffects.AddEffect(stunEffect);
                }

                // Hiệu ứng visual
                if (skillData.rootEffectPrefab != null)
                {
                    GameObject effect = Instantiate(
                        skillData.rootEffectPrefab,
                        enemy.transform.position,
                        Quaternion.identity,
                        enemy.transform
                    );
                    Destroy(effect, skillData.stunDuration);
                }
            }
        }
    }

    private IEnumerator DrainHealthCoroutine()
    {
        int remainingTicks = tickCount;

        while (remainingTicks > 0 && rootedUnits.Count > 0)
        {
            float drainPerUnit = drainPerTick / rootedUnits.Count;
            float overheal = 0;

            foreach (Unit enemy in rootedUnits.ToArray())
            {
                if (enemy != null && !enemy.IsDead)
                {
                    // Gây sát thương chuẩn cho enemy
                    enemy.TakeDamage(drainPerUnit, DamageType.True, caster);

                    // Hồi máu cho caster
                    float currentHP = caster.GetUnitStats().CurrentHP;
                    float maxHP = caster.GetUnitStats().MaxHp;
                    float healing = drainPerUnit;

                    // Chuyển lượng máu dư thành khiên
                    if (currentHP + healing > maxHP)
                    {
                        overheal += (currentHP + healing) - maxHP;
                        healing = maxHP - currentHP;
                        
                    }

                    if (healing > 0){
                        caster.GetUnitStats().Heal(healing);
                    }

                    // Hiệu ứng hút máu
                    if (skillData.drainEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(
                            skillData.drainEffectPrefab,
                            enemy.transform.position,
                            Quaternion.identity
                        );
                        Destroy(effect, skillData.drainInterval);
                    }
                }
            }
            caster.GetUnitStats().AddShield(overheal, -1);

            remainingTicks--;
            yield return new WaitForSeconds(skillData.drainInterval);
        }

        Cleanup();
    }

    private bool IsValidTarget(Unit target)
    {
        return target != null && 
               !target.IsDead && 
               target.IsPlayerUnit != caster.IsPlayerUnit;
    }

    private bool ValidateExecution()
    {
        if (caster == null || skillData == null)
        {
            Debug.LogError("AncientRoot: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        rootedUnits.Clear();
        Destroy(this);
    }
} 
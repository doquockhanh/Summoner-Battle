using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillEffectHandler : MonoBehaviour
{
    public static SkillEffectHandler Instance { get; private set; }
    
    [Header("Hiệu ứng")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject chargeEffectPrefab;
    
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
    }
} 
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
    
    public void HandleChargeSkill(Unit caster, Vector3 targetPos, ChargeSkill skill)
    {
        if (caster == null)
        {
            Debug.LogError("HandleChargeSkill: caster is null!");
            return;
        }

        StartCoroutine(ChargeCoroutine(caster, targetPos, skill));
    }
    
    private IEnumerator ChargeCoroutine(Unit caster, Vector3 targetPos, ChargeSkill skill)
    {
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
                    
                    // Hất tung
                    StartCoroutine(KnockupCoroutine(enemy, skill.knockupDuration));
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Đảm bảo unit đến đúng vị trí cuối
        caster.transform.position = targetPos;
    }
    
    private IEnumerator KnockupCoroutine(Unit target, float duration)
    {
        if (target == null) yield break;
        
        Vector3 originalPos = target.transform.position;
        float maxHeight = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float heightPercent = Mathf.Sin((elapsedTime / duration) * Mathf.PI);
            float currentHeight = maxHeight * heightPercent;
            
            target.transform.position = originalPos + Vector3.up * currentHeight;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        target.transform.position = originalPos;
    }
} 
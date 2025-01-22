using UnityEngine;
using System.Collections.Generic;

public class UnitStats : MonoBehaviour
{
    private UnitData data;
    private float currentHp;
    private float damageModifier = 1f;
    private float speedModifier = 1f; 
    private float defenseModifier = 1f;
    private List<SkillEffect> activeEffects = new List<SkillEffect>();
    
    public bool IsDead => currentHp <= 0;
    public float CurrentHP => currentHp;
    public UnitData Data => data;
    public List<SkillEffect> ActiveEffects => activeEffects;
    
    public void Initialize(UnitData unitData)
    {
        data = unitData;
        currentHp = data.hp;
    }

    public void ModifyDamage(float amount)
    {
        damageModifier += amount;
    }
    
    public void ModifySpeed(float amount) 
    {
        speedModifier += amount;
    }
    
    public void ModifyDefense(float amount)
    {
        defenseModifier += amount;
    }

    public float GetModifiedDamage()
    {
        return data.damage * damageModifier;
    }

    public float GetModifiedSpeed()
    {
        return data.moveSpeed * speedModifier;
    }

    public float GetModifiedDefense()
    {
        return defenseModifier;
    }

    public void TakeDamage(float damage)
    {
        damage *= (1f / defenseModifier);
        
        if (damage < 0 && currentHp >= data.hp)
        {
            return;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, data.hp);

        // Hiển thị floating text
        if (damage > 0)
        {
            FloatingTextManager.Instance.ShowFloatingText(
                damage.ToString("F0"), 
                transform.position, 
                Color.red
            );
        }
        else if (damage < 0)
        {
            FloatingTextManager.Instance.ShowFloatingText(
                (-damage).ToString("F0"), 
                transform.position, 
                Color.green
            );
        }
    }
} 
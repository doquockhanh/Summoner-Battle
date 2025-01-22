using UnityEngine;

public enum EffectType
{
    DamageBoost,
    SpeedBoost,
    DefenseBoost,
    Slow,
    DamageOverTime
}

public class SkillEffect : MonoBehaviour
{
    private EffectType type;
    private float duration;
    private float amount;
    private float timer;
    private Unit targetUnit;
    
    public void Initialize(EffectType type, float duration, float amount, Unit target)
    {
        this.type = type;
        this.duration = duration;
        this.amount = amount;
        this.targetUnit = target;
        timer = duration;
        
        ApplyEffect();
    }
    
    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            
            if (type == EffectType.DamageOverTime)
            {
                targetUnit.TakeDamage(amount * Time.deltaTime);
            }
            
            if (timer <= 0)
            {
                RemoveEffect();
                Destroy(gameObject);
            }
        }
    }
    
    private void ApplyEffect()
    {
        switch (type)
        {
            case EffectType.DamageBoost:
                targetUnit.ModifyDamage(amount);
                break;
            case EffectType.SpeedBoost:
                targetUnit.ModifySpeed(amount);
                break;
            case EffectType.DefenseBoost:
                targetUnit.ModifyDefense(amount);
                break;
            case EffectType.Slow:
                targetUnit.ModifySpeed(-amount);
                break;
        }
    }
    
    private void RemoveEffect()
    {
        switch (type)
        {
            case EffectType.DamageBoost:
                targetUnit.ModifyDamage(-amount);
                break;
            case EffectType.SpeedBoost:
                targetUnit.ModifySpeed(-amount);
                break;
            case EffectType.DefenseBoost:
                targetUnit.ModifyDefense(-amount);
                break;
            case EffectType.Slow:
                targetUnit.ModifySpeed(amount);
                break;
        }
    }
} 
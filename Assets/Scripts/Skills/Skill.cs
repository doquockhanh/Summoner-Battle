using UnityEngine;

public abstract class Skill : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillName;
    public string description;
    public float manaCost;
    public float cooldown;
    public float duration;
    public float radius;
    
    [Header("Phân loại")]
    public SkillType skillType;
    public TargetType targetType;
    
    protected bool isOnCooldown;
    protected float currentCooldown;
    
    public virtual bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost && !isOnCooldown;
    }
    
    public abstract void ApplyToUnit(Unit target, Unit[] nearbyUnits = null);
    public abstract void ApplyToSummon(Unit summonedUnit);
    
    protected virtual void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = cooldown;
    }
    
    public virtual void UpdateCooldown()
    {
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                isOnCooldown = false;
            }
        }
    }
}

public enum SkillType
{
    Damage,
    Buff,
    Debuff,
    Special
}

public enum TargetType 
{
    Single,
    AOE,
    Self
} 
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
    
    // Thêm reference đến card sở hữu
    [HideInInspector]
    public CardController ownerCard;
    
    public virtual bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }
    
    public abstract void ApplyToUnit(Unit target, Unit[] nearbyUnits = null);
    public abstract void ApplyToSummon(Unit summonedUnit);
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
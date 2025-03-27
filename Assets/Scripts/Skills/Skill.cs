using UnityEngine;

public abstract class Skill : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillName;
    public string description;
    public float manaCost;
    public float duration;
    public int radius;
    public Sprite skillIcon;

    public bool hasPassive;

    [Header("Phân loại")]
    public SkillType skillType;

    // Thêm reference đến card sở hữu
    [HideInInspector]
    public CardController ownerCard;

    public virtual bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public abstract void ApplyToUnit(Unit target, Unit[] nearbyUnits = null);
    public abstract void ApplyToSummon(Unit summonedUnit);

    public abstract void ApplyPassive(Unit summonedUnit);
}

public enum SkillType
{
    Direct,     // Tác động trực tiếp (AOE, single target)
    OnSummon,   // Áp dụng cho unit mới summon
    Passive
}
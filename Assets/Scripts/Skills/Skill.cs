using System;
using UnityEngine;

public abstract class Skill : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillName;
    public string description;
    public float manaCost;
    public Sprite skillIcon;
    public bool hasPassive;
    public SkillType skillType;

    [Header("Skill animation info")]
    [Tooltip("Total length of skill animation")]
    [Range(0.3f, 10f)]
    public float animationDuration = 2f;
    public float doSkillActionAt = 1.5f;

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

    private void OnValidate()
    {
        animationDuration = Mathf.Max(0.3f, animationDuration);
        doSkillActionAt = Mathf.Min(animationDuration, doSkillActionAt);
    }
}

public enum SkillType
{
    Direct,     // Tác động trực tiếp (AOE, single target)
    OnSummon,   // Áp dụng cho unit mới summon
    Passive
}
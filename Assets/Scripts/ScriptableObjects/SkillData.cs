using UnityEngine;

public enum TargetType
{
    SingleTarget,
    AOE,
    Self,
    Ally,
    AllAllies
}

public enum SkillType 
{
    Damage,
    Heal,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    [TextArea]
    public string description;
    public Sprite skillIcon;
    
    [Header("Targeting")]
    public TargetType targetType;
    public float targetRadius = 1f;  // Cho kỹ năng AOE
    
    [Header("Stats")]
    public float rageCost = 50f;
    public float cooldown = 5f;
    
    [Header("Effects")]
    public float damage;
    public float healing;
    public float buffDuration;
    public float buffAmount;
    
    [Header("Visual Effects")]
    public GameObject skillEffectPrefab;
    public AudioClip skillSound;
} 
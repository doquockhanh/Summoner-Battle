using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public string description;
    public Sprite skillIcon;
    
    [Header("Stats")]
    public float rageCost;
    public float cooldown;
    public float damage;
} 
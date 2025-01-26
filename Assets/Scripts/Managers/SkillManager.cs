using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    
    private Dictionary<string, Skill> activeSkills = new Dictionary<string, Skill>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void RegisterSkill(string id, Skill skill)
    {
        if (!activeSkills.ContainsKey(id))
        {
            activeSkills.Add(id, skill);
        }
    }
    
    public void ActivateSkill(string skillId, Unit caster, Vector3 targetPosition)
    {
        if (activeSkills.TryGetValue(skillId, out Skill skill))
        {
            Unit[] targets = FindUnitsInRange(targetPosition, skill.radius);
            if (targets.Length > 0)
            {
                foreach (Unit target in targets)
                {
                    skill.ApplyToUnit(target);
                }
            }
        }
    }
    
    private Unit[] FindUnitsInRange(Vector3 center, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        List<Unit> units = new List<Unit>();
        
        foreach (Collider2D col in colliders)
        {
            Unit unit = col.GetComponent<Unit>();
            if (unit != null)
            {
                units.Add(unit);
            }
        }
        
        return units.ToArray();
    }
} 
using UnityEngine;

[CreateAssetMenu(fileName = "Healing", menuName = "Game/Skills/Healing")]
public class HealingSkill : Skill
{
    [Header("Cài đặt Hồi Phục")]
    [Range(0f, 100f)]
    [Tooltip("Phần trăm máu hồi phục (50% = 50)")]
    public float healPercent = 50f;
    
    [Range(0f, 100f)]
    [Tooltip("Lượng mana hồi phục cho card (5 = 5)")]
    public float manaRestore = 5f;

    [Header("Hiệu ứng")]
    public GameObject healEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        if (ownerCard == null) return;

        // Tìm đồng minh yếu nhất
        Unit weakestAlly = FindWeakestAlly();
        if (weakestAlly == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Áp dụng hồi phục
        if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.HandleHealingSkill(weakestAlly, this);
            ownerCard.OnSkillActivated();
        }
        else
        {
            ownerCard.OnSkillFailed();
        }
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây là kỹ năng trực tiếp
    }

    private Unit FindWeakestAlly()
    {
        Unit weakest = null;
        float lowestHealthPercent = float.MaxValue;

        Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.IsPlayerUnit == ownerCard.IsPlayer)
            {
                float healthPercent = unit.GetCurrentHP() / unit.GetUnitStats().MaxHp;
                if (healthPercent < lowestHealthPercent)
                {
                    lowestHealthPercent = healthPercent;
                    weakest = unit;
                }
            }
        }

        return weakest;
    }
} 
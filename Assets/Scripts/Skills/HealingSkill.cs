using System.Collections.Generic;
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
        if (ownerCard == null || ownerCard.GetActiveUnits().Count <= 0) return;

        // Tìm đồng minh yếu nhất
        Unit weakestAlly = FindWeakestAlly();
        if (weakestAlly == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Thêm effect xử lý kỹ năng
        var effect = ownerCard.gameObject.AddComponent<HealingSkillEffect>();
        effect.Initialize(weakestAlly, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây là kỹ năng trực tiếp
    }

    private Unit FindWeakestAlly()
    {
        Unit weakest = null;
        float lowestHealthPercent = float.MaxValue;

        List<Unit> allies = BattleManager.Instance.GetAllUnitInteam(ownerCard.IsPlayer);
        foreach (Unit unit in allies)
        {
            if (unit != null)
            {
                float healthPercent = unit.GetCurrentHP() / unit.GetUnitStats().GetMaxHp();
                if (healthPercent < lowestHealthPercent)
                {
                    lowestHealthPercent = healthPercent;
                    weakest = unit;
                }
            }
        }

        return weakest;
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
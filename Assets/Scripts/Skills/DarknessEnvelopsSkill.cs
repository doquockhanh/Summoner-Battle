using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DarknessEnvelops", menuName = "Game/Skills/DarknessEnvelops")]
public class DarknessEnvelopsSkill : Skill
{
    [Header("Cài đặt Sát thương")]
    [Range(0f, 100f)]
    public float baseDamage = 20f;

    [Range(0f, 10f)]
    [Tooltip("Phần trăm sát thương mỗi giây theo máu tối đa (2% = 2)")]
    public float maxHealthDamagePercent = 2f;

    [Range(0f, 100f)]
    [Tooltip("Giảm kháng phép (50% = 50)")]
    public float magicResistReduction = 50f;

    [Range(0f, 20f)]
    public float activeDuration = 10f;

    [Header("Cài đặt Hồn Ma")]
    [Range(0f, 10f)]
    public float ghostDuration = 5f;

    [Range(0f, 200f)]
    [Tooltip("Tăng tốc đánh (100% = 100)")]
    public float attackSpeedBonus = 100f;

    [Range(0f, 100f)]
    [Tooltip("Sát thương phép thêm (20% = 20)")]
    public float bonusMagicDamage = 20f;

    [Header("Hiệu ứng")]
    public GameObject darknessPrefab;
    public GameObject ghostPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        if (ownerCard == null) return;

        Unit[] enemies = GameObject.FindObjectsOfType<Unit>()
                            .Where(u => u.IsPlayerUnit != ownerCard.IsPlayer).ToArray();

        foreach (Unit enemy in enemies)
        {
            if (enemy != null)
            {
                var effect = enemy.gameObject.AddComponent<DarknessEnvelopsEffect>();
                effect.Initialize(enemy, this);
                effect.Execute(Vector3.zero);
            }
        }

        ownerCard.OnSkillActivated();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // not used
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        if (summonedUnit == null) return;

        // Thêm GhostFormBehavior để xử lý nội tại
        var ghostBehavior = summonedUnit.gameObject.AddComponent<GhostFormBehavior>();
        ghostBehavior.Initialize(summonedUnit, this);
    }
}
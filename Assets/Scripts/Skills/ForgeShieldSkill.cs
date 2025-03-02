using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ForgeShield", menuName = "Game/Skills/ForgeShield")]
public class ForgeShieldSkill : Skill
{
    [Header("Thông số khiên")]
    [Range(0f, 100f)]
    [Tooltip("Phần trăm máu tối đa làm khiên (30% = 30)")]
    public float shieldHealthPercent = 30f;

    [Range(0f, 100f)]
    [Tooltip("Phần trăm khiên chia sẻ (50% = 50)")]
    public float sharedShieldPercent = 50f;

    [Range(1f, 10f)]
    public float shareRadius = 3f;

    [Header("Hiệu ứng")]
    public GameObject forgeEffectPrefab;
    public GameObject shieldEffectPrefab;

    private int shieldID;
    private Unit strongestSmith;
    private ShieldLayer shield;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Không sử dụng vì đây là kỹ năng OnSummon
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null) return;

        // Tìm thợ rèn mạnh nhất dựa trên chỉ số phòng thủ
        strongestSmith = ownerCard.GetStrongestUnit(unit => unit.GetUnitStats().GetArmor());
        if (strongestSmith == null) return;

        // Tính lượng khiên dựa trên máu tối đa
        float shieldAmount = strongestSmith.GetUnitStats().MaxHp * (shieldHealthPercent / 100f);

        // Áp dụng khiên sharing
        if (SkillEffectHandler.Instance != null)
        {
            shield = new ShieldLayer(shieldAmount, duration, strongestSmith);
            strongestSmith.GetUnitStats().AddShield(shield);
            shield.OnShieldBroken += HandleShareShield;
            shield.OnShieldExpired += HandleShareShield;
            shieldID = shield.GetOwnerSkillID();
            ownerCard.OnSkillActivated();

            // Tạo hiệu ứng rèn
            if (forgeEffectPrefab != null)
            {
                GameObject effect = Instantiate(forgeEffectPrefab,
                    strongestSmith.transform.position,
                    Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            ownerCard.OnSkillFailed();
        }
    }

    private void HandleShareShield(int id)
    {
        shield.OnShieldBroken -= HandleShareShield;
        shield.OnShieldExpired -= HandleShareShield;
        if (id == shieldID)
        {
            Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
            var nearbyAllies = allUnits
                .Where(u => u != null &&
                           !u.IsDead &&
                           u.IsPlayerUnit == strongestSmith.IsPlayerUnit &&
                           u != strongestSmith)
                .OrderBy(u => Vector3.Distance(strongestSmith.transform.position, u.transform.position))
                .Take(2)
                .ToArray();

            foreach (var ally in nearbyAllies)
            {
                if (ally != null)
                {
                    float shieldAmount = strongestSmith.GetUnitStats().MaxHp * (shieldHealthPercent / 100f) * (sharedShieldPercent / 100f);
                    ally.GetUnitStats().AddShield(shieldAmount, duration);
                }
            }
        }
    }
}
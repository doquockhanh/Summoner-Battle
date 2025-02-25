using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GuardianAura", menuName = "Game/Skills/GuardianAura")]
public class GuardianAuraSkill : Skill
{
    [Header("Thông số phòng thủ")]
    [Range(0f, 100f)]
    [Tooltip("Tăng giáp (30% = 30)")]
    public float armorBoost = 30f;

    [Range(0f, 100f)]
    [Tooltip("Tăng kháng phép (20% = 20)")]
    public float magicResistBoost = 20f;

    [Range(0f, 5f)]
    public float auraRadius = 2f;

    [Header("Hiệu ứng")]
    public GameObject auraEffectPrefab;

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

        // Tìm unit mạnh nhất dựa trên chỉ số phòng thủ
        Unit strongestUnit = FindStrongestDefender();
        if (strongestUnit == null) return;

        // Áp dụng hiệu ứng
        if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.HandleGuardianAuraSkill(
                strongestUnit,
                this
            );
            ownerCard.OnSkillActivated();
        }
        else
        {
            ownerCard.OnSkillFailed();
        }
    }

    private Unit FindStrongestDefender()
    {
        Unit bestUnit = null;
        int maxAlliesNearby = 0;

        // Lấy tất cả unit của phe ta
        Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
        var allyUnits = allUnits.Where(u => u != null && u.OwnerCard == ownerCard).ToList();

        // Kiểm tra từng unit
        foreach (Unit unit in allyUnits)
        {
            // Tìm số lượng đồng minh trong bán kính
            int alliesCount = CountNearbyAllies(unit.transform.position, auraRadius, allyUnits);
            
            // Cập nhật unit tốt nhất nếu có nhiều đồng minh hơn
            if (alliesCount > maxAlliesNearby)
            {
                maxAlliesNearby = alliesCount;
                bestUnit = unit;
            }
        }

        return bestUnit;
    }

    private int CountNearbyAllies(Vector3 center, float radius, List<Unit> allyUnits)
    {
        int count = 0;
        foreach (Unit ally in allyUnits)
        {
            if (Vector2.Distance(center, ally.transform.position) <= radius)
            {
                count++;
            }
        }
        return count;
    }
}
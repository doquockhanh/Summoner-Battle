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

    [Range(0, 5)]
    public int auraRadius = 2;
        [Range(0, 5)]
    public int auraDuration = 5;

    [Header("Hiệu ứng")]
    public GameObject auraEffectPrefab;

    private List<Unit> alliesNearBy;
    private Unit strongestUnit;

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
        if (ownerCard == null || ownerCard.GetActiveUnits().Count <= 0) return;

        // Tìm unit mạnh nhất dựa vào số lượng allies trong tầm kỹ năng
        strongestUnit = FindStrongestDefender();
        if (strongestUnit == null) return;

        GrowSizeEffect growSizeEffect = new(5f, 1.3f);
        strongestUnit.GetComponent<UnitView>().PlaySkillAnimation(CastSkill);
        strongestUnit.GetComponent<UnitStatusEffects>().AddEffect(growSizeEffect);
        ownerCard.OnSkillActivated();
    }

    public void CastSkill()
    {
        // Thêm effect xử lý kỹ năng
        var effect = strongestUnit.gameObject.AddComponent<GuardianAuraSkillEffect>();
        effect.Initialize(strongestUnit, alliesNearBy, this);
        effect.Execute(Vector3.zero);
    }

    private Unit FindStrongestDefender()
    {
        Unit bestUnit = null;
        int maxAlliesNearby = -1;

        // Lấy tất cả AllyUnit
        List<Unit> allyUnits = BattleManager.Instance.GetAllUnitInteam(ownerCard.IsPlayer);
        List<Unit> activeUnit = ownerCard.GetActiveUnits();

        foreach (Unit unit in activeUnit)
        {
            if (unit.OccupiedCell == null) continue;
            // Tìm số lượng đồng minh trong bán kính
            List<Unit> allies = CountNearbyAllies(unit.OccupiedCell, auraRadius, allyUnits);

            // Cập nhật unit tốt nhất nếu có nhiều đồng minh hơn
            if (allies.Count > maxAlliesNearby)
            {
                maxAlliesNearby = allies.Count;
                bestUnit = unit;
                alliesNearBy = allies;
            }
        }

        return bestUnit;
    }

    private List<Unit> CountNearbyAllies(HexCell center, int radius, List<Unit> allyUnits)
    {
        int count = 0;
        List<Unit> allies = new List<Unit>();
        foreach (Unit ally in allyUnits)
        {
            if (ally == null || ally.OccupiedCell == null) continue;

            if (center.Coordinates.DistanceTo(ally.OccupiedCell.Coordinates) <= radius)
            {
                count++;
                allies.Add(ally);
            }
        }
        return allies;
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HolyAura", menuName = "Game/Skills/HolyAura")]
public class HolyAuraSkill : Skill
{
    [Header("Thông số cơ bản")]
    [Range(0f, 100f)]
    [Tooltip("Phần trăm tăng chỉ số vĩnh viễn (20% = 20)")]
    public float permanentStatsBoost = 20f;

    [Range(0f, 100f)]
    [Tooltip("Phần trăm sát thương hấp thụ (50% = 50)")]
    public float damageAbsorbPercent = 50f;

    [Range(0f, 100f)]
    [Tooltip("Phần trăm sát thương nhận lại (30% = 30)")]
    public float damageSharePercent = 30f;

    [Range(1, 10)]
    public int auraRadius = 3;

    public float auraTimer = 5f;

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

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null) return;

        // Tìm Hộ thần mạnh nhất dựa trên chỉ số phòng thủ
        Unit strongestGuardian = ownerCard.GetActiveUnits()
            .Select(unit => new { Unit = unit, Score = CalculateGuardianScore(unit) })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault()
            ?.Unit;

        if (strongestGuardian == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Thêm effect xử lý kỹ năng
        var effect = strongestGuardian.gameObject.AddComponent<HolyAuraEffect>();
        effect.Initialize(strongestGuardian, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }

    private float CalculateGuardianScore(Unit unit)
    {
        if (unit == null || unit.IsDead) return -1;

        float score = 0;
        var stats = unit.GetUnitStats();

        // 1. Unit còn sống
        score += 1;

        // 2. Chỉ số phòng thủ
        score += stats.GetArmor() + stats.GetMagicResist();

        // 3. Máu hiện tại
        score += stats.CurrentHP / stats.GetMaxHp();

        return score;
    }
} 
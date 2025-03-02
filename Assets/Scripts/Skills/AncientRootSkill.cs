using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "AncientRoot", menuName = "Game/Skills/AncientRoot")]
public class AncientRootSkill : Skill
{
    [Header("Cài đặt Rễ Cổ đại")]
    [Range(0f, 100f)]
    [Tooltip("Phần trăm máu tối đa hút mỗi giây (5% = 5)")]
    public float maxHealthDrainPercent = 5f;
     
    [Range(0f, 200f)]
    [Tooltip("Máu hồi trực tiếp theo chỉ số magic damage")]
    public float magicPercent = 100f;

    [Range(0f, 2f)]
    public float drainInterval = 1f;

    [Range(0f, 5f)]
    public float stunDuration = 2f;

    [Range(1f, 5f)]
    public float rootRadius = 3f;

    [Header("Hiệu ứng")]
    public GameObject rootEffectPrefab;
    public GameObject drainEffectPrefab;

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

        // Tìm unit mạnh nhất dựa trên chỉ số
        Unit strongestUnit = ownerCard.GetActiveUnits()
            .Select(unit => new { Unit = unit, Score = CalculateUnitScore(unit) })
            .OrderByDescending(x => x.Score)
            .First()
            .Unit;

        if (strongestUnit == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Thêm effect xử lý kỹ năng
        var effect = strongestUnit.gameObject.AddComponent<AncientRootEffect>();
        effect.Initialize(strongestUnit, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }

    private float CalculateUnitScore(Unit unit)
    {
        if (unit == null || unit.IsDead) return -1;

        float score = 0;
        score += 1; // Unit còn sống

        // Ưu tiên unit có nhiều máu
        var stats = unit.GetUnitStats();
        float healthPercent = stats.CurrentHP / stats.MaxHp;
        score += healthPercent;

        // 3. Đang tấn công đối phương
        UnitTargeting targeting = unit.GetComponent<UnitTargeting>();

        if (targeting != null && targeting.CurrentTarget != null && targeting.IsInRange(targeting.CurrentTarget))
        {
            score += 1;
        }

        return score;
    }
}
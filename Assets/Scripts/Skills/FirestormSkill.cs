using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Firestorm", menuName = "Game/Skills/Firestorm")]
public class FirestormSkill : Skill
{
    [Header("Cài đặt Bão Lửa")]
    [Range(0f, 200f)]
    [Tooltip("Sát thương bùng nổ ban đầu (100% = 100)")]
    public float initialDamagePercent = 100f;

    [Range(0f, 50f)]
    [Tooltip("Sát thương theo thời gian (15% = 15)")]
    public float tickDamagePercent = 15f;

    [Range(0.1f, 2f)]
    public float tickInterval = 0.1f;

    [Range(1f, 10f)]
    public float stormSpeed = 5f;

    [Range(1f, 5f)]
    public float stormRadius = 2f;

    [Range(1f, 5f)]
    public float stormDuration = 2f;

    [Header("Hiệu ứng")]
    public GameObject firestormEffectPrefab;

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

        // Tìm unit mạnh nhất dựa trên sát thương phép
        Unit strongestUnit = ownerCard.GetActiveUnits()
            .Select(unit => new
            {
                Unit = unit,
                Score = CalculateUnitScore(unit)
            })
            .OrderByDescending(x => x.Score)
            .First()
            .Unit;
            
        if (strongestUnit == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        if (strongestUnit.CurrentTarget == null) {
            Debug.Log("FirestormSkill: strongestUnit has no target!");
            return;
        }

        // Thêm effect xử lý kỹ năng
        var effect = strongestUnit.gameObject.AddComponent<FirestormEffect>();
        effect.Initialize(strongestUnit, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }


    private float CalculateUnitScore(Unit unit)
    {
        if (unit == null || unit.IsDead) return -1;

        float score = 0;

        // 1. Unit còn sống (điều kiện bắt buộc, đã check ở trên)
        score += 1;

        // 22. Đang tấn công đối phương
        UnitTargeting targeting = unit.GetComponent<UnitTargeting>();

        if (targeting != null && targeting.CurrentTarget != null && targeting.IsInRange(targeting.CurrentTarget))
        {
            score += 1;
        }

        // 3. Độ gần với 60% máu
        var stats = unit.GetUnitStats();
        float healthPercent = stats.CurrentHP / stats.MaxHp;
        score += healthPercent;

        return score;
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
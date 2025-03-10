using UnityEngine;

[CreateAssetMenu(fileName = "DeadlyFourth", menuName = "Game/Skills/DeadlyFourth")]
public class DeadlyFourthSkill : Skill
{
    [Header("Cài đặt Deadly Fourth")]
    [Range(0f, 200f)]
    [Tooltip("Sát thương tăng thêm cho đòn đánh thứ 4 (100% = 100)")]
    public float bonusDamagePercent = 100f;

    [Range(0f, 30f)]
    [Tooltip("Ngưỡng máu để kết liễu (15% = 15)")]
    public float executeThreshold = 15f;

    
    [Range(0f, 30f)]
    [Tooltip("Tầm đánh của đòn cường hóa")]
    public float fourthShotRange = 8f;

    public override bool CanActivate(float currentMana)
    {
        return false; // Kỹ năng passive
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Không sử dụng vì là passive
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì là passive  
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        if (summonedUnit == null) return;

        var effect = summonedUnit.gameObject.AddComponent<DeadlyFourthEffect>();
        effect.Initialize(summonedUnit, this);
        effect.Execute(Vector3.zero);
    }
} 
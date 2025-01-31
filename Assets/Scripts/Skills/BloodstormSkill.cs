using UnityEngine;

[CreateAssetMenu(fileName = "Bloodstorm", menuName = "Game/Skills/Bloodstorm")]
public class BloodstormSkill : Skill
{
    [Header("Bloodstorm Settings")]
    public float damageBasePercent = 80f;
    public float damagePerSoulPercent = 20f;
    public float healingStartPercent = 50f;
    public float healingDecreasePercent = 10f;
    public float effectRadius = 3f;
    public float moveSpeedBonus = 5f;
    public float damageInterval = 0.5f;
    
    [Header("Effects")]
    public GameObject bloodstormEffectPrefab;
    public GameObject soulAbsorbEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        // Kỹ năng này kích hoạt tự động khi đủ điều kiện
        return false;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Kỹ năng được kích hoạt qua BloodLordBehavior
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Thêm behavior đặc biệt cho Huyết Quỷ
        var bloodLord = summonedUnit.gameObject.AddComponent<BloodLordBehavior>();
        bloodLord.Initialize(this);
    }
} 
using UnityEngine;

[CreateAssetMenu(fileName = "Bloodstorm", menuName = "Game/Skills/Bloodstorm")]
public class BloodstormSkill : Skill
{
    [Header("Damage Settings")]
    [Range(0f, 200f)]
    [Tooltip("Base damage as percentage (80% = 80)")]
    public float damageBasePercent = 80f;

    [Range(0f, 100f)]
    [Tooltip("Additional damage percent per soul (20% = 20)")]
    public float damagePerSoulPercent = 20f;

    [Header("Effect Settings")]
    [Range(1f, 10f)]
    public float effectRadius = 3f;

    [Range(0f, 10f)]
    public float moveSpeedBonus = 5f;

    [Range(0.1f, 2f)]
    public float damageInterval = 0.5f;
    
    [Header("Visual Effects")]
    public GameObject bloodstormEffectPrefab;
    public GameObject soulAbsorbEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        // Kỹ năng kích hoạt tự động
        return false;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Kỹ năng được kích hoạt qua BloodLordBehavior
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (summonedUnit == null)
            throw new System.ArgumentNullException(nameof(summonedUnit));

        // Xóa behavior cũ nếu có
        var existingBehavior = summonedUnit.gameObject
            .GetComponent<BloodLordBehavior>();
        if (existingBehavior != null)
        {
            Destroy(existingBehavior);
        }

        // Di chuyển unit đến vị trí ngẫu nhiên
        Vector3 randomPosition = RandomMovementHandler.Instance.GetRandomSpawnPosition();
        summonedUnit.transform.position = randomPosition;

        // Thêm behavior mới
        var bloodLord = summonedUnit.gameObject
            .AddComponent<BloodLordBehavior>();
        bloodLord.Initialize(this);
    }

    private void OnValidate()
    {
        // Đảm bảo các giá trị hợp lệ
        damageBasePercent = Mathf.Max(0f, damageBasePercent);
        damagePerSoulPercent = Mathf.Max(0f, damagePerSoulPercent);
        effectRadius = Mathf.Max(1f, effectRadius);
        moveSpeedBonus = Mathf.Max(0f, moveSpeedBonus);
        damageInterval = Mathf.Max(0.1f, damageInterval);
    }
} 
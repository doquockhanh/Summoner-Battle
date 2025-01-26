using UnityEngine;

[CreateAssetMenu(fileName = "FuriousCavalryCharge", menuName = "Game/Skills/FuriousCavalryCharge")]
public class ChargeSkill : Skill
{
    [Header("Charge Properties")]
    public float chargeSpeed = 10f;
    public float damageMultiplier = 1f;
    public float knockupDuration = 1f;
    public float shieldPercent = 30f;
    public float shieldDuration = 5f;
    public float lifestealPercent = 20f;

    private Unit strongestUnit;

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Không sử dụng phương thức này vì đây là kỹ năng đặc biệt
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null)
        {
            Debug.LogError("ChargeSkill: ownerCard is null!");
            return;
        }

        // Tìm unit mạnh nhất trong số các unit được tạo bởi card này
        Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
        float highestDamage = 0;
        strongestUnit = null;
        
        foreach (Unit unit in allUnits)
        {
            // Kiểm tra kỹ các điều kiện null
            if (unit == null || unit.OwnerCard == null) continue;
            
            // Chỉ xét các unit được tạo bởi card này
            if (unit.OwnerCard == ownerCard)
            {
                UnitStats stats = unit.GetUnitStats();
                if (stats == null) continue;
                
                try
                {
                    float damage = stats.GetModifiedDamage();
                    if (damage > highestDamage)
                    {
                        highestDamage = damage;
                        strongestUnit = unit;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error getting unit damage: {e.Message}");
                    continue;
                }
            }
        }

        if (strongestUnit != null)
        {
            try
            {
                // Tìm khu vực đông địch nhất
                Vector3 bestTargetPos = FindBestTargetPosition(strongestUnit);
                
                // Kiểm tra SkillEffectHandler
                if (SkillEffectHandler.Instance != null)
                {
                    // Kích hoạt hiệu ứng xung kích
                    SkillEffectHandler.Instance.HandleChargeSkill(strongestUnit, bestTargetPos, this);
                    
                    // Báo cho card là đã kích hoạt thành công
                    ownerCard.OnSkillActivated();
                }
                else
                {
                    Debug.LogError("SkillEffectHandler.Instance is null!");
                    ownerCard.OnSkillFailed();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error activating charge skill: {e.Message}");
                ownerCard.OnSkillFailed();
            }
        }
        else
        {
            // Báo cho card là chưa thể kích hoạt
            ownerCard.OnSkillFailed();
            Debug.Log("No valid units found for charge skill");
        }
    }

    private Vector3 FindBestTargetPosition(Unit caster)
    {
        if (caster == null) return Vector3.zero;

        Vector3 bestPosition = caster.transform.position;
        int maxEnemies = 0;

        try
        {
            // Quét các vị trí xung quanh trong tầm
            for (float angle = 0; angle < 360; angle += 30)
            {
                Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
                Vector3 checkPosition = caster.transform.position + direction * radius;
                
                // Đếm số lượng địch trong vùng
                Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, 2f);
                int enemyCount = 0;
                
                foreach (Collider2D hit in hits)
                {
                    if (hit == null) continue;
                    
                    Unit enemy = hit.GetComponent<Unit>();
                    if (enemy != null && enemy.IsPlayerUnit != caster.IsPlayerUnit)
                    {
                        enemyCount++;
                    }
                }

                if (enemyCount > maxEnemies)
                {
                    maxEnemies = enemyCount;
                    bestPosition = checkPosition;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error finding best target position: {e.Message}");
            return caster.transform.position;
        }

        return bestPosition;
    }
}
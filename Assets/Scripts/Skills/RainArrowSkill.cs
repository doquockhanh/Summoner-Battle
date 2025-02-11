using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "RainArrow", menuName = "Game/Skills/RainArrow")]
public class RainArrowSkill : Skill
{
    [Header("Cài đặt mưa tên")]
    [Range(1, 10)]
    public int arrowWaveCount = 5;
    
    [Range(0f, 200f)]
    [Tooltip("Phần trăm sát thương mỗi đợt tên (50% = 50)")]
    public float damagePerWavePercent = 50f;
    
    [Range(0.1f, 2f)]
    public float timeBetweenWaves = 0.3f;
    
    [Range(1f, 5f)]
    public float effectRadius = 2f;
    
    [Header("Hiệu ứng")]
    public GameObject rainArrowEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        // Không sử dụng phương thức này vì đây là kỹ năng đặc biệt
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null)
        {
            Debug.LogError("RainArrowSkill: ownerCard is null!");
            return;
        }

        Vector3 bestTargetPos = FindBestTargetPosition();
        
        if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.HandleRainArrowSkill(bestTargetPos, this);
            ownerCard.OnSkillActivated();
        }
        else
        {
            Debug.LogError("SkillEffectHandler.Instance is null!");
            ownerCard.OnSkillFailed();
        }
    }

    private Vector3 FindBestTargetPosition()
    {
        Vector3 bestPosition = Vector3.zero;
        float maxEnemyCount = 0;
        float checkRadius = effectRadius;

        // Quét toàn bộ map để tìm khu vực đông địch nhất
        for (float x = -10f; x <= 10f; x += 2f)
        {
            for (float y = -5f; y <= 5f; y += 2f)
            {
                Vector3 checkPosition = new Vector3(x, y, 0);
                int enemyCount = CountEnemiesInRange(checkPosition, checkRadius);

                if (enemyCount > maxEnemyCount)
                {
                    maxEnemyCount = enemyCount;
                    bestPosition = checkPosition;
                }
            }
        }

        return bestPosition;
    }

    private int CountEnemiesInRange(Vector3 center, float radius)
    {
        int count = 0;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit != ownerCard.IsPlayer)
            {
                count++;
            }
        }
        
        return count;
    }
} 
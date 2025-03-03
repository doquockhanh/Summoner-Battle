using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Game/Skills/Fireball")]
public class FireballSkill : Skill
{
    [Header("Cài đặt Fireball")]
    [Range(0f, 200f)]
    [Tooltip("Phần trăm sát thương phép (100% = 100)")]
    public float magicDamagePercent = 100f;
    
    [Range(0f, 5f)]
    public float effectRadius = 2f;
    
    [Header("Hiệu ứng thiêu đốt")]
    [Range(0f, 5f)]
    [Tooltip("Phần trăm sát thương mỗi giây theo máu tối đa (1% = 0.01)")]
    public float burnDamagePercent = 0.01f;
    
    [Range(0f, 20f)]
    public float burnDuration = 10f;
    
    [Range(0f, 1f)]
    [Tooltip("Giảm hồi máu (50% = 0.5)")]
    public float healingReduction = 0.5f;
    
    [Header("Hiệu ứng")]
    public GameObject fireballEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        if (ownerCard == null)
        {
            Debug.LogError("FireballSkill: ownerCard is null!");
            return;
        }

        Vector3 bestTargetPos = FindBestTargetPosition();
        
        if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.HandleFireballSkill(bestTargetPos, this, ownerCard.IsPlayer);
            ownerCard.OnSkillActivated();
        }
        else
        {
            Debug.LogError("SkillEffectHandler.Instance is null!");
            ownerCard.OnSkillFailed();
        }
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây là kỹ năng AOE trực tiếp
    }

    private Vector3 FindBestTargetPosition()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager.Instance is null!");
            return Vector3.zero;
        }

        var searchParams = new AOETargetFinder.AOESearchParams
        {
            searchWidth = BattleManager.Instance.MapWidth,
            searchHeight = BattleManager.Instance.MapHeight,
            effectRadius = effectRadius,
            gridSize = 1f,
            isPlayerTeam = ownerCard.IsPlayer,
            customFilter = (unit) => !unit.IsDead
        };

        return AOETargetFinder.FindBestAOEPosition(searchParams);
    }
} 
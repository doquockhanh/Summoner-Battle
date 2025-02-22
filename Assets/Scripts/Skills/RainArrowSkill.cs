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

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây không phải kỹ năng liên quan đến summon
    }

    private Vector3 FindBestTargetPosition()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("RainArrowSkill: BattleManager.Instance is null!");
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
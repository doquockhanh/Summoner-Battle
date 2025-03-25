using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Range(1, 5)]
    public int effectRadius = 2;

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

        List<Unit> enemies = BattleManager.Instance.GetAllUnitInteam(!ownerCard.IsPlayer);
        if (enemies.Count <= 0) return;

        HexCell bestTargetPos = HexGrid.Instance.FindSpotForAOESkill(effectRadius, !ownerCard.IsPlayer);
        // Thêm effect xử lý kỹ năng
        var effect = ownerCard.gameObject.AddComponent<RainArrowSkillEffect>();
        effect.Initialize(bestTargetPos, this, rainArrowEffectPrefab, ownerCard.IsPlayer);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây không phải kỹ năng liên quan đến summon
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
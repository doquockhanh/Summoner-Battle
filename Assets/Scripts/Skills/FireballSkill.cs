using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Game/Skills/Fireball")]
public class FireballSkill : Skill
{
    [Header("Cài đặt Fireball")]
    [Range(0f, 200f)]
    [Tooltip("Phần trăm sát thương phép (100% = 100)")]
    public float magicDamagePercent = 100f;

    [Range(0, 5)]
    public int effectRadius = 2;

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
        if (ownerCard == null || BattleManager.Instance.GetAllUnitInteam(!ownerCard.IsPlayer).Count <= 0)
        {
            Debug.LogError("FireballSkill: ownerCard is null!");
            return;
        }

        HexCell bestTargetPos = HexGrid.Instance.FindSpotForAOESkill(effectRadius, !ownerCard.IsPlayer);
        var effect = ownerCard.gameObject.AddComponent<FireBallSkillEffect>();
        effect.Initialize(bestTargetPos, this);
        effect.Execute(Vector3.zero);
        ownerCard.OnSkillActivated();
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì đây là kỹ năng AOE trực tiếp
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
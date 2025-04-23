using UnityEngine;

[CreateAssetMenu(fileName = "SummonMonsters", menuName = "Game/Skills/SummonMonsters")]
public class SummonMonstersSkill : Skill
{
    [Header("Cài đặt Triệu Hồi")]
    public int monsterCount = 3;
    public float monsterLifetime = 2f;
    public int monsterExplodeRadius = 2;
    public UnitData monsterData;
    public GameObject summonEffectPrefab;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {
        if (ownerCard == null || monsterData == null) return;

        // Tìm vị trí tốt nhất để triệu hồi
        Vector3 bestPos = FindBestSummonPosition();

        // Triệu hồi quái vật
        for (int i = 0; i < monsterCount; i++)
        {
            Vector3 offset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPos = bestPos + offset;

            Unit monster = UnitPoolManager.Instance.GetUnit(monsterData, ownerCard.IsPlayer, ownerCard);
            monster.transform.position = spawnPos;
            monster.Initialize(monsterData, ownerCard.IsPlayer, ownerCard);

            // Thêm effect nổ khi chết
            var effect = monster.gameObject.AddComponent<ExplodingMonsterEffect>();
            effect.Initialize(monster, this);
            effect.Execute(Vector3.zero);
        }

        // Hiệu ứng triệu hồi
        if (summonEffectPrefab != null)
        {
            GameObject effect = Instantiate(summonEffectPrefab, bestPos, Quaternion.identity);
            Destroy(effect, 1f);
        }

        ownerCard.OnSkillActivated();
    }

    private Vector3 FindBestSummonPosition()
    {
        if (BattleManager.Instance == null) return Vector3.zero;
        return BattleManager.Instance.GetSpawnPosition(ownerCard.IsPlayer);
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        // Không sử dụng vì là kỹ năng trực tiếp
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        // Không sử dụng
    }
}
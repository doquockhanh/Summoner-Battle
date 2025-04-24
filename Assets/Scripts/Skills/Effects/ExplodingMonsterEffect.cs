using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplodingMonsterEffect : MonoBehaviour, ISkillEffect
{
    private Unit monster;
    private SummonMonstersSkill skillData;
    private float lifetime;

    public void Initialize(Unit monster, SummonMonstersSkill skillData)
    {
        this.monster = monster;
        this.skillData = skillData;
        this.lifetime = skillData.monsterLifetime;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        StartCoroutine(LifetimeCoroutine());
        GetComponent<UnitTargeting>().SetTarget(FindRandomEnenmy());
    }

    private Unit FindRandomEnenmy()
    {
        // Lấy tất cả unit trong tầm đánh của quái vật
        Unit[] units = FindObjectsOfType<Unit>();

        // Lọc ra các unit địch còn sống
        List<Unit> enemies = new List<Unit>();
        foreach (Unit hit in units)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null &&
                !unit.IsDead &&
                unit.IsPlayerUnit != monster.IsPlayerUnit)
            {
                enemies.Add(unit);
            }
        }

        // Trả về một unit ngẫu nhiên từ danh sách
        if (enemies.Count > 0)
        {
            int randomIndex = Random.Range(0, enemies.Count);
            return enemies[randomIndex];
        }

        return null;
    }

    private bool ValidateExecution()
    {
        if (monster == null || skillData == null)
        {
            Debug.LogError("ExplodingMonster: Invalid setup");
            return false;
        }
        return true;
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (monster != null && !monster.IsDead)
        {
            // Gây sát thương nổ
            Explode();

            // Hủy quái vật
            monster.GetUnitStats().TakeDamage(999f, DamageType.True);
            UnitPoolManager.Instance.ReturnToPool(monster);
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(monster.transform.position, skillData.monsterExplodeRadius);

        foreach (Collider2D hit in hits)
        {
            Unit enemy = hit.GetComponent<Unit>();
            if (enemy != null && enemy.IsPlayerUnit != monster.IsPlayerUnit)
            {
                float damage = monster.GetUnitStats().GetMagicDamage();
                enemy.TakeDamage(damage, DamageType.Magic, monster);
            }
        }
    }

    public void Cleanup()
    {
        Destroy(this);
    }

    void OnDestroy()
    {
        Cleanup();
    }
}
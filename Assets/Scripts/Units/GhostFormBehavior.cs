using UnityEngine;
using System.Collections;

public class GhostFormBehavior : MonoBehaviour
{
    private Unit unit;
    private DarknessEnvelopsSkill skillData;
    private UnitStats stats;
    private bool isGhostForm = false;

    public void Initialize(Unit unit, DarknessEnvelopsSkill skillData)
    {
        this.unit = unit;
        this.skillData = skillData;
        this.stats = unit.GetUnitStats();
        
        // Đăng ký sự kiện chết
        unit.OnDeath += ActivateGhostForm;
    }

    private void ActivateGhostForm()
    {
        if (isGhostForm) return;
        isGhostForm = true;

        // Tăng tốc đánh
        float currentAttackSpeed = stats.GetAttackSpeed() * (skillData.attackSpeedBonus / 100f);
        stats.ModifyAttackSpeed(currentAttackSpeed);

        // Hiệu ứng visual
        if (skillData.ghostPrefab != null)
        {
            GameObject effect = Instantiate(
                skillData.ghostPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            Destroy(effect, skillData.ghostDuration);
        }

        // Hủy unit sau thời gian
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(skillData.ghostDuration);
        Destroy(unit.gameObject);
    }

    private void OnDestroy()
    {
        if (unit != null)
        {
            unit.OnDeath -= ActivateGhostForm;
        }
    }
} 
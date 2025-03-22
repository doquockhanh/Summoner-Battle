using UnityEngine;
using System.Collections;

public class GhostFormBehavior : MonoBehaviour
{
    private Unit unit;
    private DarknessEnvelopsSkill skillData;
    private UnitStats stats;
    private bool isGhostForm = false;
    private ImmortalEffect immortalEffect;

    public void Initialize(Unit unit, DarknessEnvelopsSkill skillData)
    {
        this.unit = unit;
        this.skillData = skillData;
        this.stats = unit.GetUnitStats();

        // Đăng ký sự kiện chết
        stats.OnTakeLethalityDamage += ActivateGhostForm;
    }

    private void ActivateGhostForm(Unit _)
    {
        if (isGhostForm) return;
        isGhostForm = true;

        // ImmortalImmortal
        if (unit.GetComponent<UnitStats>().CurrentHP <= 0)
        {
            unit.GetComponent<UnitStats>().SetCurrentHp(1);
        }

        // Tăng tốc đánh
        float currentAttackSpeed = stats.GetAttackSpeed() * (skillData.attackSpeedBonus / 100f);
        stats.ModifyStat(StatType.AttackSpeed, currentAttackSpeed);

        // Immortal
        var statusEffects = unit.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            immortalEffect = new ImmortalEffect(unit, skillData.ghostDuration);
            statusEffects.AddEffect(immortalEffect);
        }
        immortalEffect.OnImmortalEffectEnded += ImmortalEffectEnded;

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
    }


    private void ImmortalEffectEnded()
    {
        if (unit != null)
        {
            stats.OnTakeLethalityDamage -= ActivateGhostForm;
            unit.GetComponent<UnitStats>().TakeDamage(9999, DamageType.True);
            UnitPoolManager.Instance.ReturnToPool(unit);
        }
        if (immortalEffect != null)
        {
            immortalEffect.OnImmortalEffectEnded -= ImmortalEffectEnded;
        }

        Destroy(this);
    }
}
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "Assassinate", menuName = "Game/Skills/Assassinate")]
public class AssassinateSkill : Skill
{
    [Header("Cài đặt Assassinate")]
    public float jumpRange = 3f;
    public float lifestealPercent = 20f;
    public float firstHitCritMultiplier = 2f;

    private Unit assassin;
    private bool hasUsedFirstHit = false;

    public override bool CanActivate(float currentMana)
    {
        return currentMana >= manaCost;
    }

    public override void ApplyToUnit(Unit target, Unit[] nearbyUnits = null)
    {

    }

    private void ApplyAssassinateEffects(Unit assassin, Unit target)
    {
        // Thêm hiệu ứng hút máu
        var statusEffects = assassin.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            assassin.GetUnitStats().ModifyStat(StatType.LifeSteal, lifestealPercent);
            var stealthEffect = new AssassinStealthEffect(assassin);

            statusEffects.AddEffect(stealthEffect);
        }

        // Di chuyển đến mục tiêu
        if (SkillEffectHandler.Instance != null)
        {
            SkillEffectHandler.Instance.HandleAssassinateSkill(assassin, target, this);
        }
    }

    private Unit FindStrongestAssassin()
    {
        Unit strongest = null;
        float highestDamage = 0;

        Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            if (unit.OwnerCard == ownerCard)
            {
                float damage = unit.GetUnitStats().GetPhysicalDamage();
                if (damage > highestDamage)
                {
                    highestDamage = damage;
                    strongest = unit;
                }
            }
        }

        return strongest;
    }

    private Unit FindWeakestTargetInRange(Vector3 center, float range)
    {
        Unit weakest = null;
        float lowestHealth = float.MaxValue;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, range);
        foreach (Collider2D col in colliders)
        {
            Unit unit = col.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit != ownerCard.IsPlayer)
            {
                float health = unit.GetCurrentHP();
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    weakest = unit;
                }
            }
        }

        return weakest;
    }

    public override void ApplyToSummon(Unit summonedUnit)
    {
        if (ownerCard == null) return;

        // Tìm assassin mạnh nhất
        assassin = FindStrongestAssassin();
        if (assassin == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Tìm mục tiêu yếu nhất trong tầm
        Unit weakestTarget = FindWeakestTargetInRange(assassin.transform.position, jumpRange);
        if (weakestTarget == null)
        {
            ownerCard.OnSkillFailed();
            return;
        }

        // Áp dụng các hiệu ứng
        ApplyAssassinateEffects(assassin, weakestTarget);
        ownerCard.OnSkillActivated();
    }

    public override void ApplyPassive(Unit summonedUnit)
    {
        throw new System.NotImplementedException();
    }
}
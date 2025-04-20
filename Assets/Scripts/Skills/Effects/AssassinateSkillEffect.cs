using UnityEngine;
using System.Collections;

public class AssassinateSkillEffect : MonoBehaviour, ISkillEffect
{
    private Unit assassin;
    private Unit target;
    private AssassinateSkill skillData;

    public void Initialize(Unit assassin, Unit target, AssassinateSkill skillData)
    {
        this.assassin = assassin;
        this.target = target;
        this.skillData = skillData;
    }

    public void Execute(Vector3 targetPos)
    {
        if (!ValidateExecution()) return;

        Assassinate();
    }


    private void Assassinate()
    {
        bool isOccupied = HexGrid.Instance.AssertOccupyCell(target.transform.position, assassin, 2);
        if (isOccupied == false) return;
        assassin.GetComponent<UnitCombat>().SetRegisteredCell(null);

        // Thêm hiệu ứng tàng hình onKill
        UnitStatusEffects statusEffects = assassin.GetComponent<UnitStatusEffects>();
        if (statusEffects != null)
        {
            UntargetableEffect untargetable = new UntargetableEffect(5);
            statusEffects.AddEffect(untargetable);

            if (!statusEffects.HasEffect(StatusEffectType.StealthOnKill))
            {
                AssassinStealthEffect stealthEffect = new AssassinStealthEffect(skillData.untargetableDuration);
                statusEffects.AddEffect(stealthEffect);
            }
        }

        assassin.transform.position = assassin.OccupiedCell.WorldPosition;
        UnitTargeting targeting = assassin.GetComponent<UnitTargeting>();
        if (targeting != null)
            targeting.SetTarget(target);

        Cleanup();
    }

    private bool ValidateExecution()
    {
        if (assassin == null || target == null || skillData == null)
        {
            Debug.LogError("AssassinateSkill: Invalid setup");
            return false;
        }
        return true;
    }

    public void Cleanup()
    {
        Destroy(this);
    }
}
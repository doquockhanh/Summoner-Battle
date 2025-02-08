using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitStats stats;
    private UnitView view;
    private float attackTimer;
    private UnitStatusEffects statusEffects;

    private const float ATTACK_COOLDOWN_BUFFER = 0.1f;

    private void Awake()
    {
        statusEffects = GetComponent<UnitStatusEffects>();
    }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.stats = unit.GetComponent<UnitStats>();
        this.view = unit.GetComponent<UnitView>();
        ResetAttackTimer();
    }

    public void TryAttack(Unit target)
    {
        if (!CanAttack() || !statusEffects.CanAct() || target == null || target.IsDead) return;

        PerformAttack(target);
        ResetAttackTimer();
    }

    public void AttackBase(Base baseTarget)
    {
        if (!CanAttack() || !statusEffects.CanAct() || baseTarget == null) return;

        PerformBaseAttack(baseTarget);
        ResetAttackTimer();
    }

    public void TryUseSkill()
    {
        if (!statusEffects.CanAct()) return;
        // ... rest of skill logic
    }

    private bool CanAttack() => attackTimer <= 0;

    private void PerformAttack(Unit target)
    {
        float damage = stats.GetModifiedDamage();
        target.TakeDamage(damage);
        
        bool faceRight = target.transform.position.x > transform.position.x;
        view.FlipSprite(faceRight);
        
        view.PlayAttackAnimation();
        view.PlayAttackEffect();

        UnitEvents.Combat.RaiseDamageDealt(unit, target, damage);
    }

    private void PerformBaseAttack(Base baseTarget)
    {
        float damage = stats.GetModifiedDamage();
        baseTarget.TakeDamage(damage);
        
        bool faceRight = baseTarget.transform.position.x > transform.position.x;
        view.FlipSprite(faceRight);
        
        view.PlayAttackAnimation();
        view.PlayAttackEffect();
    }

    private void ResetAttackTimer()
    {
        attackTimer = (1f / stats.Data.attackSpeed) + ATTACK_COOLDOWN_BUFFER;
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }
}
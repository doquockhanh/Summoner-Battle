using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UnitStats : MonoBehaviour
{
    private UnitData data;
    private float currentHp;
    private float currentShield;
    
    // Các hệ số modifier
    private float damageModifier = 1f;
    private float speedModifier = 1f;
    private float defenseModifier = 1f;
    
    // Cache các giá trị đã tính toán
    private float cachedModifiedDamage;
    private bool isDamageModified = false;

    private Coroutine removeShieldCoroutine;
    private Coroutine resetLifestealCoroutine;

    private float healingReceivedModifier = 1f;

    public UnitData Data => data;
    public float CurrentHP => currentHp;
    public float MaxHp => data.hp;
    public bool IsDead => currentHp <= 0;
    public float CurrentHealthPercent => currentHp / MaxHp;
    
    // Events
    public event System.Action<float> OnHealthChanged;
    public event System.Action<float> OnShieldChanged;
    public event System.Action OnDeath;

    public void Initialize(UnitData unitData)
    {
        data = unitData;
        currentHp = unitData.hp;
        currentShield = 0;
        ResetModifiers();
    }

    public void TakeDamage(float rawDamage)
    {
        if (IsDead) return;

        float damage = CalculateDamageAfterDefense(rawDamage);
        
        // Xử lý shield trước
        float remainingDamage = ProcessShieldDamage(damage);
        
        // Xử lý HP
        if (remainingDamage > 0)
        {
            ProcessHealthDamage(remainingDamage);
        }

        CheckDeath();
    }

    private float CalculateDamageAfterDefense(float rawDamage)
    {
        return rawDamage / defenseModifier;
    }

    private float ProcessShieldDamage(float damage)
    {
        if (currentShield <= 0) return damage;

        // Tính toán lại damage cho shield
        float shieldDamage = Mathf.Min(damage, currentShield);
        currentShield -= shieldDamage;
        OnShieldChanged?.Invoke(currentShield);
        
        ShowDamageNumber(shieldDamage, DamageType.Shield);
        
        return damage - shieldDamage; // Trả về damage còn lại sau khi trừ shield
    }

    private void ProcessHealthDamage(float damage)
    {
        currentHp -= damage;
        OnHealthChanged?.Invoke(currentHp);
        ShowDamageNumber(damage, DamageType.Health);
    }

    private void CheckDeath()
    {
        if (currentHp <= 0)
        {
            currentHp = 0;
            OnDeath?.Invoke();
            UnitEvents.Status.RaiseUnitDeath(GetComponent<Unit>());
        }
    }

    public float GetModifiedDamage()
    {
        if (!isDamageModified)
        {
            cachedModifiedDamage = data.damage * damageModifier;
            isDamageModified = true;
        }
        return cachedModifiedDamage;
    }

    public void ModifyDamage(float modifier)
    {
        damageModifier += modifier;
        isDamageModified = false;
    }

    public void ModifySpeed(float modifier)
    {
        speedModifier += modifier;
    }

    public void ModifyDefense(float modifier)
    {
        defenseModifier += modifier;
    }

    public void ResetModifiers()
    {
        damageModifier = 1f;
        speedModifier = 1f;
        defenseModifier = 1f;
        isDamageModified = false;
    }

    public void AddShield(float amount)
    {
        currentShield += amount;
        OnShieldChanged?.Invoke(currentShield);
    }

    public void Heal(float amount)
    {
        float modifiedHealing = amount * healingReceivedModifier;
        currentHp = Mathf.Min(currentHp + modifiedHealing, MaxHp);
        OnHealthChanged?.Invoke(currentHp);
    }

    public void ProcessLifesteal(float damageDealt)
    {
        if (data.lifestealPercent <= 0) return;
        float healAmount = damageDealt * data.lifestealPercent;
        Heal(healAmount);
    }

    private void ShowDamageNumber(float amount, DamageType type)
    {
        Color textColor = type switch
        {
            DamageType.Health => Color.red,
            DamageType.Shield => Color.black,
            DamageType.Heal => Color.green,
            _ => Color.white
        };

        Vector3 position = type == DamageType.Shield ? 
            transform.position + Vector3.up * 0.5f : 
            transform.position;

        FloatingTextManager.Instance.ShowFloatingText(
            amount.ToString("F0"),
            position,
            textColor
        );
    }

    public void ApplyShield(float shieldPercent, float duration)
    {
        // Hủy coroutine cũ nếu đang có shield
        if (removeShieldCoroutine != null)
        {
            StopCoroutine(removeShieldCoroutine);
            currentShield = 0; // Reset shield cũ
        }

        // Áp dụng shield mới
        float shieldAmount = MaxHp * (shieldPercent / 100f); // Chuyển % thành tỉ lệ
        currentShield = shieldAmount;
        OnShieldChanged?.Invoke(currentShield);
        
        // Bắt đầu coroutine mới
        removeShieldCoroutine = StartCoroutine(RemoveShieldAfterDelay(duration));
    }

    private IEnumerator RemoveShieldAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentShield = 0;
        OnShieldChanged?.Invoke(currentShield);
        removeShieldCoroutine = null;
    }

    public void SetLifesteal(float percent, float duration)
    {
        // Hủy coroutine cũ nếu đang có lifesteal
        if (resetLifestealCoroutine != null)
        {
            StopCoroutine(resetLifestealCoroutine);
            data.lifestealPercent = 0;
        }

        // Áp dụng lifesteal mới
        data.lifestealPercent = percent / 100f; // Chuyển % thành tỉ lệ
        resetLifestealCoroutine = StartCoroutine(ResetLifestealAfterDelay(duration));
    }

    private IEnumerator ResetLifestealAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        data.lifestealPercent = 0;
        resetLifestealCoroutine = null;
    }

    public void ModifyHealingReceived(float modifier)
    {
        healingReceivedModifier = modifier;
    }
    
    public void ResetHealingReceived()
    {
        healingReceivedModifier = 1f;
    }

    private enum DamageType
    {
        Health,
        Shield,
        Heal
    }
}
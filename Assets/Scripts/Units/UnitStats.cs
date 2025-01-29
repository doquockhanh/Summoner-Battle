using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UnitStats : MonoBehaviour
{
    public delegate void HealthChangedHandler(float currentHealth);
    public event HealthChangedHandler OnHealthChanged;
    public delegate void ShieldChangedHandler(float shieldAmount);
    public event ShieldChangedHandler OnShieldChanged;

    public delegate void ShieldDamageHandler(float damage);
    public event ShieldDamageHandler OnShieldDamage;

    private UnitData data;
    private float currentHp;
    private float damageModifier = 1f;
    private float speedModifier = 1f;
    private float defenseModifier = 1f;
    private float maxHp;
    private float currentShield;
    private float shieldModifier = 1f;
    private float lifestealPercent;
    private bool hasLifesteal;

    public bool IsDead => currentHp <= 0;
    public float CurrentHP => currentHp;
    public UnitData Data => data;
    public float MaxHp => maxHp;
    public float CurrentShield => currentShield;
    public bool HasLifesteal => hasLifesteal;
    public float LifestealPercent => lifestealPercent;

    public void Initialize(UnitData unitData)
    {
        data = unitData;
        currentHp = data.hp;
        maxHp = data.hp;
    }

    public void ModifyDamage(float amount)
    {
        damageModifier += amount;
    }

    public void ModifySpeed(float amount)
    {
        speedModifier += amount;
    }

    public void ModifyDefense(float amount)
    {
        defenseModifier += amount;
    }

    public float GetModifiedDamage()
    {
        return data.damage * damageModifier;
    }

    public float GetModifiedSpeed()
    {
        return data.moveSpeed * speedModifier;
    }

    public float GetModifiedDefense()
    {
        return defenseModifier;
    }

    public void TakeDamage(float damage)
    {
        float remainingDamage = damage;

        // Xử lý shield trước
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(remainingDamage, currentShield);
            currentShield -= shieldDamage;
            remainingDamage -= shieldDamage;
            OnShieldChanged?.Invoke(currentShield);

            // Hiển thị sát thương shield
            FloatingTextManager.Instance.ShowFloatingText(
                shieldDamage.ToString("F0"),
                transform.position + Vector3.up * 0.5f,
                Color.black
            );
        }

        // Nếu còn sát thương, xử lý HP
        if (remainingDamage > 0)
        {
            currentHp -= remainingDamage;
            OnHealthChanged?.Invoke(currentHp);

            // Hiển thị sát thương HP
            FloatingTextManager.Instance.ShowFloatingText(
                remainingDamage.ToString("F0"),
                transform.position,
                Color.red
            );
        }
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, MaxHp);
        OnHealthChanged?.Invoke(CurrentHP);
        FloatingTextManager.Instance.ShowFloatingText(
               amount.ToString("F0"),
               transform.position,
               Color.green
           );
    }

    public void TakeShieldDamage(float damage)
    {
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(damage, currentShield);
            currentShield -= shieldDamage;
            OnShieldDamage?.Invoke(shieldDamage);
        }
    }

    public void AddShield(float amount)
    {
        currentShield += amount;
        OnHealthChanged?.Invoke(currentHp); // Cập nhật UI
    }

    public void ApplyShield(float amount, float duration)
    {
        float shieldAmount = MaxHp * (amount / 100f);
        currentShield += shieldAmount;
        OnShieldChanged?.Invoke(currentShield);
        StartCoroutine(ShieldDurationCoroutine(duration));
    }

    public void SetLifesteal(float percent, float duration)
    {
        lifestealPercent = percent;
        hasLifesteal = true;
        StartCoroutine(LifestealDurationCoroutine(duration));
    }

    public void ProcessLifesteal(float damageDealt)
    {
        if (hasLifesteal)
        {
            float healAmount = damageDealt * (lifestealPercent / 100f);
            Heal(healAmount);
        }
    }

    private IEnumerator ShieldDurationCoroutine(float duration)
    {
        float startShield = currentShield;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            float remainingPercent = 1 - (elapsedTime / duration);
            currentShield = startShield * remainingPercent;
            OnShieldChanged?.Invoke(currentShield);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        currentShield = 0;
        OnShieldChanged?.Invoke(currentShield);
    }

    private IEnumerator LifestealDurationCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        lifestealPercent = 0;
        hasLifesteal = false;
    }
}
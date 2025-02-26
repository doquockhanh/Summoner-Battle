using UnityEngine;
using System;

public class ShieldLayer
{
    public float Value { get; private set; }
    public float RemainingValue { get; private set; }
    public float Duration { get; private set; }
    public Unit Source { get; private set; }
    public ShieldType Type { get; private set; }
    
    public event Action<float> OnShieldAbsorbed;
    public event Action OnShieldBroken;
    public event Action OnShieldExpired;

    public ShieldLayer(float value, float duration, Unit source, ShieldType type = ShieldType.Normal)
    {
        Value = value;
        RemainingValue = value;
        Duration = duration;
        Source = source;
        Type = type;
    }

    public float AbsorbDamage(float damage)
    {
        float absorbed = Mathf.Min(RemainingValue, damage);
        RemainingValue -= absorbed;
        
        OnShieldAbsorbed?.Invoke(absorbed);
        
        if (RemainingValue <= 0)
        {
            OnShieldBroken?.Invoke();
        }
        
        return damage - absorbed;
    }

    public void UpdateDuration(float deltaTime)
    {
        if (Duration < 0) return; // Shield vĩnh viễn
        
        Duration -= deltaTime;
        if (Duration <= 0 && RemainingValue > 0)
        {
            OnShieldExpired?.Invoke();
        }
    }

    public bool IsExpired => Duration >= 0 && Duration <= 0;

    public void ProcessAbsorption(float absorbedAmount)
    {
        OnShieldAbsorbed?.Invoke(absorbedAmount);
        
        if (RemainingValue <= 0)
        {
            OnShieldBroken?.Invoke();
        }
    }
}

public enum ShieldType
{
    Normal,     // Shield thông thường
    Absorption, // Shield hấp thụ và chuyển hóa
    Reflective, // Shield phản sát thương
    Sharing     // Shield chia sẻ
} 
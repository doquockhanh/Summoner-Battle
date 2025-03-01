using UnityEngine;
using System;

public class ShieldLayer
{
    public float Value { get; private set; }
    public float RemainingValue { get; private set; }
    public float Duration { get; private set; }
    public Unit Source { get; private set; }
    public int OwnerSkillID { get; private set; }
    
    public event Action<float, int> OnShieldAbsorbed;
    public event Action<int> OnShieldBroken;
    public event Action<int> OnShieldExpired;

    public ShieldLayer(float value, float duration, Unit source = null)
    {
        Value = value;
        RemainingValue = value;
        Duration = duration;
        Source = source;
        OwnerSkillID = UnityEngine.Random.Range(0, 100000);
    }

    public int GetOwnerSkillID(){
        return OwnerSkillID;
    }

    public float AbsorbDamage(float damage)
    {
        float absorbed = Mathf.Min(RemainingValue, damage);
        RemainingValue -= absorbed;
        
        OnShieldAbsorbed?.Invoke(absorbed, OwnerSkillID);
        
        if (RemainingValue <= 0)
        {
            OnShieldBroken?.Invoke(OwnerSkillID);
        }
        
        return damage - absorbed;
    }

    public void UpdateDuration(float deltaTime)
    {
        if (Duration < 0) return; // Shield vĩnh viễn
        
        Duration = Mathf.Max(Duration - deltaTime, 0);
        if (Duration == 0)
        {
            OnShieldExpired?.Invoke(OwnerSkillID);
        }
    }

    public bool IsExpired => Duration >= 0 && Duration <= 0;
}
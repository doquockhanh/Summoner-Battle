using System.Collections.Generic;
using UnityEngine;

public class UnitStatusEffects : MonoBehaviour
{
    private Dictionary<StatusEffectType, IStatusEffect> activeEffects = new Dictionary<StatusEffectType, IStatusEffect>();
    private Unit unit;

    public bool IsKnockedUp => HasEffect(StatusEffectType.Knockup);
    public bool IsStunned => HasEffect(StatusEffectType.Stun);
    public bool IsSlowed => HasEffect(StatusEffectType.Slow);

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        var expiredEffects = new List<StatusEffectType>();
        
        foreach (var effect in activeEffects.Values)
        {
            effect.Tick();
            if (effect.IsExpired)
            {
                expiredEffects.Add(effect.Type);
            }
        }

        foreach (var type in expiredEffects)
        {
            RemoveEffect(type);
        }
    }

    public void AddEffect(IStatusEffect effect)
    {
        if (activeEffects.ContainsKey(effect.Type))
        {
            RemoveEffect(effect.Type);
        }

        activeEffects[effect.Type] = effect;
        effect.Apply(unit);
    }

    public void RemoveEffect(StatusEffectType type)
    {
        if (activeEffects.TryGetValue(type, out var effect))
        {
            effect.Remove();
            activeEffects.Remove(type);
        }
    }

    public bool HasEffect(StatusEffectType type)
    {
        return activeEffects.ContainsKey(type);
    }

    public bool CanAct()
    {
        return !IsKnockedUp && !IsStunned;
    }

    public IStatusEffect GetEffect(StatusEffectType type)
    {
        activeEffects.TryGetValue(type, out var effect);
        return effect;
    }
} 
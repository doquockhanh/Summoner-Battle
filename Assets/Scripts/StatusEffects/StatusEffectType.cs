public enum StatusEffectType
{
    None,
    Knockup,
    Stun,
    Slow,
    SpeedBuff,
    DamageBuff,
    DefenseBuff,
    Shield,
    Lifesteal,
    Bloodstorm,
    Burning,
    StealthOnKill,
}

public interface IStatusEffect
{
    StatusEffectType Type { get; }
    float Duration { get; }
    float RemainingTime { get; }
    bool IsExpired { get; }
    void Apply(Unit target);
    void Remove();
    void Tick();
} 

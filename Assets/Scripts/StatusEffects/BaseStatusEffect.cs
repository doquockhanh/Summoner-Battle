using UnityEngine;

public abstract class BaseStatusEffect : IStatusEffect
{
    protected Unit owner;
    protected float duration;
    protected float remainingTime;
    protected StatusEffectType type;

    public StatusEffectType Type => type;
    public float Duration => duration;
    public float RemainingTime => remainingTime;
    public bool IsExpired => remainingTime <= 0;
    public event System.Action OnExpired;

    protected BaseStatusEffect(float duration)
    {
        this.duration = duration;
        this.remainingTime = duration;
    }

    public virtual void Apply(Unit owner)
    {
        this.owner = owner;
    }

    public virtual void Remove()
    {
        OnExpired?.Invoke();
        // Override để xử lý cleanup
    }

    public virtual void Tick()
    {
        remainingTime -= Time.deltaTime;
    }
} 
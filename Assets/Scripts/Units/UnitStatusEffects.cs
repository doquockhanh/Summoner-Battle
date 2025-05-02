using System.Collections.Generic;
using UnityEngine;

public class UnitStatusEffects : MonoBehaviour
{
    private Dictionary<StatusEffectType, IStatusEffect> activeEffects = new Dictionary<StatusEffectType, IStatusEffect>();
    private Unit unit;
    private HealthBarUI healthBarUI;
    private Dictionary<StatusEffectType, GameObject> effectIcons = new Dictionary<StatusEffectType, GameObject>();

    public bool IsKnockedUp => HasEffect(StatusEffectType.Knockup);
    public bool IsStunned => HasEffect(StatusEffectType.Stun);
    public bool IsSlowed => HasEffect(StatusEffectType.Slow);
    public bool IsTargetable => !HasEffect(StatusEffectType.Untargetable);

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        healthBarUI = unit.GetComponent<UnitView>().GetHealthBar();
    }

    private void FixedUpdate()
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
        UpdateStatusEffectsUI();
    }

    public void RemoveEffect(StatusEffectType type)
    {
        if (activeEffects.TryGetValue(type, out var effect))
        {
            effect.Remove();
            activeEffects.Remove(type);
            UpdateStatusEffectsUI();
        }
    }

    private void RemoveStatusEffectsUI()
    {
        foreach (var icon in effectIcons.Values)
        {
            Destroy(icon);
        }
        effectIcons.Clear();
    }

    private void UpdateStatusEffectsUI()
    {
        if (healthBarUI == null) return;

        RemoveStatusEffectsUI();

        float xOffset = 0f;
        foreach (var effect in activeEffects)
        {
            string iconPath = $"EffectIcons/{effect.Key}";
            Sprite iconSprite = Resources.Load<Sprite>(iconPath);

            if (iconSprite != null)
            {
                GameObject iconObject = new GameObject($"{effect.Key}Icon");
                iconObject.transform.SetParent(healthBarUI.transform);

                SpriteRenderer spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = iconSprite;
                spriteRenderer.sortingOrder = HealthBarManager.Instance.GetCanvas().sortingOrder;
                spriteRenderer.sortingLayerName = UnitSortingOrder.SORTING_Y_LAYER;

                // Đặt vị trí icon phía trên thanh máu
                Vector3 position = healthBarUI.transform.position;
                position.y += 0.3f; // Điều chỉnh khoảng cách từ thanh máu
                position.x -= 0.4f;
                position.x += xOffset;
                iconObject.transform.position = position;

                // Đặt kích thước icon
                iconObject.transform.localScale = new Vector3(1f, 1f, 1f);

                effectIcons[effect.Key] = iconObject;
                float iconSpacing = 0.3f;
                xOffset += iconSpacing;
            }
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

    public void ResetStatusEffect()
    {
        foreach (var effect in activeEffects.Values)
        {
            effect.Remove();
        }
        activeEffects.Clear();
        RemoveStatusEffectsUI();
    }
}
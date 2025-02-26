using UnityEngine;
using System.Collections.Generic;

public class ShieldEffectHandler : MonoBehaviour
{
    [Header("Absorption Shield")]
    [SerializeField] private float healingPercent = 0.5f; // % chuyển thành máu
    [SerializeField] private ParticleSystem healEffect;

    [Header("Reflective Shield")]
    [SerializeField] private float reflectPercent = 0.3f; // % sát thương phản lại
    [SerializeField] private float reflectRadius = 3f;
    [SerializeField] private ParticleSystem reflectEffect;

    [Header("Sharing Shield")]
    [SerializeField] private float sharingRadius = 5f;
    [SerializeField] private float sharedPercent = 0.5f; // % shield chia sẻ
    [SerializeField] private ParticleSystem shareEffect;

    private UnitStats stats;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
    }

    public void HandleShieldAbsorbed(ShieldLayer shield, float absorbedAmount)
    {
        switch (shield.Type)
        {
            case ShieldType.Absorption:
                ProcessAbsorptionEffect(absorbedAmount);
                break;
            case ShieldType.Reflective:
                ProcessReflectiveEffect(shield.Source, absorbedAmount);
                break;
        }
    }

    public void HandleShieldExpired(ShieldLayer shield)
    {
        switch (shield.Type)
        {
            case ShieldType.Sharing:
                ProcessSharingEffect(shield.RemainingValue);
                break;
        }
    }

    private void ProcessAbsorptionEffect(float absorbedAmount)
    {
        float healAmount = absorbedAmount * healingPercent;
        stats.Heal(healAmount);
        
        if (healEffect != null)
        {
            healEffect.Play();
        }
    }

    private void ProcessReflectiveEffect(Unit source, float absorbedAmount)
    {
        float reflectDamage = absorbedAmount * reflectPercent;
        
        // Gây sát thương cho kẻ tấn công
        if (source != null)
        {
            source.GetComponent<UnitStats>()?.TakeDamage(reflectDamage, DamageType.Magic, GetComponent<Unit>());
        }

        // Gây sát thương AOE
        var colliders = Physics2D.OverlapCircleAll(transform.position, reflectRadius);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<UnitStats>(out var targetStats))
            {
                if (targetStats != stats) // Không gây sát thương cho bản thân
                {
                    targetStats.TakeDamage(reflectDamage * 0.5f, DamageType.Magic, GetComponent<Unit>()); // Sát thương AOE giảm 50%
                }
            }
        }

        if (reflectEffect != null)
        {
            reflectEffect.Play();
        }
    }

    private void ProcessSharingEffect(float remainingShield)
    {
        if (remainingShield <= 0) return;

        var colliders = Physics2D.OverlapCircleAll(transform.position, sharingRadius);
        List<UnitStats> allies = new List<UnitStats>();

        // Tìm đồng minh trong phạm vi
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<UnitStats>(out var allyStats))
            {
                if (allyStats != stats && IsAlly(allyStats)) // Kiểm tra đồng minh
                {
                    allies.Add(allyStats);
                }
            }
        }

        if (allies.Count > 0)
        {
            float sharedAmount = (remainingShield * sharedPercent) / allies.Count;
            foreach (var ally in allies)
            {
                ally.AddShield(sharedAmount, 5f, ShieldType.Normal); // Shield chia sẻ tồn tại 5 giây
            }

            if (shareEffect != null)
            {
                shareEffect.Play();
            }
        }
    }

    private bool IsAlly(UnitStats other)
    {
        // Kiểm tra đồng minh dựa theo team/faction
        return other.gameObject.layer == gameObject.layer;
    }
} 
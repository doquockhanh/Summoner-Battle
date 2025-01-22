using UnityEngine;

public class SkillEffectHandler : MonoBehaviour, ISkillEffect
{
    private Card cardData;
    
    public void Initialize(Card card)
    {
        this.cardData = card;
    }

    public void ApplyEffect(Unit target)
    {
        if (target == null) return;
        
        if (cardData.skill.damage > 0)
        {
            target.TakeDamage(cardData.skill.damage);
            ShowDamageNumber(target.transform.position, cardData.skill.damage);
        }
        
        if (cardData.skill.healing > 0)
        {
            target.TakeDamage(-cardData.skill.healing);
            ShowHealNumber(target.transform.position, cardData.skill.healing);
        }
        
        if (cardData.skill.buffDuration > 0 && cardData.skill.buffAmount != 0)
        {
           target.AddEffect(DetermineEffectType(), cardData.skill.buffDuration, cardData.skill.buffAmount);
        }
        
        ShowVisualEffect(target.transform.position);
    }

    public void ShowVisualEffect(Vector3 position)
    {
        if (cardData.skill.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(cardData.skill.skillEffectPrefab, 
                position, 
                Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    private void ShowDamageNumber(Vector3 position, float amount)
    {
        FloatingTextManager.Instance.ShowFloatingText(
            amount.ToString("F0"), 
            position, 
            Color.red
        );
    }
    
    private void ShowHealNumber(Vector3 position, float amount)
    {
        FloatingTextManager.Instance.ShowFloatingText(
            amount.ToString("F0"), 
            position, 
            Color.green
        );
    }
    
    private EffectType DetermineEffectType()
    {
        if (cardData.skill.damage > 0)
            return EffectType.DamageBoost;
        else if (cardData.skill.buffAmount > 0)
            return EffectType.SpeedBoost;
        else
            return EffectType.DefenseBoost;
    }
} 
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image rageBar;
    
    private SkillData skillData;
    private float currentCooldown;
    private float currentRage;
    
    public void Setup(SkillData data)
    {
        skillData = data;
        skillIcon.sprite = data.skillIcon;
        UpdateUI(0, 0);
    }
    
    public void UpdateUI(float cooldown, float rage)
    {
        currentCooldown = cooldown;
        currentRage = rage;
        
        // Cập nhật cooldown overlay
        cooldownOverlay.fillAmount = cooldown / skillData.cooldown;
        
        // Cập nhật rage bar
        rageBar.fillAmount = rage / skillData.rageCost;
    }
} 
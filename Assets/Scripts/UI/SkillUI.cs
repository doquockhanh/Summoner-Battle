using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image manabar;
    
    private SkillData skillData;
    private float currentMana;
    
    public void Setup(SkillData data)
    {
        skillData = data;
        skillIcon.sprite = data.skillIcon;
        UpdateUI(0);
    }
    
    public void UpdateUI(float mana)
    {
        currentMana = mana;
        

        // Cập nhật rage bar
        manabar.fillAmount = mana / skillData.manaCost;
    }
} 
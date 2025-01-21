using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Base : MonoBehaviour
{
    [SerializeField] private bool isPlayerBase;
    public bool IsPlayerBase => isPlayerBase;
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    
    private float currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            BattleManager.Instance.EndGame(!isPlayerBase);
        }
    }
    
    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
            
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{maxHealth}";
    }
} 
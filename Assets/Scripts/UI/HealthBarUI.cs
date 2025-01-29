using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image damageDelayedBar;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image shieldBar;
    
    [Header("Settings")]
    [SerializeField] private float damageDelayTime = 0.35f;
    [SerializeField] private float damageDelaySpeed = 2f;
    
    private float maxHp;
    private float currentHp;
    private float delayedHp;
    private float currentShield;
    private Coroutine damageDelayCoroutine;
    
    public Color healthColor = new Color(0.3f, 0.8f, 0.3f);
    public Color shieldColor = new Color(0.8f, 0.8f, 1f, 0.8f);
    public Color delayedColor = new Color(0.4f, 0.4f, 0.4f);

    public void Initialize(float maxHealth)
    {
        maxHp = maxHealth;
        currentHp = maxHealth;
        delayedHp = maxHealth;
        currentShield = 0;
        
        SetupBars();
    }

    private void SetupBars()
    {
        backgroundBar.transform.SetSiblingIndex(0);
        damageDelayedBar.transform.SetSiblingIndex(1);
        healthBar.transform.SetSiblingIndex(2);
        shieldBar.transform.SetSiblingIndex(3);
        
        backgroundBar.color = Color.black;
        healthBar.color = healthColor;
        shieldBar.color = shieldColor;
        damageDelayedBar.color = delayedColor;
        
        healthBar.fillAmount = 1f;
        damageDelayedBar.fillAmount = 1f;
        shieldBar.fillAmount = 0f;
        shieldBar.gameObject.SetActive(true);
    }

    public void UpdateHealth(float newHp)
    {
        float oldHpPercent = currentHp / maxHp;
        float newHpPercent = newHp / maxHp;
        
        currentHp = newHp;
        
        if (newHpPercent < oldHpPercent)
        {
            if (damageDelayCoroutine != null)
            {
                StopCoroutine(damageDelayCoroutine);
            }
            damageDelayCoroutine = StartCoroutine(UpdateDamageDelayBar(oldHpPercent));
        }
        else
        {
            delayedHp = currentHp;
            damageDelayedBar.fillAmount = newHpPercent;
        }

        UpdateShieldDisplay();
    }

    public void UpdateShield(float shieldAmount)
    {
        currentShield = shieldAmount;
        UpdateShieldDisplay();
    }

    private void UpdateShieldDisplay()
    {
        float totalHealth = currentHp + currentShield;
        float totalPercent = Mathf.Min(totalHealth / maxHp, 1f);
        
        float healthPercent = currentHp / maxHp;
        float shieldPercent = currentShield / maxHp;
        
        healthBar.fillAmount = healthPercent;
        shieldBar.fillAmount = shieldPercent;
        
        Color shieldColorWithAlpha = shieldColor;
        shieldColorWithAlpha.a = shieldPercent > 0 ? 0.8f : 0f;
        shieldBar.color = shieldColorWithAlpha;
    }

    private IEnumerator UpdateDamageDelayBar(float startPercent)
    {
        damageDelayedBar.fillAmount = startPercent;
        yield return new WaitForSeconds(damageDelayTime);
        
        float targetFill = currentHp / maxHp;
        
        while (damageDelayedBar.fillAmount > targetFill)
        {
            damageDelayedBar.fillAmount = Mathf.MoveTowards(
                damageDelayedBar.fillAmount,
                targetFill,
                damageDelaySpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
} 
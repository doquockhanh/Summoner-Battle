using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image damageDelayedBar;
    [SerializeField] private Image healthBar;
    [SerializeField] private Transform shieldBarContainer;
    [SerializeField] private Image shieldBar;
    [SerializeField] private TextMeshProUGUI soulCountText;

    [Header("Settings")]
    [SerializeField] private float damageDelayTime = 0.5f;
    [SerializeField] private float damageDelaySpeed = 2f;

    [Header("Shield Colors")]
    [SerializeField] private Color normalShieldColor = new Color(0.7f, 0.7f, 1f, 0.8f);
    [SerializeField] private Color absorptionShieldColor = new Color(0.7f, 1f, 0.7f, 0.8f);
    [SerializeField] private Color reflectiveShieldColor = new Color(1f, 0.7f, 0.7f, 0.8f);
    [SerializeField] private Color sharingShieldColor = new Color(1f, 1f, 0.7f, 0.8f);

    private float maxHp;
    private float currentHp;
    private float delayedHp;
    private Dictionary<ShieldType, Image> shieldBars = new Dictionary<ShieldType, Image>();
    private Coroutine damageDelayCoroutine;

    public Color healthColor = new Color(0.3f, 0.8f, 0.3f);
    public Color delayedColor = new Color(0.4f, 0.4f, 0.4f);

    public void Initialize(float maxHealth)
    {
        maxHp = maxHealth;
        currentHp = maxHealth;
        delayedHp = maxHealth;
        ClearShieldBars();

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

        healthBar.fillAmount = 1f;
        damageDelayedBar.fillAmount = 1f;
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

        healthBar.fillAmount = newHpPercent;
    }

    public void UpdateShields(List<ShieldLayer> shields)
    {
        float totalShieldPercent = 0;

        foreach (var shield in shields)
        {
            if (shield.RemainingValue <= 0) continue;

            if (!shieldBars.TryGetValue(shield.Type, out Image shieldBar))
            {
                shieldBar = CreateShieldBar(shield.Type);
                shieldBars[shield.Type] = shieldBar;
            }

            float shieldPercent = shield.RemainingValue / maxHp;
            shieldBar.fillAmount = shieldPercent;
            shieldBar.transform.SetAsLastSibling();

            var rectTransform = shieldBar.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, totalShieldPercent * rectTransform.rect.height);

            totalShieldPercent += shieldPercent;

            Color shieldColorWithAlpha = shieldBar.color;
            shieldColorWithAlpha.a = totalShieldPercent > 0 ? 0.8f : 0f;
            shieldBar.color = shieldColorWithAlpha;
        }

        var unusedTypes = new List<ShieldType>();
        foreach (var kvp in shieldBars)
        {
            if (!shields.Exists(s => s.Type == kvp.Key && s.RemainingValue > 0))
            {
                unusedTypes.Add(kvp.Key);
            }
        }

        foreach (var type in unusedTypes)
        {
            Destroy(shieldBars[type].gameObject);
            shieldBars.Remove(type);
        }
    }

    private Image CreateShieldBar(ShieldType type)
    {
        var newBar = Instantiate(shieldBar, shieldBarContainer);
        newBar.color = GetShieldColor(type);
        return newBar;
    }

    private Color GetShieldColor(ShieldType type)
    {
        return type switch
        {
            ShieldType.Normal => normalShieldColor,
            ShieldType.Absorption => absorptionShieldColor,
            ShieldType.Reflective => reflectiveShieldColor,
            ShieldType.Sharing => sharingShieldColor,
            _ => normalShieldColor
        };
    }

    private void ClearShieldBars()
    {
        foreach (var bar in shieldBars.Values)
        {
            Destroy(bar.gameObject);
        }
        shieldBars.Clear();
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

    public void ShowSoulCounter(bool show)
    {
        if (soulCountText != null)
        {
            soulCountText.gameObject.SetActive(show);
        }
    }

    public void UpdateSoulCount(int count)
    {
        if (soulCountText != null)
        {
            soulCountText.text = $"{count}";
        }
    }
}
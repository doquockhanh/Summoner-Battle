using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    public static readonly Color AllyHealth = Color.green;
    public static readonly Color AllyDelayed = Color.red;
    public static readonly Color EnemyHealth = Color.red;
    public static readonly Color EnemyDelayed = Color.green;

    [Header("References")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private bool useWorldSpaceCanvas = true;

    private float maxHp;

    private void Awake()
    {
        if (healthBarUI == null && healthBarPrefab != null)
        {
            Transform parent = useWorldSpaceCanvas && HealthBarManager.Instance != null
                ? HealthBarManager.Instance.GetCanvasTransform()
                : this.transform;
            var healthBarObj = Instantiate(healthBarPrefab, parent);
            healthBarUI = healthBarObj.GetComponent<HealthBarUI>();
            healthBarUI.transform.SetParent(parent, true);
        }
    }

    public void Initialize(float maxHp, bool isAlly = true)
    {
        this.maxHp = maxHp;
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(maxHp);
            if (isAlly)
                healthBarUI.SetBarColors(AllyHealth, AllyDelayed);
            else
                healthBarUI.SetBarColors(EnemyHealth, EnemyDelayed);
            UpdateHealthBarPosition();
        }
    }

    public void UpdateHealth(float newHp, float? overrideMaxHp = null)
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(newHp, overrideMaxHp ?? maxHp);
        }
    }

    public void UpdateShield(float shieldAmount)
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateShield(shieldAmount);
        }
    }

    public void Show()
    {
        if (healthBarUI != null)
            healthBarUI.Show();
    }

    public void Hide()
    {
        if (healthBarUI != null)
            healthBarUI.Hide();
    }

    private void LateUpdate()
    {
        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarUI != null)
        {
            Vector3 worldPosition = GetHealthBarPosition();
            healthBarUI.transform.position = worldPosition;
        }
    }

    private Vector3 GetHealthBarPosition()
    {
        // Nếu gắn cho Unit thì có thể override hàm này để lấy vị trí top của sprite
        // Nếu gắn cho Card thì chỉ lấy transform.position hoặc custom offset
        return transform.position;
    }

    public HealthBarUI GetHealthBarUI() => healthBarUI;
} 
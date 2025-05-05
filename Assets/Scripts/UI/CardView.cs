using UnityEngine;

public class CardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBarController healthBarController;

    private CardStats cardStats;

    // Gọi khi khởi tạo card (ví dụ khi load vào inventory, shop, v.v.)
    public void Initialize()
    {
        if (healthBarController != null)
        { 
            cardStats = GetComponent<CardStats>();
            cardStats.OnHealthChanged += SetHealth;
        }
    }

    // Nếu muốn cập nhật máu động (ví dụ preview damage)
    public void SetHealth(float hp)
    {
        if (healthBarController != null)
        {
            healthBarController.UpdateHealth(hp, cardStats.GetMaxHp());
        }

    }

    public void SetShield(float shield)
    {
        if (healthBarController != null)
            healthBarController.UpdateShield(shield);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Hiển thị 1 item trong inventory và xử lý sự kiện double click để sử dụng item.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image icon;
    public TMP_Text quantityText;

    private Item item;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // giây

    /// <summary>
    /// Khởi tạo slot với dữ liệu item và player.
    /// </summary>
    public void Setup(Item item)
    {
        this.item = item;
        icon.sprite = item.data.icon;
        quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
    }

    /// <summary>
    /// Bắt sự kiện click để phát hiện double click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.unscaledTime - lastClickTime;
        if (timeSinceLastClick <= doubleClickThreshold)
        {
            // Double click: sử dụng item
            ItemUseHelper.UseItem(item);
        }
        lastClickTime = Time.unscaledTime;
    }
}
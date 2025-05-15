using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;
    [TextArea]
    public string description;
    // Có thể mở rộng thêm các trường khác nếu cần
} 
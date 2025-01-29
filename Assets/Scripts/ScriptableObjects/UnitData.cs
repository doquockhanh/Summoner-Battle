using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit")]
public class UnitData : ScriptableObject
{
    [Header("Unit Info")]
    public string unitName;
    public Sprite unitImage;
    [TextArea]
    public string description;

    [Header("Stats")]
    public float hp;
    public float damage;
    public float attackSpeed;
    public float moveSpeed;
    public float range;        // Tầm đánh
    public float detectRange;  // Tầm phát hiện kẻ địch
    public float lifestealPercent; // Phần trăm hút máu (0-1)

    // Thêm reference đến prefab
    public GameObject unitPrefab;

    private void OnValidate()
    {
        // Đảm bảo các giá trị không âm
        hp = Mathf.Max(0, hp);
        damage = Mathf.Max(0, damage);
        attackSpeed = Mathf.Max(0.1f, attackSpeed);
        moveSpeed = Mathf.Max(0, moveSpeed);
        range = Mathf.Max(0.1f, range);
        detectRange = Mathf.Max(range, detectRange); // Tầm phát hiện phải >= tầm đánh
    }
} 
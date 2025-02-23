using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card")]
public class Card : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public Sprite cardImage;
    [TextArea]
    public string description;
    
    [Header("Stats")]
    public float maxMana = 100f;
    public float manaRegen = 5f; // Hồi mana/giây
    public float spawnCooldown; // Thời gian chờ giữa các lần gọi unit
    
    [Header("Unit Data")]
    public UnitData summonUnit;
    
    [Header("Skill")]
    public Skill skill;
    
    
    private void OnValidate()
    {
        // Đảm bảo dữ liệu hợp lệ
        maxMana = Mathf.Max(0, maxMana);
        manaRegen = Mathf.Max(0, manaRegen);
        spawnCooldown = Mathf.Max(0, spawnCooldown);
    }
    
    private void OnEnable()
    {
        // Khởi tạo lại dữ liệu khi load
        hideFlags = HideFlags.None;
    }
} 
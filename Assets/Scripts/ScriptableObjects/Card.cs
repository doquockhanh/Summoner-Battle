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
    public float mana;
    public float rageGainRate;
    public float spawnCooldown; // Thời gian chờ giữa các lần gọi unit
    
    [Header("Unit Data")]
    public UnitData summonUnit;
    
    private void OnValidate()
    {
        // Đảm bảo dữ liệu hợp lệ
        mana = Mathf.Max(0, mana);
        rageGainRate = Mathf.Max(0, rageGainRate);
        spawnCooldown = Mathf.Max(0, spawnCooldown);
    }
    
    private void OnEnable()
    {
        // Khởi tạo lại dữ liệu khi load
        hideFlags = HideFlags.None;
    }
} 
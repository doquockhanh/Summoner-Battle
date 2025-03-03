using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit")]
public class UnitData : ScriptableObject
{
    [Header("Unit Info")]
    public string unitName;
    public Sprite unitImage;
    [TextArea]
    public string description;

    [Header("Health & Defense")]
    public float maxHp;

    [Range(0f, 100f)]   
    public float hpRegen;

    [Range(0f, 500f)]
    public float armor;// Giáp

    [Range(0f, 500f)]
    public float magicResist;        // Kháng phép

    [Range(0f, 100f)]
    public float damageReduction;    // % Giảm sát thương cuối cùng

    [Range(0f, 100f)]
    public float tenacity;           // % Giảm thời gian khống chế

    [Header("Offensive Stats")]
    [Range(0f, 500f)]
    public float physicalDamage;     // Sát thương vật lý

    [Range(0f, 500f)]
    public float magicDamage;        // Sát thương phép

    [Range(0f, 100f)]
    public float damageAmplification; // % Tăng sát thương cuối cùng

    [Range(0f, 100f)]
    public float armorPenetration;   // % Xuyên giáp

    [Range(0f, 100f)]
    public float magicPenetration;   // % Xuyên kháng phép

    [Range(0f, 100f)]
    public float criticalChance;     // % Tỉ lệ chí mạng

    [Range(150f, 500f)]
    public float criticalDamage;     // % Tăng sát thương khi chí mạng

    [Range(0f, 100f)]
    public float lifestealPercent;   // % Hút máu

    [Header("Utility")]
    [Range(0f, 5f)]
    public float attackSpeed;

    [Range(0f, 10f)]
    public float moveSpeed;

    [Range(0f, 8f)]
    public float range;        // Tầm đánh

    [Range(0f, 10f)]
    public float detectRange;  // Tầm phát hiện kẻ địch
    public float healingReceivedPercent = 100f; // % Tăng lượng hồi máu nhận được

    // Thêm reference đến prefab
    public GameObject unitPrefab;

    private void OnValidate()
    {
        // Giới hạn các giá trị
        maxHp = Mathf.Max(1, maxHp);
        hpRegen = Mathf.Max(0, hpRegen);
        armor = Mathf.Max(0, armor);
        magicResist = Mathf.Max(0, magicResist);
        damageReduction = Mathf.Clamp01(damageReduction);
        tenacity = Mathf.Clamp01(tenacity);
        
        physicalDamage = Mathf.Max(0, physicalDamage);
        magicDamage = Mathf.Max(0, magicDamage);
        damageAmplification = Mathf.Max(0, damageAmplification);
        armorPenetration = Mathf.Max(0, armorPenetration);
        magicPenetration = Mathf.Max(0, magicPenetration);
        criticalChance = Mathf.Max(0, criticalChance);
        criticalDamage = Mathf.Max(150f, criticalDamage);
        lifestealPercent = Mathf.Max(0, lifestealPercent);

        attackSpeed = Mathf.Max(0.1f, attackSpeed);
        moveSpeed = Mathf.Max(0, moveSpeed);
        range = Mathf.Max(0.1f, range);
        detectRange = Mathf.Max(range, detectRange);
        healingReceivedPercent = Mathf.Max(0, healingReceivedPercent);
    }
} 
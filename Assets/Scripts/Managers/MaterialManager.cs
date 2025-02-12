using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance { get; private set; }

    [Header("Outline Materials")]
    [SerializeField] private Material allyOutlineMaterial;
    [SerializeField] private Material enemyOutlineMaterial;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Material GetUnitMaterial(bool isPlayerUnit)
    {
        return isPlayerUnit ? allyOutlineMaterial : enemyOutlineMaterial;
    }

    public void Initialize()
    {
        if (allyOutlineMaterial == null)
            allyOutlineMaterial = Resources.Load<Material>("Materials/AllyOutline");
        if (enemyOutlineMaterial == null)
            enemyOutlineMaterial = Resources.Load<Material>("Materials/EnemyOutline");
    }
} 
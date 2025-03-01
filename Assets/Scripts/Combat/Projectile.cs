using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Cài đặt cơ bản")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Tham chiếu")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Material projectileMaterial;

    private Unit target;
    private Unit source;
    private float damage;
    private bool isInitialized;
    private float lifetime;

    public void Initialize(Unit target, float damage, Color projectileColor, Unit source)
    {
        this.target = target;
        this.damage = damage;
        this.source = source;
        this.lifetime = 0f;

        if (projectileMaterial != null)
        {
            // Gán material instance mới để tránh ảnh hưởng tới prefab
            Material instanceMaterial = new Material(projectileMaterial);
            instanceMaterial.SetColor("_ProjectileColor", projectileColor);
            spriteRenderer.material = instanceMaterial;
        }

        if (trailRenderer != null)
        {
            // Tạo gradient mới
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

            // Đặt màu bắt đầu
            colorKeys[0] = new GradientColorKey(projectileColor, 0f);
            colorKeys[1] = new GradientColorKey(projectileColor, 1f);

            // Đặt độ trong suốt
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            trailRenderer.colorGradient = gradient;
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        // Di chuyển đến target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Xoay projectile theo hướng di chuyển
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Kiểm tra va chạm
        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage, DamageType.Physical, source);
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
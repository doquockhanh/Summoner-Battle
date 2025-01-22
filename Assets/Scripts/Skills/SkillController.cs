using UnityEngine;
using System.Collections;

public class SkillController : MonoBehaviour
{
    [SerializeField] private GameObject hitboxPrefab;
    private SkillHitboxVisual hitboxVisual;
    private bool isShowingHitbox = false;
    
    private Card cardData;
    private bool isPlayer;
    private float skillCooldown;
    
    public void Initialize(Card card, bool isPlayer)
    {
        this.cardData = card;
        this.isPlayer = isPlayer;
        skillCooldown = 0;
    }
    
    private void Start()
    {
        // Tạo hitbox từ prefab nếu có
        if (hitboxPrefab != null && hitboxVisual == null)
        {
            GameObject hitboxObj = Instantiate(hitboxPrefab);
            hitboxVisual = hitboxObj.GetComponent<SkillHitboxVisual>();
            hitboxObj.transform.SetParent(null);
            hitboxObj.SetActive(false);
            Debug.Log("[Skill] Đã tạo hitbox visual");
        }
        // Tạo hitbox từ code nếu không có prefab
        else if (hitboxVisual == null)
        {
            GameObject hitboxObj = new GameObject("SkillHitbox");
            hitboxObj.transform.SetParent(null);
            
            SpriteRenderer renderer = hitboxObj.AddComponent<SpriteRenderer>();
            hitboxVisual = hitboxObj.AddComponent<SkillHitboxVisual>();
            
            // Tạo sprite hình tròn đơn giản
            int resolution = 128;
            Texture2D texture = new Texture2D(resolution, resolution);
            Vector2 center = new Vector2(resolution / 2, resolution / 2);
            
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = distance < (resolution / 2) ? 0.3f : 0f;
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
            texture.Apply();
            
            Sprite circleSprite = Sprite.Create(texture, 
                new Rect(0, 0, resolution, resolution), 
                new Vector2(0.5f, 0.5f), 
                100f);
                
            renderer.sprite = circleSprite;
            renderer.sortingOrder = 5;
            
            hitboxObj.SetActive(false);
            Debug.Log("[Skill] Đã tạo hitbox visual từ code");
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup hitbox khi destroy SkillController
        if (hitboxVisual != null)
        {
            Destroy(hitboxVisual.gameObject);
        }
    }
    
    public bool CanUseSkill(float currentRage)
    {
        return skillCooldown <= 0 && currentRage >= cardData.skill.rageCost;
    }
    
    public void UseSkill()
    {
        if (cardData.skill == null)
        {
            return;
        }
        
        switch (cardData.skill.targetType)
        {
            case TargetType.SingleTarget:
                HandleSingleTargetSkill();
                break;
            case TargetType.AOE:
                HandleAOESkill();
                break;
            case TargetType.Ally:
                HandleAllySkill();
                break;
            case TargetType.AllAllies:
                HandleAllAlliesSkill();
                break;
        }
        
        skillCooldown = cardData.skill.cooldown;
        PlaySkillEffects();
    }
    
    private void HandleSingleTargetSkill()
    {
        Unit bestTarget = FindBestSingleTarget();
        if (bestTarget != null)
        {
            Vector3 targetWorldPos = bestTarget.transform.position;
            
            if (hitboxVisual != null)
            {
                hitboxVisual.ShowHitbox(TargetType.SingleTarget, 0.5f, targetWorldPos);
                isShowingHitbox = true;
                StartCoroutine(HideHitboxAfterDelay(1f));
            }
            
            ApplySkillEffect(bestTarget);
        }
    }
    
    private Unit FindBestSingleTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, cardData.skill.targetRadius);
        Unit bestTarget = null;
        float bestScore = float.MinValue;
        
        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit != isPlayer)
            {
                float score = EvaluateTarget(unit);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = unit;
                }
            }
        }
        
        return bestTarget;
    }
    
    private float EvaluateTarget(Unit unit)
    {
        float score = 0;
        UnitData data = unit.GetUnitData();
        
        // Đánh giá dựa trên máu còn lại
        score += (unit.GetCurrentHP() / data.hp) * 0.4f;
        
        // Đánh giá dựa trên sát thương
        score += (data.damage / 100f) * 0.3f;
        
        // Đánh giá dựa trên khoảng cách đến base
        float distanceToBase = Vector2.Distance(unit.transform.position, GetAlliedBase().transform.position);
        score += (1 - distanceToBase/10f) * 0.3f;
        
        return score;
    }
    
    private void HandleAOESkill()
    {
        Vector2 bestPosition = FindBestAOEPosition();
        
        if (hitboxVisual != null)
        {
            hitboxVisual.transform.position = new Vector3(bestPosition.x, bestPosition.y, 0);
            hitboxVisual.ShowHitbox(TargetType.AOE, cardData.skill.targetRadius, bestPosition);
            isShowingHitbox = true;
            StartCoroutine(HideHitboxAfterDelay(1f));
        }
        
        // Áp dụng hiệu ứng cho các unit trong vùng
        Collider2D[] targets = Physics2D.OverlapCircleAll(bestPosition, cardData.skill.targetRadius);
        int affectedTargets = 0;
        
        foreach (Collider2D collider in targets)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit != isPlayer)
            {
                affectedTargets++;
                ApplySkillEffect(unit);
            }
        }
    }
    
    private Vector2 FindBestAOEPosition()
    {
        // Tìm tất cả unit trong phạm vi lớn của world space
        Collider2D[] allUnits = Physics2D.OverlapCircleAll(Vector2.zero, 50f); // Phạm vi tìm kiếm rộng
        
        Vector2 bestPosition = Vector2.zero;
        int maxTargets = 0;
        float highestThreatLevel = 0f;
        
        // Kiểm tra từng unit enemy làm tâm cho kỹ năng AOE
        foreach (Collider2D collider in allUnits)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit == null || unit.IsPlayerUnit == isPlayer) continue;
            
            Vector2 testPosition = unit.transform.position;
            // Tìm các unit khác trong phạm vi skill radius
            Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(testPosition, cardData.skill.targetRadius);
            
            int targetCount = 0;
            float threatLevel = 0f;
            
            foreach (Collider2D nearby in nearbyUnits)
            {
                Unit nearbyUnit = nearby.GetComponent<Unit>();
                if (nearbyUnit != null && nearbyUnit.IsPlayerUnit != isPlayer)
                {
                    targetCount++;
                    threatLevel += EvaluateTarget(nearbyUnit);
                }
            }
            
            
            if (targetCount > maxTargets || 
                (targetCount == maxTargets && threatLevel > highestThreatLevel))
            {
                maxTargets = targetCount;
                highestThreatLevel = threatLevel;
                bestPosition = testPosition;
            }
        }
        
        return bestPosition;
    }
    
    private void HandleAllySkill()
    {
        Unit bestAlly = FindBestAlly();
        if (bestAlly != null)
        {
            Vector3 allyWorldPos = bestAlly.transform.position;
            
            if (hitboxVisual != null)
            {
                hitboxVisual.ShowHitbox(TargetType.Ally, cardData.skill.targetRadius, allyWorldPos);
                isShowingHitbox = true;
                StartCoroutine(HideHitboxAfterDelay(1f));
            }
            
            ApplySkillEffect(bestAlly);
        }
    }
    
    private Unit FindBestAlly()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, cardData.skill.targetRadius);
        Unit bestTarget = null;
        float bestScore = float.MinValue;
        
        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit == isPlayer)
            {
                float score = EvaluateAlly(unit);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = unit;
                }
            }
        }
        
        return bestTarget;
    }
    
    private float EvaluateAlly(Unit unit)
    {
        float score = 0;
        UnitData data = unit.GetUnitData();
        
        // Đánh giá dựa trên % máu còn lại
        float healthPercent = unit.GetCurrentHP() / data.hp;
        score += (1 - healthPercent) * 0.4f; // Unit có ít máu được ưu tiên cao hơn
        
        // Đánh giá dựa trên sát thương
        score += (data.damage / 100f) * 0.3f;
        
        // Đánh giá dựa trên khoảng cách đến base địch
        Base enemyBase = GetEnemyBase();
        if (enemyBase != null)
        {
            float distanceToEnemyBase = Vector2.Distance(unit.transform.position, enemyBase.transform.position);
            score += (1 - distanceToEnemyBase/10f) * 0.3f; // Unit gần base địch được ưu tiên
        }
        
        return score;
    }
    
    private void HandleAllAlliesSkill()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, cardData.skill.targetRadius);
        
        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit == isPlayer)
            {
                ApplySkillEffect(unit);
            }
        }
    }
    
    private Base GetAlliedBase()
    {
        Base[] bases = FindObjectsOfType<Base>();
        foreach (Base b in bases)
        {
            if (b.IsPlayerBase == isPlayer)
                return b;
        }
        return null;
    }
    
    private Base GetEnemyBase()
    {
        Base[] bases = FindObjectsOfType<Base>();
        foreach (Base b in bases)
        {
            if (b.IsPlayerBase != isPlayer)
                return b;
        }
        return null;
    }
    
    private void ApplySkillEffect(Unit target)
    {
        if (target == null)
        {
            return;
        }
        
        if (cardData.skill.damage > 0)
        {
            target.TakeDamage(cardData.skill.damage);
            ShowDamageNumber(target.transform.position, cardData.skill.damage);
        }
        
        if (cardData.skill.healing > 0)
        {
            target.TakeDamage(-cardData.skill.healing);
            ShowHealNumber(target.transform.position, cardData.skill.healing);
        }
        
        if (cardData.skill.buffDuration > 0 && cardData.skill.buffAmount != 0)
        {
           target.AddEffect(DetermineEffectType(), cardData.skill.buffDuration, cardData.skill.buffAmount);
        }
        
        // Visual effects
        if (cardData.skill.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(cardData.skill.skillEffectPrefab, 
                target.transform.position, 
                Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    private void ShowDamageNumber(Vector3 position, float amount)
    {
        FloatingTextManager.Instance.ShowFloatingText(
            amount.ToString("F0"), 
            position, 
            Color.red
        );
    }
    
    private void ShowHealNumber(Vector3 position, float amount)
    {
        FloatingTextManager.Instance.ShowFloatingText(
            amount.ToString("F0"), 
            position, 
            Color.green
        );
    }
    
    private EffectType DetermineEffectType()
    {
        // Logic để xác định loại hiệu ứng dựa trên skill data
        if (cardData.skill.damage > 0)
            return EffectType.DamageBoost;
        else if (cardData.skill.buffAmount > 0)
            return EffectType.SpeedBoost;
        else
            return EffectType.DefenseBoost;
    }
    
    private void PlaySkillEffects()
    {
        if (cardData.skill.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(cardData.skill.skillEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        if (cardData.skill.skillSound != null)
        {
            AudioSource.PlayClipAtPoint(cardData.skill.skillSound, transform.position);
        }
    }
    
    private void Update()
    {
        if (skillCooldown > 0)
        {
            skillCooldown -= Time.deltaTime;
        }
    }
    
    private System.Collections.IEnumerator HideHitboxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitboxVisual != null)
        {
            hitboxVisual.Hide();
            isShowingHitbox = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        // Debug visualization trong Editor
        if (cardData != null && cardData.skill != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, cardData.skill.targetRadius);
        }
    }
} 
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
    
    private SkillEffectHandler effectHandler;
    private SkillTargeting targeting;
    
    public void Initialize(Card card, bool isPlayer)
    {
        this.cardData = card;
        this.isPlayer = isPlayer;
        skillCooldown = 0;
        
        effectHandler = gameObject.AddComponent<SkillEffectHandler>();
        effectHandler.Initialize(card);
        
        targeting = gameObject.AddComponent<SkillTargeting>();
        targeting.Initialize(card, isPlayer);
        
        InitializeHitbox();
    }
    
    private void InitializeHitbox()
    {
        if (hitboxPrefab != null && hitboxVisual == null)
        {
            GameObject hitboxObj = Instantiate(hitboxPrefab);
            hitboxVisual = hitboxObj.GetComponent<SkillHitboxVisual>();
            hitboxObj.transform.SetParent(null);
            hitboxObj.SetActive(false);
        }
        else if (hitboxVisual == null)
        {
            CreateHitboxFromCode();
        }
    }
    
    private void CreateHitboxFromCode()
    {
        // Tạo hitbox từ code nếu không có prefab
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

    
    public void UseSkill()
    {
        if (cardData.skill == null) return;
        
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
        
        PlaySkillEffects();
    }
    
    private void HandleSingleTargetSkill()
    {
        Unit bestTarget = targeting.FindBestSingleTarget(transform.position);
        if (bestTarget != null)
        {
            Vector3 targetWorldPos = bestTarget.transform.position;
            ShowHitbox(TargetType.SingleTarget, 0.5f, targetWorldPos);
            effectHandler.ApplyEffect(bestTarget);
        }
    }
    
    private void HandleAOESkill()
    {
        Vector2 bestPosition = targeting.FindBestAOEPosition(transform.position);
        ShowHitbox(TargetType.AOE, cardData.skill.targetRadius, bestPosition);
        
        Collider2D[] targets = Physics2D.OverlapCircleAll(bestPosition, cardData.skill.targetRadius);
        foreach (Collider2D collider in targets)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.IsPlayerUnit != isPlayer)
            {
                effectHandler.ApplyEffect(unit);
            }
        }
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
            
            effectHandler.ApplyEffect(bestAlly);
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
                effectHandler.ApplyEffect(unit);
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
    
    private void ShowHitbox(TargetType targetType, float radius, Vector3 position)
    {
        if (hitboxVisual != null)
        {
            hitboxVisual.ShowHitbox(targetType, radius, position);
            isShowingHitbox = true;
            StartCoroutine(HideHitboxAfterDelay(1f));
        }
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
    
    private IEnumerator HideHitboxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitboxVisual != null)
        {
            hitboxVisual.Hide();
            isShowingHitbox = false;
        }
    }
    
    private void OnDestroy()
    {
        if (hitboxVisual != null)
        {
            Destroy(hitboxVisual.gameObject);
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
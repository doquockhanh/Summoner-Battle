using UnityEngine;

public class SkillTargeting : MonoBehaviour
{
    private Card cardData;
    private bool isPlayer;
    
    public void Initialize(Card card, bool isPlayer)
    {
        this.cardData = card;
        this.isPlayer = isPlayer;
    }

    public Unit FindBestSingleTarget(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, cardData.skill.targetRadius);
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
    
    public Vector2 FindBestAOEPosition(Vector3 position)
    {
        Collider2D[] allUnits = Physics2D.OverlapCircleAll(Vector2.zero, 50f);
        Vector2 bestPosition = Vector2.zero;
        int maxTargets = 0;
        float highestThreatLevel = 0f;
        
        foreach (Collider2D collider in allUnits)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit == null || unit.IsPlayerUnit == isPlayer) continue;
            
            Vector2 testPosition = unit.transform.position;
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

    private float EvaluateTarget(Unit unit)
    {
        float score = 0;
        UnitData data = unit.GetUnitData();
        
        score += (unit.GetCurrentHP() / data.hp) * 0.4f;
        score += (data.damage / 100f) * 0.3f;
        
        float distanceToBase = Vector2.Distance(unit.transform.position, GetAlliedBase().transform.position);
        score += (1 - distanceToBase/10f) * 0.3f;
        
        return score;
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
} 
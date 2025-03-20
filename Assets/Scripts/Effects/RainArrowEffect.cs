using UnityEngine;

public class RainArrowEffect : MonoBehaviour
{
    private RainArrowSkill skillData;
    private HexCell targetPosition;
    private System.Action onDamageCallback;

    public void Initialize(RainArrowSkill skill, HexCell position, System.Action damageCallback)
    {
        skillData = skill;
        targetPosition = position;
        onDamageCallback = damageCallback;
        transform.position = position.WorldPosition;
    }

    // Gọi từ Animation Event cho mỗi đợt tên
    public void OnWaveHit()
    {
        if (onDamageCallback != null)
        {
            onDamageCallback.Invoke();
        }
    }

    // Gọi khi animation kết thúc
    public void OnAnimationComplete()
    {
        Destroy(gameObject);
    }
} 
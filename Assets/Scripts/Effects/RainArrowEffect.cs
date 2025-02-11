using UnityEngine;

public class RainArrowEffect : MonoBehaviour
{
    private RainArrowSkill skillData;
    private Vector3 targetPosition;
    private System.Action<Vector3> onDamageCallback;

    public void Initialize(RainArrowSkill skill, Vector3 position, System.Action<Vector3> damageCallback)
    {
        skillData = skill;
        targetPosition = position;
        onDamageCallback = damageCallback;
        transform.position = position;
    }

    // Gọi từ Animation Event cho mỗi đợt tên
    public void OnWaveHit()
    {
        if (onDamageCallback != null)
        {
            onDamageCallback.Invoke(targetPosition);
        }
    }

    // Gọi khi animation kết thúc
    public void OnAnimationComplete()
    {
        Destroy(gameObject);
    }
} 
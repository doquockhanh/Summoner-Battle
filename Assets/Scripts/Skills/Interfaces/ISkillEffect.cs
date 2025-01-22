using UnityEngine;

public interface ISkillEffect
{
    void ApplyEffect(Unit target);
    void ShowVisualEffect(Vector3 position);
} 
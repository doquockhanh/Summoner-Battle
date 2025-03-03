using UnityEngine;

public interface ISkillEffect
{
    void Execute(Vector3 targetPosition);
    void Cleanup();
}
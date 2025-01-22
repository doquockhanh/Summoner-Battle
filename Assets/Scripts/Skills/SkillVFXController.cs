using UnityEngine;
using System.Collections.Generic;

public class SkillVFXController : MonoBehaviour
{
    [System.Serializable]
    public class SkillVFX
    {
        public string skillName;
        public GameObject vfxPrefab;
        public AudioClip soundEffect;
        public float duration = 1f;
    }
    
    [SerializeField] private List<SkillVFX> skillEffects;
    private Dictionary<string, SkillVFX> effectsDict = new Dictionary<string, SkillVFX>();
    
    private void Awake()
    {
        foreach (var effect in skillEffects)
        {
            effectsDict[effect.skillName] = effect;
        }
    }
    
    public void PlaySkillVFX(string skillName, Vector3 position, Vector3 direction)
    {
        if (effectsDict.TryGetValue(skillName, out SkillVFX effect))
        {
            // Tạo hiệu ứng visual
            if (effect.vfxPrefab != null)
            {
                GameObject vfx = Instantiate(effect.vfxPrefab, position, Quaternion.LookRotation(direction));
                Destroy(vfx, effect.duration);
            }
            
            // Phát âm thanh
            if (effect.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(effect.soundEffect, position);
            }
        }
    }
} 
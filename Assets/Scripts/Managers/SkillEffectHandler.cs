using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillEffectHandler : MonoBehaviour
{
    public static SkillEffectHandler Instance { get; private set; }

    [Header("Hiệu ứng")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject chargeEffectPrefab;
    [SerializeField] private GameObject rangeIndicatorPrefab;
    [SerializeField] private GameObject rainArrowEffectPrefab;

    private Dictionary<int, GameObject> activeRangeIndicators = new Dictionary<int, GameObject>();
    private int nextIndicatorId = 0;

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

    public void HandleAssassinateSkill(Unit assassin, Unit target, AssassinateSkill skill)
    {
        if (assassin == null || target == null) return;

        StartCoroutine(AssassinateCoroutine(assassin, target, skill));
    }

    private IEnumerator AssassinateCoroutine(Unit assassin, Unit target, AssassinateSkill skill)
    {
        // Lưu vị trí ban đầu
        Vector3 startPos = assassin.transform.position;
        Vector3 targetPos = target.transform.position;

        // Animation nhảy
        float jumpTime = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < jumpTime)
        {
            float t = elapsedTime / jumpTime;
            // Thêm đường cong cho animation nhảy
            float height = Mathf.Sin(t * Mathf.PI) * 2f;
            assassin.transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * height;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo đến đúng vị trí
        assassin.transform.position = targetPos;

        // Gán target mới cho assassin
        var targeting = assassin.GetComponent<UnitTargeting>();
        if (targeting != null)
        {
            targeting.SetTarget(target);
        }
    }


    public int ShowRangeIndicator(HexCell cell, float radius, Color? color = null, float? duration = 0f)
    {
        Vector2 pos2 = cell.WorldPosition;
        GameObject indicator = Instantiate(rangeIndicatorPrefab, pos2, Quaternion.identity);
        SkillRangeIndicator rangeIndicator = indicator.GetComponent<SkillRangeIndicator>();

        if (rangeIndicator != null)
        {
            rangeIndicator.SetRadius(radius);
            rangeIndicator.SetColor(color ?? Color.red);
        }

        int indicatorId = nextIndicatorId++;
        activeRangeIndicators[indicatorId] = indicator;

        if (duration > 0)
        {
            StartCoroutine(HideRangeIndicator(indicatorId, duration.Value));
        }
        return indicatorId;
    }

    public GameObject CreateRangeIndicator(HexCell cell, float radius, Color? color = null)
    {
        Vector2 pos2 = cell.WorldPosition;
        GameObject indicator = Instantiate(rangeIndicatorPrefab, pos2, Quaternion.identity);
        SkillRangeIndicator rangeIndicator = indicator.GetComponent<SkillRangeIndicator>();

        if (rangeIndicator != null)
        {
            rangeIndicator.SetRadius(radius);
            rangeIndicator.SetColor(color ?? Color.red);
        }

        return indicator;
    }

    public IEnumerator HideRangeIndicator(int indicatorId, float duration)
    {
        yield return new WaitForSeconds(duration);
        HideRangeIndicator(indicatorId);
    }

    public void HideRangeIndicator(int indicatorId)
    {
        if (activeRangeIndicators.TryGetValue(indicatorId, out GameObject indicator))
        {
            Destroy(indicator);
            activeRangeIndicators.Remove(indicatorId);
        }
    }

    private void HideAllRangeIndicators()
    {
        foreach (var indicator in activeRangeIndicators.Values)
        {
            Destroy(indicator);
        }
        activeRangeIndicators.Clear();
    }
}
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

    public int ShowRangeIndicator(HexCell cell, float radius, Color? color = null, float? duration = 5f)
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

    public GameObject CreateRangeIndicator(HexCell cell, float radius, Color? color = null, float duration = 10f)
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
            StartCoroutine(HideRangeIndicator(indicatorId, duration));
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
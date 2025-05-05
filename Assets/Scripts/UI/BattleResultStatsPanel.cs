using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleResultStatsPanel : MonoBehaviour
{
    [SerializeField] private GameObject cardStatItemPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        Hide();
    }

    public void Show(List<BattleStatsManager.CardBattleStats> statsList)
    {
        gameObject.SetActive(true);
        // Xóa các item cũ
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }
        // Tạo item mới cho mỗi card
        foreach (var cardStats in statsList)
        {
            GameObject item = Instantiate(cardStatItemPrefab, contentRoot);
            var view = item.GetComponent<CardStatItemView>();
            if (view != null)
            {
                view.Set(cardStats);
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

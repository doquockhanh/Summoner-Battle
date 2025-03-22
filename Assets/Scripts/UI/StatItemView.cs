using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatItemView : MonoBehaviour
{
    [SerializeField] private Image statIcon;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI statValueText;
    
    public void Setup(string statName, string value, Sprite icon = null)
    {
        statNameText.text = statName;
        statValueText.text = value;
        
        if (icon != null)
        {
            statIcon.sprite = icon;
            statIcon.gameObject.SetActive(true);
        }
        else
        {
            statIcon.gameObject.SetActive(false);
        }
    }
} 
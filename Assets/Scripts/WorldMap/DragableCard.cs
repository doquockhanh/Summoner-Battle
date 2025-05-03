using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas canvas;
    public Image cardImage;

    private GameObject draggingCard;
    private RectTransform draggingRect;
    private CanvasGroup canvasGroup;
    [HideInInspector] public string id; 

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        // Tạo clone
        draggingCard = Instantiate(gameObject, canvas.transform);
        draggingRect = draggingCard.GetComponent<RectTransform>();
        canvasGroup = draggingCard.GetComponent<CanvasGroup>();

        // Copy sprite
        var cloneImg = draggingCard.GetComponent<DraggableCard>().cardImage;
        cloneImg.sprite = cardImage.sprite;

        // Vô hiệu tương tác clone
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingRect != null)
        {
            draggingRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingCard != null)
        {
            Destroy(draggingCard);
        }
    }

    public Sprite GetSprite()
    {
        return cardImage.sprite;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public Image slotImage;
    public int slotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropedGO = eventData.pointerDrag;
        var dragged = dropedGO?.GetComponent<DraggableCard>();
        if (dragged == null) return;
        slotImage.sprite = dragged.GetSprite();
        slotImage.color = Color.white;

        DraggableCard draggableCard = dropedGO.GetComponent<DraggableCard>();
        ResourcePoint resourcePoint = GetComponentInParent<ResourcePoint>();

        Debug.Log(draggableCard);
        Debug.Log(resourcePoint);
        if (draggableCard == null || resourcePoint == null) return;

        Debug.Log(3);
        resourcePoint.SetCardInSlot(slotIndex, draggableCard.id);
    }
}

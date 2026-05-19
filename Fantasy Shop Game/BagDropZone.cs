using UnityEngine;
using UnityEngine.EventSystems;

public class BagDropZone : MonoBehaviour, IDropHandler
{
    [Header("Bag Info")]
    [SerializeField] private string bagId = "Bag 1";

    public string BagId => bagId;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        DraggableItem item = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (item != null)
        {
            item.PlaceInBag(this);
        }
    }
}
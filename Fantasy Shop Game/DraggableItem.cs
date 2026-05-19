using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Sizes")]
    [SerializeField] private Vector2 counterSize = new Vector2(130f, 170f);
    [SerializeField] private Vector2 bagSize = new Vector2(120f, 70f);

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;
    private ItemCard itemCard;

    private Transform startParent;
    private Vector2 startPosition;
    private Vector2 startSize;
    private bool startWasInBag;
    private string startBagId;

    private bool droppedInValidZone;
    private bool canDragThisTime;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        itemCard = GetComponent<ItemCard>();
        canvas = GetComponentInParent<Canvas>();

        SetCounterSize();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemCard != null && itemCard.IsRejected)
        {
            canDragThisTime = false;
            return;
        }

        canDragThisTime = true;

        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
        startSize = rectTransform.sizeDelta;
        startWasInBag = itemCard != null && itemCard.IsInBag;
        startBagId = itemCard != null ? itemCard.CurrentBagId : "";

        droppedInValidZone = false;

        if (itemCard != null)
        {
            itemCard.SetInBag(false);
        }

        layoutElement.ignoreLayout = true;

        canvasGroup.alpha = 0.85f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDragThisTime)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDragThisTime)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!droppedInValidZone)
        {
            transform.SetParent(startParent, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = startSize;
            rectTransform.anchoredPosition = startPosition;

            layoutElement.ignoreLayout = !startWasInBag;

            if (itemCard != null)
            {
                itemCard.SetInBag(startWasInBag, startBagId);
            }
        }

        canDragThisTime = false;
    }

    public void PlaceInBag(BagDropZone bagDropZone)
    {
        if (itemCard != null && itemCard.IsRejected)
        {
            return;
        }

        droppedInValidZone = true;

        transform.SetParent(bagDropZone.transform, false);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        SetBagSize();

        if (itemCard != null)
        {
            itemCard.SetInBag(true, bagDropZone.BagId);
        }
    }

    private void SetCounterSize()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = counterSize;

        layoutElement.preferredWidth = counterSize.x;
        layoutElement.preferredHeight = counterSize.y;
        layoutElement.ignoreLayout = true;
    }

    private void SetBagSize()
    {
        rectTransform.sizeDelta = bagSize;

        layoutElement.preferredWidth = bagSize.x;
        layoutElement.preferredHeight = bagSize.y;
        layoutElement.ignoreLayout = false;
    }
}
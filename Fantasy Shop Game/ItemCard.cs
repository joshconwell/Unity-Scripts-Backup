using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    [SerializeField] private ItemData itemData;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI iconText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Counter View")]
    [SerializeField] private float counterNameFontSize = 16f;
    [SerializeField] private float counterIconFontSize = 22f;
    [SerializeField] private float counterPriceFontSize = 18f;

    [Header("Bag View")]
    [SerializeField] private bool hideIconInBag = true;
    [SerializeField] private float bagNameFontSizeMax = 12f;
    [SerializeField] private float bagNameFontSizeMin = 8f;
    [SerializeField] private float bagPriceFontSize = 13f;

    [Header("Selection Border")]
    [SerializeField] private Color selectedBorderColor = Color.yellow;
    [SerializeField] private float borderThickness = 5f;

    private CheckoutGameManager gameManager;
    private Image cardImage;
    private GameObject selectionBorder;
    private GameObject rejectedOverlay;

    private bool hasBeenScanned;
    private bool isInBag;
    private bool isRejected;
    private string currentBagId = "";

    public string ItemId => itemData != null ? itemData.itemId : "";
    public string ItemName => itemData != null ? itemData.itemName : "Missing Item Data";
    public int BaseValue => itemData != null ? itemData.baseValue : 0;
    public bool HasBeenScanned => hasBeenScanned;
    public bool IsInBag => isInBag;
    public bool IsRejected => isRejected;
    public string CurrentBagId => currentBagId;
    public bool MustBeBaggedAlone => itemData != null && itemData.mustBeBaggedAlone;
    public bool MustBeRejected => itemData != null && itemData.mustBeRejected;

    private void Awake()
    {
        gameManager = FindObjectOfType<CheckoutGameManager>();
        cardImage = GetComponent<Image>();

        FindTextReferencesAutomatically();

        CreateSelectionBorder();
        CreateRejectedOverlay();

        SetSelected(false);
        SetRejectedVisual(false);
        ApplyDataToVisuals();
    }

    public void SetData(ItemData newItemData)
    {
        itemData = newItemData;

        hasBeenScanned = false;
        isInBag = false;
        isRejected = false;
        currentBagId = "";

        SetSelected(false);
        SetRejectedVisual(false);
        ApplyDataToVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager != null)
        {
            gameManager.SelectItem(this);
        }
    }

    public void Scan()
    {
        hasBeenScanned = true;
        RefreshDisplayedTexts();
    }

    public string GetScanText()
    {
        if (itemData == null)
        {
            return "Missing item data.";
        }

        return itemData.itemName + " — " + itemData.scanDescription;
    }

    public void RejectItem()
    {
        isRejected = true;
        isInBag = false;
        currentBagId = "";

        SetSelected(false);
        SetRejectedVisual(true);
        RefreshDisplayedTexts();
    }

    public void SetInBag(bool value, string bagId = "")
    {
        if (isRejected)
        {
            isInBag = false;
            currentBagId = "";
            RefreshDisplayedTexts();
            return;
        }

        isInBag = value;
        currentBagId = value ? bagId : "";
        RefreshDisplayedTexts();
    }

    public bool ExplodesWith(ItemCard otherItem)
    {
        if (itemData == null || otherItem == null || itemData.explodesWithItemIdsInSameBag == null)
        {
            return false;
        }

        foreach (string badItemId in itemData.explodesWithItemIdsInSameBag)
        {
            if (otherItem.ItemId == badItemId)
            {
                return true;
            }
        }

        return false;
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(selected);
        }
    }

    private void ApplyDataToVisuals()
    {
        if (itemData == null)
        {
            if (cardImage != null)
            {
                cardImage.color = Color.white;
            }

            if (itemNameText != null)
            {
                itemNameText.text = "Missing Data";
            }

            if (iconText != null)
            {
                iconText.text = "?";
                iconText.gameObject.SetActive(true);
            }

            if (priceText != null)
            {
                priceText.text = "???";
            }

            return;
        }

        if (cardImage != null)
        {
            cardImage.color = itemData.cardColor;
        }

        RefreshDisplayedTexts();
    }

    private void RefreshDisplayedTexts()
    {
        if (itemData == null)
        {
            return;
        }

        if (isInBag)
        {
            ApplyBagView();
        }
        else
        {
            ApplyCounterView();
        }

        RefreshPriceText();
    }

    private void ApplyCounterView()
    {
        if (itemNameText != null)
        {
            itemNameText.enableAutoSizing = false;
            itemNameText.fontSize = counterNameFontSize;
            itemNameText.text = itemData.itemName.Replace(" ", "\n");
        }

        if (iconText != null)
        {
            iconText.gameObject.SetActive(true);
            iconText.enableAutoSizing = false;
            iconText.fontSize = counterIconFontSize;
            iconText.text = itemData.iconText;
        }

        if (priceText != null)
        {
            priceText.enableAutoSizing = false;
            priceText.fontSize = counterPriceFontSize;
        }
    }

    private void ApplyBagView()
    {
        if (itemNameText != null)
        {
            itemNameText.enableAutoSizing = true;
            itemNameText.fontSizeMin = bagNameFontSizeMin;
            itemNameText.fontSizeMax = bagNameFontSizeMax;
            itemNameText.text = itemData.itemName;
        }

        if (iconText != null)
        {
            iconText.text = itemData.iconText;
            iconText.gameObject.SetActive(!hideIconInBag);
        }

        if (priceText != null)
        {
            priceText.enableAutoSizing = false;
            priceText.fontSize = bagPriceFontSize;
        }
    }

    private void RefreshPriceText()
    {
        if (priceText == null || itemData == null)
        {
            return;
        }

        if (!hasBeenScanned)
        {
            priceText.text = "???";
            return;
        }

        if (itemData.mustBeRejected)
        {
            priceText.text = "REJECT";
        }
        else
        {
            priceText.text = itemData.baseValue + "g";
        }
    }

    private void SetRejectedVisual(bool rejected)
    {
        if (rejectedOverlay != null)
        {
            rejectedOverlay.SetActive(rejected);
        }
    }

    private void FindTextReferencesAutomatically()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "ItemNameText")
            {
                itemNameText = text;
            }
            else if (text.gameObject.name == "IconText")
            {
                iconText = text;
            }
            else if (text.gameObject.name == "PriceText")
            {
                priceText = text;
            }
        }
    }

    private void CreateRejectedOverlay()
    {
        rejectedOverlay = new GameObject("RejectedOverlay");
        rejectedOverlay.transform.SetParent(transform, false);

        RectTransform overlayRect = rejectedOverlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = rejectedOverlay.AddComponent<Image>();
        overlayImage.color = new Color(0.7f, 0f, 0f, 0.65f);
        overlayImage.raycastTarget = false;

        GameObject textObject = new GameObject("RejectedText");
        textObject.transform.SetParent(rejectedOverlay.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI rejectedText = textObject.AddComponent<TextMeshProUGUI>();
        rejectedText.text = "REJECTED";
        rejectedText.fontSize = 18;
        rejectedText.alignment = TextAlignmentOptions.Center;
        rejectedText.color = Color.white;
        rejectedText.raycastTarget = false;

        rejectedOverlay.transform.SetAsLastSibling();
    }

    private void CreateSelectionBorder()
    {
        selectionBorder = new GameObject("SelectionBorder");
        selectionBorder.transform.SetParent(transform, false);

        RectTransform borderRoot = selectionBorder.AddComponent<RectTransform>();
        borderRoot.anchorMin = Vector2.zero;
        borderRoot.anchorMax = Vector2.one;
        borderRoot.offsetMin = Vector2.zero;
        borderRoot.offsetMax = Vector2.zero;

        CreateBorderEdge("TopBorder", borderRoot, BorderEdge.Top);
        CreateBorderEdge("BottomBorder", borderRoot, BorderEdge.Bottom);
        CreateBorderEdge("LeftBorder", borderRoot, BorderEdge.Left);
        CreateBorderEdge("RightBorder", borderRoot, BorderEdge.Right);

        selectionBorder.transform.SetAsLastSibling();
    }

    private void CreateBorderEdge(string edgeName, RectTransform parent, BorderEdge edge)
    {
        GameObject edgeObject = new GameObject(edgeName);
        edgeObject.transform.SetParent(parent, false);

        Image image = edgeObject.AddComponent<Image>();
        image.color = selectedBorderColor;
        image.raycastTarget = false;

        RectTransform rect = edgeObject.GetComponent<RectTransform>();

        switch (edge)
        {
            case BorderEdge.Top:
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0f, -borderThickness);
                rect.offsetMax = new Vector2(0f, 0f);
                break;

            case BorderEdge.Bottom:
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.offsetMin = new Vector2(0f, 0f);
                rect.offsetMax = new Vector2(0f, borderThickness);
                break;

            case BorderEdge.Left:
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.offsetMin = new Vector2(0f, 0f);
                rect.offsetMax = new Vector2(borderThickness, 0f);
                break;

            case BorderEdge.Right:
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.offsetMin = new Vector2(-borderThickness, 0f);
                rect.offsetMax = new Vector2(0f, 0f);
                break;
        }
    }

    private enum BorderEdge
    {
        Top,
        Bottom,
        Left,
        Right
    }
}
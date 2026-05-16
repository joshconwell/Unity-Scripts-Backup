using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PressureEventToastUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string defaultTitleText = "THREAT EVENT";
    [SerializeField] private string defaultSubtitleText = "ENEMY PRESSURE INCREASING";

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(560f, 110f);
    [SerializeField] private Vector2 panelAnchoredPosition = new Vector2(0f, -245f);

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.035f, 0.025f, 0.045f, 0.9f);
    [SerializeField] private Color sideBarColor = new Color(1f, 0.45f, 0.12f, 1f);
    [SerializeField] private Color titleColor = new Color(1f, 0.62f, 0.2f, 1f);
    [SerializeField] private Color subtitleColor = Color.white;

    [Header("Text Sizes")]
    [SerializeField] private int titleFontSize = 30;
    [SerializeField] private int subtitleFontSize = 17;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float fadeOutDuration = 0.22f;
    [SerializeField] private float slideDistance = 48f;
    [SerializeField] private float pulseAmount = 0.025f;
    [SerializeField] private float pulseSpeed = 8f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rootRectTransform;
    private RectTransform panelRectTransform;

    private Image sideBarImage;
    private Text titleTextComponent;
    private Text subtitleTextComponent;

    private Font runtimeFont;
    private Coroutine activeRoutine;

    private Vector2 hiddenPosition;
    private Vector2 shownPosition;

    private void Awake()
    {
        runtimeFont = GetRuntimeFont();
        BuildUI();
        HideInstant();
    }

    public Coroutine ShowEvent(string title, string subtitle, Color accentColor, float duration)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(PlayEventRoutine(title, subtitle, accentColor, duration));
        return activeRoutine;
    }

    private IEnumerator PlayEventRoutine(string title, string subtitle, Color accentColor, float duration)
    {
        BuildUI();

        if (string.IsNullOrWhiteSpace(title))
        {
            title = defaultTitleText;
        }

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            subtitle = defaultSubtitleText;
        }

        titleTextComponent.text = title.ToUpper();
        subtitleTextComponent.text = subtitle;

        titleTextComponent.color = accentColor;
        sideBarImage.color = accentColor;

        shownPosition = panelAnchoredPosition;
        hiddenPosition = panelAnchoredPosition + new Vector2(0f, slideDistance);

        rootRectTransform.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        panelRectTransform.anchoredPosition = hiddenPosition;
        panelRectTransform.localScale = Vector3.one;

        float safeFadeInDuration = Mathf.Max(0.01f, fadeInDuration);
        float safeFadeOutDuration = Mathf.Max(0.01f, fadeOutDuration);
        float safeDuration = Mathf.Max(duration, safeFadeInDuration + safeFadeOutDuration + 0.05f);
        float holdDuration = safeDuration - safeFadeInDuration - safeFadeOutDuration;

        float timer = 0f;

        while (timer < safeFadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / safeFadeInDuration);
            float easedT = EaseOutBack(t);

            canvasGroup.alpha = t;
            panelRectTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, shownPosition, easedT);
            UpdatePulse();

            yield return null;
        }

        canvasGroup.alpha = 1f;
        panelRectTransform.anchoredPosition = shownPosition;

        timer = 0f;

        while (timer < holdDuration)
        {
            timer += Time.unscaledDeltaTime;
            UpdatePulse();

            yield return null;
        }

        timer = 0f;

        while (timer < safeFadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / safeFadeOutDuration);

            canvasGroup.alpha = 1f - t;
            panelRectTransform.anchoredPosition = Vector2.Lerp(shownPosition, hiddenPosition, t);
            UpdatePulse();

            yield return null;
        }

        HideInstant();
        activeRoutine = null;
    }

    public void HideInstant()
    {
        if (rootRectTransform != null)
        {
            rootRectTransform.gameObject.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (panelRectTransform != null)
        {
            panelRectTransform.localScale = Vector3.one;
            panelRectTransform.anchoredPosition = panelAnchoredPosition;
        }
    }

    private void UpdatePulse()
    {
        if (panelRectTransform == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        panelRectTransform.localScale = Vector3.one * pulse;
    }

    private void BuildUI()
    {
        if (rootRectTransform != null)
        {
            return;
        }

        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Pressure Event Toast Canvas");
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 430;

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject rootObject = new GameObject("Pressure Event Toast Root");
        rootObject.transform.SetParent(canvas.transform, false);

        rootRectTransform = rootObject.AddComponent<RectTransform>();
        rootRectTransform.anchorMin = Vector2.zero;
        rootRectTransform.anchorMax = Vector2.one;
        rootRectTransform.offsetMin = Vector2.zero;
        rootRectTransform.offsetMax = Vector2.zero;

        canvasGroup = rootObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject panelObject = new GameObject("Pressure Event Panel");
        panelObject.transform.SetParent(rootObject.transform, false);

        panelRectTransform = panelObject.AddComponent<RectTransform>();
        panelRectTransform.anchorMin = new Vector2(0.5f, 1f);
        panelRectTransform.anchorMax = new Vector2(0.5f, 1f);
        panelRectTransform.pivot = new Vector2(0.5f, 1f);
        panelRectTransform.anchoredPosition = panelAnchoredPosition;
        panelRectTransform.sizeDelta = panelSize;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        HorizontalLayoutGroup horizontalLayoutGroup = panelObject.AddComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayoutGroup.spacing = 12f;
        horizontalLayoutGroup.padding = new RectOffset(0, 18, 10, 10);
        horizontalLayoutGroup.childControlWidth = false;
        horizontalLayoutGroup.childControlHeight = true;
        horizontalLayoutGroup.childForceExpandWidth = false;
        horizontalLayoutGroup.childForceExpandHeight = true;

        GameObject sideBarObject = new GameObject("Threat Side Bar");
        sideBarObject.transform.SetParent(panelObject.transform, false);

        RectTransform sideBarRect = sideBarObject.AddComponent<RectTransform>();
        sideBarRect.sizeDelta = new Vector2(10f, panelSize.y);

        LayoutElement sideBarLayout = sideBarObject.AddComponent<LayoutElement>();
        sideBarLayout.preferredWidth = 10f;
        sideBarLayout.minWidth = 10f;
        sideBarLayout.preferredHeight = panelSize.y;

        sideBarImage = sideBarObject.AddComponent<Image>();
        sideBarImage.color = sideBarColor;
        sideBarImage.raycastTarget = false;

        GameObject textColumnObject = new GameObject("Text Column");
        textColumnObject.transform.SetParent(panelObject.transform, false);

        RectTransform textColumnRect = textColumnObject.AddComponent<RectTransform>();
        textColumnRect.sizeDelta = new Vector2(panelSize.x - 40f, panelSize.y);

        LayoutElement textColumnLayout = textColumnObject.AddComponent<LayoutElement>();
        textColumnLayout.preferredWidth = panelSize.x - 40f;
        textColumnLayout.preferredHeight = panelSize.y;

        VerticalLayoutGroup verticalLayoutGroup = textColumnObject.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        verticalLayoutGroup.spacing = 4f;
        verticalLayoutGroup.padding = new RectOffset(0, 0, 10, 8);
        verticalLayoutGroup.childControlWidth = true;
        verticalLayoutGroup.childControlHeight = false;
        verticalLayoutGroup.childForceExpandWidth = true;
        verticalLayoutGroup.childForceExpandHeight = false;

        titleTextComponent = CreateText(
            textColumnObject.transform,
            "Title Text",
            defaultTitleText,
            titleFontSize,
            FontStyle.Bold,
            titleColor,
            42f,
            TextAnchor.MiddleLeft
        );

        subtitleTextComponent = CreateText(
            textColumnObject.transform,
            "Subtitle Text",
            defaultSubtitleText,
            subtitleFontSize,
            FontStyle.Bold,
            subtitleColor,
            28f,
            TextAnchor.MiddleLeft
        );
    }

    private Text CreateText(
        Transform parent,
        string objectName,
        string startingText,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        float preferredHeight,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(panelSize.x, preferredHeight);

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.minHeight = preferredHeight;

        Text text = textObject.AddComponent<Text>();
        text.text = startingText;
        text.font = runtimeFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private Font GetRuntimeFont()
    {
        Font font = null;

        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            font = null;
        }

        if (font != null)
        {
            return font;
        }

        try
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        catch
        {
            font = null;
        }

        if (font != null)
        {
            return font;
        }

        return Font.CreateDynamicFontFromOSFont(
            new string[] { "Arial", "Liberation Sans", "Verdana" },
            14
        );
    }
}
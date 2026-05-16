using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MiniBossIntroBannerUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string titleText = "MINI-BOSS INCOMING";
    [SerializeField] private string subtitleText = "ARENA LOCKDOWN INITIATED";

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(760f, 220f);
    [SerializeField] private Vector2 panelAnchoredPosition = new Vector2(0f, 70f);

    [Header("Colors")]
    [SerializeField] private Color screenDimColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color panelColor = new Color(0.08f, 0.02f, 0.02f, 0.92f);
    [SerializeField] private Color titleColor = new Color(1f, 0.2f, 0.12f, 1f);
    [SerializeField] private Color bossNameColor = Color.white;
    [SerializeField] private Color subtitleColor = new Color(1f, 0.78f, 0.32f, 1f);

    [Header("Text Sizes")]
    [SerializeField] private int titleFontSize = 42;
    [SerializeField] private int bossNameFontSize = 54;
    [SerializeField] private int subtitleFontSize = 24;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.18f;
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float pulseAmount = 0.045f;
    [SerializeField] private float pulseSpeed = 8f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rootRectTransform;
    private RectTransform panelRectTransform;

    private Image dimImage;
    private Image panelImage;

    private Text titleTextComponent;
    private Text bossNameTextComponent;
    private Text subtitleTextComponent;

    private Font runtimeFont;
    private Coroutine activeRoutine;

    private void Awake()
    {
        runtimeFont = GetRuntimeFont();
        BuildUI();
        HideInstant();
    }

    public Coroutine ShowIntro(string bossName, float duration)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(PlayIntroRoutine(bossName, duration));
        return activeRoutine;
    }

    public IEnumerator PlayIntroRoutine(string bossName, float duration)
    {
        BuildUI();

        if (string.IsNullOrEmpty(bossName))
        {
            bossName = "UNKNOWN THREAT";
        }

        titleTextComponent.text = titleText;
        bossNameTextComponent.text = bossName;
        subtitleTextComponent.text = subtitleText;

        rootRectTransform.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        float safeFadeInDuration = Mathf.Max(0.01f, fadeInDuration);
        float safeFadeOutDuration = Mathf.Max(0.01f, fadeOutDuration);
        float safeDuration = Mathf.Max(duration, safeFadeInDuration + safeFadeOutDuration + 0.05f);

        float holdDuration = safeDuration - safeFadeInDuration - safeFadeOutDuration;

        float timer = 0f;

        while (timer < safeFadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeFadeInDuration);

            canvasGroup.alpha = t;
            UpdatePulse();

            yield return null;
        }

        canvasGroup.alpha = 1f;

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
            GameObject canvasObject = new GameObject("Mini Boss Intro Canvas");
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject rootObject = new GameObject("Mini Boss Intro Banner Root");
        rootObject.transform.SetParent(canvas.transform, false);

        rootRectTransform = rootObject.AddComponent<RectTransform>();
        rootRectTransform.anchorMin = Vector2.zero;
        rootRectTransform.anchorMax = Vector2.one;
        rootRectTransform.offsetMin = Vector2.zero;
        rootRectTransform.offsetMax = Vector2.zero;

        canvasGroup = rootObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject dimObject = new GameObject("Screen Dim");
        dimObject.transform.SetParent(rootObject.transform, false);

        RectTransform dimRectTransform = dimObject.AddComponent<RectTransform>();
        dimRectTransform.anchorMin = Vector2.zero;
        dimRectTransform.anchorMax = Vector2.one;
        dimRectTransform.offsetMin = Vector2.zero;
        dimRectTransform.offsetMax = Vector2.zero;

        dimImage = dimObject.AddComponent<Image>();
        dimImage.color = screenDimColor;

        GameObject panelObject = new GameObject("Intro Panel");
        panelObject.transform.SetParent(rootObject.transform, false);

        panelRectTransform = panelObject.AddComponent<RectTransform>();
        panelRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
        panelRectTransform.anchoredPosition = panelAnchoredPosition;
        panelRectTransform.sizeDelta = panelSize;

        panelImage = panelObject.AddComponent<Image>();
        panelImage.color = panelColor;

        VerticalLayoutGroup verticalLayoutGroup = panelObject.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        verticalLayoutGroup.spacing = 4f;
        verticalLayoutGroup.padding = new RectOffset(20, 20, 18, 18);
        verticalLayoutGroup.childControlWidth = true;
        verticalLayoutGroup.childControlHeight = false;
        verticalLayoutGroup.childForceExpandWidth = true;
        verticalLayoutGroup.childForceExpandHeight = false;

        titleTextComponent = CreateText(
            panelObject.transform,
            "Title Text",
            titleText,
            titleFontSize,
            FontStyle.Bold,
            titleColor,
            52f
        );

        bossNameTextComponent = CreateText(
            panelObject.transform,
            "Boss Name Text",
            "THE WARDEN",
            bossNameFontSize,
            FontStyle.Bold,
            bossNameColor,
            72f
        );

        subtitleTextComponent = CreateText(
            panelObject.transform,
            "Subtitle Text",
            subtitleText,
            subtitleFontSize,
            FontStyle.Bold,
            subtitleColor,
            40f
        );
    }

    private Text CreateText(
        Transform parent,
        string objectName,
        string startingText,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        float preferredHeight)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(panelSize.x, preferredHeight);

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.minHeight = preferredHeight;

        Text text = textObject.AddComponent<Text>();
        text.text = startingText;
        text.font = runtimeFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
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
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EliteHealthBarUI : MonoBehaviour
{
    private struct TargetBarTheme
    {
        public string TargetTypeLabel;
        public Color FillColor;
        public Color DamageLagColor;
        public Color BackgroundColor;
        public Color FrameColor;
        public Color GlowColor;
        public Color AccentColor;
        public Color NameColor;
        public Color HealthTextColor;
        public Color TypeTextColor;
        public Color CriticalPulseColor;
    }

    public static EliteHealthBarUI Instance { get; private set; }

    public static bool HasInstance
    {
        get { return Instance != null; }
    }

    [Header("UI Root")]
    [SerializeField] private GameObject barRoot;
    [SerializeField] private RectTransform panelRectTransform;

    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [Tooltip("Optional. If left empty, the script will create a damage lag bar automatically.")]
    [SerializeField] private Image damageLagFillImage;

    [SerializeField] private RectTransform damageLagFillContainer;

    [Header("Decoration - Optional")]
    [SerializeField] private Image panelBackgroundImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image glowImage;
    [SerializeField] private Image leftAccentImage;
    [SerializeField] private Image rightAccentImage;

    [Header("Legacy Text Optional")]
    [SerializeField] private Text targetTypeText;
    [SerializeField] private Text eliteNameText;
    [SerializeField] private Text healthValueText;

    [Header("TextMeshPro Text Optional")]
    [SerializeField] private TMP_Text targetTypeTMPText;
    [SerializeField] private TMP_Text eliteNameTMPText;
    [SerializeField] private TMP_Text healthValueTMPText;

    [Header("Display")]
    [SerializeField] private string defaultTargetName = "TARGET";
    [SerializeField] private bool hideWhenNoTarget = true;
    [SerializeField] private float hideAfterDeathDelay = 0.5f;

    [Header("Animation")]
    [SerializeField] private float frontBarLerpSpeed = 4.5f;
    [SerializeField] private float lagBarDropSpeed = 1.1f;
    [SerializeField] private float lagBarCatchUpSpeed = 8f;
    [SerializeField] private float introPopDuration = 0.18f;
    [SerializeField] private float introPopScale = 1.06f;

    [Header("Low Health Pulse")]
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float lowHealthPulseSpeed = 8f;
    [SerializeField] private float lowHealthPulseStrength = 0.28f;

    private Health trackedHealth;
    private GameObject trackedTargetObject;
    private EnemyTargetBarStyle trackedStyle = EnemyTargetBarStyle.Elite;

    private Coroutine hideCoroutine;
    private Coroutine introPopCoroutine;

    private float targetHealthPercent = 1f;
    private float displayedHealthPercent = 1f;
    private float displayedLagPercent = 1f;

    private Vector3 basePanelScale = Vector3.one;
    private Font runtimeFont;
    private TargetBarTheme currentTheme;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ResolveReferences();
        runtimeFont = GetRuntimeFont();
        AutoBuildOptionalPolish();

        if (panelRectTransform != null)
        {
            basePanelScale = panelRectTransform.localScale;
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            healthSlider.interactable = false;
        }

        HideBarInstant();
    }

    private void Update()
    {
        UpdateAnimatedBar();
        UpdateLowHealthPulse();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTrackedHealth();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void TrackTarget(
        GameObject targetObject,
        Health targetHealth,
        string targetName,
        EnemyTargetBarStyle targetStyle)
    {
        if (targetObject == null || targetHealth == null)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        ResolveReferences();
        AutoBuildOptionalPolish();

        UnsubscribeFromTrackedHealth();

        trackedTargetObject = targetObject;
        trackedHealth = targetHealth;
        trackedStyle = targetStyle;

        trackedHealth.OnHealthChanged += HandleHealthChanged;
        trackedHealth.OnDied += HandleTargetDied;

        currentTheme = GetThemeForStyle(trackedStyle);
        ApplyTheme(currentTheme);

        SetTargetName(string.IsNullOrWhiteSpace(targetName) ? defaultTargetName : targetName);
        SetTargetTypeLabel(currentTheme.TargetTypeLabel);

        ShowBar();

        targetHealthPercent = Mathf.Clamp01(trackedHealth.CurrentHealth / Mathf.Max(1f, trackedHealth.MaxHealth));
        displayedHealthPercent = targetHealthPercent;
        displayedLagPercent = targetHealthPercent;

        UpdateHealthText(trackedHealth.CurrentHealth, trackedHealth.MaxHealth);
        ApplyImmediateBarVisual();

        if (introPopCoroutine != null)
        {
            StopCoroutine(introPopCoroutine);
        }

        introPopCoroutine = StartCoroutine(IntroPopRoutine());
    }

    public void TrackElite(GameObject eliteObject, Health eliteHealth, string eliteName)
    {
        TrackTarget(eliteObject, eliteHealth, eliteName, EnemyTargetBarStyle.Elite);
    }

    public void StopTrackingTarget(GameObject targetObject)
    {
        if (trackedTargetObject != targetObject)
        {
            return;
        }

        UnsubscribeFromTrackedHealth();

        trackedTargetObject = null;
        trackedHealth = null;

        if (hideWhenNoTarget)
        {
            HideBarInstant();
        }
    }

    public void StopTrackingElite(GameObject eliteObject)
    {
        StopTrackingTarget(eliteObject);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        targetHealthPercent = Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth));
        UpdateHealthText(currentHealth, maxHealth);
    }

    private void HandleTargetDied()
    {
        float maxHealth = trackedHealth != null ? trackedHealth.MaxHealth : 1f;

        targetHealthPercent = 0f;
        UpdateHealthText(0f, maxHealth);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(hideAfterDeathDelay);

        UnsubscribeFromTrackedHealth();
        trackedTargetObject = null;
        trackedHealth = null;

        HideBarInstant();

        hideCoroutine = null;
    }

    private IEnumerator IntroPopRoutine()
    {
        if (panelRectTransform == null)
        {
            yield break;
        }

        float timer = 0f;
        float safeDuration = Mathf.Max(0.01f, introPopDuration);

        while (timer < safeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            float easedT = EaseOutBack(t);

            float scale = Mathf.Lerp(introPopScale, 1f, easedT);
            panelRectTransform.localScale = basePanelScale * scale;

            yield return null;
        }

        panelRectTransform.localScale = basePanelScale;
        introPopCoroutine = null;
    }

    private void UpdateAnimatedBar()
    {
        if (trackedHealth == null && !barRoot.activeSelf)
        {
            return;
        }

        displayedHealthPercent = Mathf.MoveTowards(
            displayedHealthPercent,
            targetHealthPercent,
            frontBarLerpSpeed * Time.unscaledDeltaTime
        );

        if (displayedLagPercent < displayedHealthPercent)
        {
            displayedLagPercent = Mathf.MoveTowards(
                displayedLagPercent,
                displayedHealthPercent,
                lagBarCatchUpSpeed * Time.unscaledDeltaTime
            );
        }
        else
        {
            displayedLagPercent = Mathf.MoveTowards(
                displayedLagPercent,
                displayedHealthPercent,
                lagBarDropSpeed * Time.unscaledDeltaTime
            );
        }

        if (healthSlider != null)
        {
            healthSlider.value = displayedHealthPercent;
        }

        UpdateDamageLagVisual();
    }

    private void UpdateDamageLagVisual()
    {
        if (damageLagFillImage == null || damageLagFillContainer == null)
        {
            return;
        }

        float containerWidth = damageLagFillContainer.rect.width;

        if (containerWidth <= 0f)
        {
            return;
        }

        RectTransform lagRect = damageLagFillImage.rectTransform;

        if (lagRect == null)
        {
            return;
        }

        lagRect.anchorMin = new Vector2(0f, 0f);
        lagRect.anchorMax = new Vector2(0f, 1f);
        lagRect.pivot = new Vector2(0f, 0.5f);
        lagRect.anchoredPosition = Vector2.zero;
        lagRect.sizeDelta = new Vector2(containerWidth * displayedLagPercent, 0f);
    }

    private void ApplyImmediateBarVisual()
    {
        if (healthSlider != null)
        {
            healthSlider.value = displayedHealthPercent;
        }

        UpdateDamageLagVisual();
    }

    private void UpdateLowHealthPulse()
    {
        if (fillImage == null)
        {
            return;
        }

        if (targetHealthPercent > lowHealthThreshold || trackedHealth == null)
        {
            fillImage.color = currentTheme.FillColor;

            if (leftAccentImage != null)
            {
                leftAccentImage.color = currentTheme.AccentColor;
            }

            if (rightAccentImage != null)
            {
                rightAccentImage.color = currentTheme.AccentColor;
            }

            return;
        }

        float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
        float pulseAmount = pulse * lowHealthPulseStrength;

        fillImage.color = Color.Lerp(currentTheme.FillColor, currentTheme.CriticalPulseColor, pulseAmount);

        if (leftAccentImage != null)
        {
            leftAccentImage.color = Color.Lerp(currentTheme.AccentColor, currentTheme.CriticalPulseColor, pulseAmount);
        }

        if (rightAccentImage != null)
        {
            rightAccentImage.color = Color.Lerp(currentTheme.AccentColor, currentTheme.CriticalPulseColor, pulseAmount);
        }
    }

    private void UpdateHealthText(float currentHealth, float maxHealth)
    {
        string healthText = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";

        if (healthValueText != null)
        {
            healthValueText.text = healthText;
            healthValueText.color = currentTheme.HealthTextColor;
        }

        if (healthValueTMPText != null)
        {
            healthValueTMPText.text = healthText;
            healthValueTMPText.color = currentTheme.HealthTextColor;
        }
    }

    private void SetTargetName(string targetName)
    {
        if (eliteNameText != null)
        {
            eliteNameText.text = targetName.ToUpper();
            eliteNameText.color = currentTheme.NameColor;
        }

        if (eliteNameTMPText != null)
        {
            eliteNameTMPText.text = targetName.ToUpper();
            eliteNameTMPText.color = currentTheme.NameColor;
        }
    }

    private void SetTargetTypeLabel(string label)
    {
        if (targetTypeText != null)
        {
            targetTypeText.text = label.ToUpper();
            targetTypeText.color = currentTheme.TypeTextColor;
        }

        if (targetTypeTMPText != null)
        {
            targetTypeTMPText.text = label.ToUpper();
            targetTypeTMPText.color = currentTheme.TypeTextColor;
        }
    }

    private void ApplyTheme(TargetBarTheme theme)
    {
        if (fillImage != null)
        {
            fillImage.color = theme.FillColor;
        }

        if (damageLagFillImage != null)
        {
            damageLagFillImage.color = theme.DamageLagColor;
        }

        if (panelBackgroundImage != null)
        {
            panelBackgroundImage.color = theme.BackgroundColor;
        }

        if (frameImage != null)
        {
            frameImage.color = theme.FrameColor;
        }

        if (glowImage != null)
        {
            glowImage.color = theme.GlowColor;
        }

        if (leftAccentImage != null)
        {
            leftAccentImage.color = theme.AccentColor;
        }

        if (rightAccentImage != null)
        {
            rightAccentImage.color = theme.AccentColor;
        }

        if (eliteNameText != null)
        {
            eliteNameText.color = theme.NameColor;
        }

        if (eliteNameTMPText != null)
        {
            eliteNameTMPText.color = theme.NameColor;
        }

        if (healthValueText != null)
        {
            healthValueText.color = theme.HealthTextColor;
        }

        if (healthValueTMPText != null)
        {
            healthValueTMPText.color = theme.HealthTextColor;
        }

        if (targetTypeText != null)
        {
            targetTypeText.color = theme.TypeTextColor;
        }

        if (targetTypeTMPText != null)
        {
            targetTypeTMPText.color = theme.TypeTextColor;
        }
    }

    private TargetBarTheme GetThemeForStyle(EnemyTargetBarStyle style)
    {
        switch (style)
        {
            case EnemyTargetBarStyle.Elite:
                return new TargetBarTheme
                {
                    TargetTypeLabel = "ELITE TARGET",
                    FillColor = new Color(0.22f, 0.82f, 1f, 1f),
                    DamageLagColor = new Color(0.95f, 0.95f, 1f, 0.7f),
                    BackgroundColor = new Color(0.04f, 0.08f, 0.12f, 0.95f),
                    FrameColor = new Color(0.3f, 0.85f, 1f, 1f),
                    GlowColor = new Color(0.15f, 0.7f, 1f, 0.18f),
                    AccentColor = new Color(0.3f, 0.9f, 1f, 1f),
                    NameColor = Color.white,
                    HealthTextColor = new Color(0.9f, 0.98f, 1f, 1f),
                    TypeTextColor = new Color(0.3f, 0.9f, 1f, 1f),
                    CriticalPulseColor = new Color(1f, 0.35f, 0.18f, 1f)
                };

            case EnemyTargetBarStyle.MiniBoss:
                return new TargetBarTheme
                {
                    TargetTypeLabel = "MINI-BOSS",
                    FillColor = new Color(1f, 0.32f, 0.12f, 1f),
                    DamageLagColor = new Color(1f, 0.88f, 0.55f, 0.7f),
                    BackgroundColor = new Color(0.12f, 0.05f, 0.05f, 0.95f),
                    FrameColor = new Color(1f, 0.45f, 0.2f, 1f),
                    GlowColor = new Color(1f, 0.25f, 0.1f, 0.18f),
                    AccentColor = new Color(1f, 0.42f, 0.16f, 1f),
                    NameColor = Color.white,
                    HealthTextColor = new Color(1f, 0.96f, 0.92f, 1f),
                    TypeTextColor = new Color(1f, 0.7f, 0.32f, 1f),
                    CriticalPulseColor = new Color(1f, 0.9f, 0.15f, 1f)
                };

            case EnemyTargetBarStyle.Boss:
                return new TargetBarTheme
                {
                    TargetTypeLabel = "BOSS THREAT",
                    FillColor = new Color(0.95f, 0.14f, 0.14f, 1f),
                    DamageLagColor = new Color(1f, 0.8f, 0.22f, 0.8f),
                    BackgroundColor = new Color(0.14f, 0.02f, 0.02f, 0.97f),
                    FrameColor = new Color(1f, 0.75f, 0.2f, 1f),
                    GlowColor = new Color(1f, 0.1f, 0.1f, 0.2f),
                    AccentColor = new Color(1f, 0.75f, 0.2f, 1f),
                    NameColor = Color.white,
                    HealthTextColor = new Color(1f, 0.95f, 0.9f, 1f),
                    TypeTextColor = new Color(1f, 0.75f, 0.2f, 1f),
                    CriticalPulseColor = new Color(1f, 1f, 0.2f, 1f)
                };
        }

        return new TargetBarTheme
        {
            TargetTypeLabel = "TARGET",
            FillColor = Color.red,
            DamageLagColor = Color.white,
            BackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.95f),
            FrameColor = Color.white,
            GlowColor = new Color(1f, 1f, 1f, 0.12f),
            AccentColor = Color.white,
            NameColor = Color.white,
            HealthTextColor = Color.white,
            TypeTextColor = Color.white,
            CriticalPulseColor = Color.yellow
        };
    }

    private void ResolveReferences()
    {
        if (barRoot == null)
        {
            barRoot = gameObject;
        }

        if (panelRectTransform == null && barRoot != null)
        {
            panelRectTransform = barRoot.GetComponent<RectTransform>();
        }

        if (panelBackgroundImage == null && barRoot != null)
        {
            panelBackgroundImage = barRoot.GetComponent<Image>();
        }

        if (healthSlider == null && barRoot != null)
        {
            healthSlider = barRoot.GetComponentInChildren<Slider>(true);
        }

        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }

        if (damageLagFillContainer == null && healthSlider != null && healthSlider.fillRect != null)
        {
            damageLagFillContainer = healthSlider.fillRect.parent as RectTransform;
        }
    }

    private void AutoBuildOptionalPolish()
    {
        if (barRoot == null || panelRectTransform == null)
        {
            return;
        }

        if (glowImage == null)
        {
            GameObject glowObject = new GameObject("Glow");
            glowObject.transform.SetParent(barRoot.transform, false);
            glowObject.transform.SetAsFirstSibling();

            RectTransform glowRect = glowObject.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-10f, -8f);
            glowRect.offsetMax = new Vector2(10f, 8f);

            glowImage = glowObject.AddComponent<Image>();
            glowImage.raycastTarget = false;
        }

        if (leftAccentImage == null)
        {
            leftAccentImage = CreateAccentImage("Left Accent", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), 8f);
        }

        if (rightAccentImage == null)
        {
            rightAccentImage = CreateAccentImage("Right Accent", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), 8f);
        }

        if (targetTypeText == null && targetTypeTMPText == null)
        {
            targetTypeText = CreateRuntimeText(
                "Target Type Text",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 8f),
                new Vector2(420f, 22f),
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        if (damageLagFillImage == null && damageLagFillContainer != null)
        {
            GameObject lagObject = new GameObject("Damage Lag Fill");
            lagObject.transform.SetParent(damageLagFillContainer, false);
            lagObject.transform.SetAsFirstSibling();

            RectTransform lagRect = lagObject.AddComponent<RectTransform>();
            lagRect.anchorMin = new Vector2(0f, 0f);
            lagRect.anchorMax = new Vector2(0f, 1f);
            lagRect.pivot = new Vector2(0f, 0.5f);
            lagRect.anchoredPosition = Vector2.zero;
            lagRect.sizeDelta = new Vector2(0f, 0f);

            damageLagFillImage = lagObject.AddComponent<Image>();
            damageLagFillImage.raycastTarget = false;
        }
    }

    private Image CreateAccentImage(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        float width)
    {
        GameObject accentObject = new GameObject(objectName);
        accentObject.transform.SetParent(barRoot.transform, false);

        RectTransform accentRect = accentObject.AddComponent<RectTransform>();
        accentRect.anchorMin = anchorMin;
        accentRect.anchorMax = anchorMax;
        accentRect.pivot = pivot;
        accentRect.sizeDelta = new Vector2(width, 0f);
        accentRect.anchoredPosition = Vector2.zero;

        Image accentImage = accentObject.AddComponent<Image>();
        accentImage.raycastTarget = false;

        return accentImage;
    }

    private Text CreateRuntimeText(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(barRoot.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Text text = textObject.AddComponent<Text>();
        text.font = runtimeFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.text = "";

        return text;
    }

    private void ShowBar()
    {
        if (barRoot != null)
        {
            barRoot.SetActive(true);
        }
    }

    private void HideBarInstant()
    {
        if (barRoot != null)
        {
            barRoot.SetActive(false);
        }

        if (panelRectTransform != null)
        {
            panelRectTransform.localScale = basePanelScale;
        }
    }

    private void UnsubscribeFromTrackedHealth()
    {
        if (trackedHealth == null)
        {
            return;
        }

        trackedHealth.OnHealthChanged -= HandleHealthChanged;
        trackedHealth.OnDied -= HandleTargetDied;
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
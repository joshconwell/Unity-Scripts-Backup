using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDOverhaul : MonoBehaviour
{
    private class AnimatedBar
    {
        public RectTransform ContainerRect;
        public RectTransform FillRect;
        public RectTransform LagFillRect;
        public Image FillImage;
        public Image LagFillImage;
        public float TargetPercent = 1f;
        public float DisplayedPercent = 1f;
        public float LagPercent = 1f;
        public Color NormalFillColor = Color.white;
    }

    private class StatCard
    {
        public RectTransform RootRect;
        public Image PanelImage;
        public Image AccentImage;
        public Text LabelText;
        public Text ValueText;
        public Vector3 BaseScale = Vector3.one;
        public float PopTimer;
    }

    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private GameRunStats gameRunStats;

    [Header("General HUD")]
    [SerializeField] private bool createCanvasIfMissing = true;
    [SerializeField] private int canvasSortingOrder = 150;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Health HUD")]
    [SerializeField] private Vector2 healthPanelPosition = new Vector2(24f, -24f);
    [SerializeField] private Vector2 healthPanelSize = new Vector2(390f, 92f);
    [SerializeField] private Color healthPanelColor = new Color(0.035f, 0.035f, 0.045f, 0.88f);
    [SerializeField] private Color healthAccentColor = new Color(1f, 0.2f, 0.16f, 1f);
    [SerializeField] private Color healthFillColor = new Color(1f, 0.16f, 0.12f, 1f);
    [SerializeField] private Color healthLagColor = new Color(1f, 0.75f, 0.28f, 0.78f);
    [SerializeField] private Color lowHealthPulseColor = new Color(1f, 0.9f, 0.15f, 1f);
    [SerializeField] private float lowHealthThreshold = 0.28f;
    [SerializeField] private float lowHealthPulseSpeed = 8f;

    [Header("XP HUD")]
    [SerializeField] private Vector2 xpPanelPosition = new Vector2(0f, 24f);
    [SerializeField] private Vector2 xpPanelSize = new Vector2(760f, 70f);
    [SerializeField] private Color xpPanelColor = new Color(0.025f, 0.035f, 0.055f, 0.9f);
    [SerializeField] private Color xpAccentColor = new Color(0.35f, 0.55f, 1f, 1f);
    [SerializeField] private Color xpFillColor = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private Color xpLagColor = new Color(0.9f, 0.95f, 1f, 0.65f);

    [Header("Stat Cards")]
    [SerializeField] private Vector2 timerCardPosition = new Vector2(0f, -24f);
    [SerializeField] private Vector2 killCardPosition = new Vector2(-24f, -24f);
    [SerializeField] private Vector2 statCardSize = new Vector2(180f, 68f);
    [SerializeField] private Color statCardColor = new Color(0.025f, 0.025f, 0.035f, 0.88f);
    [SerializeField] private Color timerAccentColor = new Color(0.5f, 0.85f, 1f, 1f);
    [SerializeField] private Color killAccentColor = new Color(1f, 0.65f, 0.18f, 1f);

    [Header("Text")]
    [SerializeField] private Color labelTextColor = new Color(0.7f, 0.78f, 0.9f, 1f);
    [SerializeField] private Color mainTextColor = Color.white;
    [SerializeField] private int labelFontSize = 14;
    [SerializeField] private int mainFontSize = 24;

    [Header("Animation")]
    [SerializeField] private float frontBarLerpSpeed = 7f;
    [SerializeField] private float lagBarDropSpeed = 1.35f;
    [SerializeField] private float lagBarCatchUpSpeed = 9f;
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float popScale = 1.12f;
    [SerializeField] private float referenceSearchInterval = 0.5f;

    private Canvas canvas;
    private RectTransform rootRect;

    private AnimatedBar healthBar = new AnimatedBar();
    private AnimatedBar xpBar = new AnimatedBar();

    private RectTransform healthPanelRect;
    private RectTransform xpPanelRect;

    private Image healthGlowImage;
    private Image xpGlowImage;

    private Text healthValueText;
    private Text xpValueText;
    private Text levelText;

    private StatCard timerCard;
    private StatCard killCard;

    private Font runtimeFont;

    private Health subscribedHealth;
    private PlayerExperience subscribedExperience;
    private GameRunStats subscribedRunStats;

    private float referenceSearchTimer;
    private int lastKillCount = -1;
    private int lastLevel = -1;
    private bool receivedHealthOnce;
    private bool receivedXPOnce;

    private void Awake()
    {
        runtimeFont = GetRuntimeFont();

        AutoFindReferences();
        BuildHUD();
        RefreshAll();
    }

    private void OnEnable()
    {
        AutoFindReferences();
        SubscribeToReferences();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeFromHealth();
        UnsubscribeFromExperience();
        UnsubscribeFromRunStats();
    }

    private void Update()
    {
        referenceSearchTimer -= Time.unscaledDeltaTime;

        if (referenceSearchTimer <= 0f)
        {
            referenceSearchTimer = referenceSearchInterval;

            AutoFindReferences();
            SubscribeToReferences();
            RefreshAllMissingOnly();
        }

        UpdateAnimatedBar(healthBar);
        UpdateAnimatedBar(xpBar);

        UpdateHealthPulse();
        UpdatePanelPop(xpPanelRect, ref xpPanelPopTimer);
        UpdateStatCardPop(timerCard);
        UpdateStatCardPop(killCard);
    }

    private float xpPanelPopTimer;

    private void AutoFindReferences()
    {
        if (playerHealth == null || playerExperience == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                if (playerHealth == null)
                {
                    playerHealth = playerObject.GetComponent<Health>();
                }

                if (playerExperience == null)
                {
                    playerExperience = playerObject.GetComponent<PlayerExperience>();
                }
            }
        }

        if (gameRunStats == null && GameRunStats.HasInstance)
        {
            gameRunStats = GameRunStats.Instance;
        }
    }

    private void SubscribeToReferences()
    {
        SubscribeToHealth();
        SubscribeToExperience();
        SubscribeToRunStats();
    }

    private void SubscribeToHealth()
    {
        if (subscribedHealth == playerHealth)
        {
            return;
        }

        UnsubscribeFromHealth();

        if (playerHealth == null)
        {
            return;
        }

        subscribedHealth = playerHealth;
        subscribedHealth.OnHealthChanged += HandleHealthChanged;
        subscribedHealth.OnDied += HandlePlayerDied;
    }

    private void SubscribeToExperience()
    {
        if (subscribedExperience == playerExperience)
        {
            return;
        }

        UnsubscribeFromExperience();

        if (playerExperience == null)
        {
            return;
        }

        subscribedExperience = playerExperience;
        subscribedExperience.OnExperienceChanged += HandleExperienceChanged;
        subscribedExperience.OnLevelChanged += HandleLevelChanged;
        subscribedExperience.OnLevelUp += HandleLevelUp;
    }

    private void SubscribeToRunStats()
    {
        if (subscribedRunStats == gameRunStats)
        {
            return;
        }

        UnsubscribeFromRunStats();

        if (gameRunStats == null)
        {
            return;
        }

        subscribedRunStats = gameRunStats;
        subscribedRunStats.OnTimerChanged += HandleTimerChanged;
        subscribedRunStats.OnKillCountChanged += HandleKillCountChanged;
        subscribedRunStats.OnRunEnded += HandleRunEnded;
    }

    private void UnsubscribeFromHealth()
    {
        if (subscribedHealth == null)
        {
            return;
        }

        subscribedHealth.OnHealthChanged -= HandleHealthChanged;
        subscribedHealth.OnDied -= HandlePlayerDied;
        subscribedHealth = null;
    }

    private void UnsubscribeFromExperience()
    {
        if (subscribedExperience == null)
        {
            return;
        }

        subscribedExperience.OnExperienceChanged -= HandleExperienceChanged;
        subscribedExperience.OnLevelChanged -= HandleLevelChanged;
        subscribedExperience.OnLevelUp -= HandleLevelUp;
        subscribedExperience = null;
    }

    private void UnsubscribeFromRunStats()
    {
        if (subscribedRunStats == null)
        {
            return;
        }

        subscribedRunStats.OnTimerChanged -= HandleTimerChanged;
        subscribedRunStats.OnKillCountChanged -= HandleKillCountChanged;
        subscribedRunStats.OnRunEnded -= HandleRunEnded;
        subscribedRunStats = null;
    }

    private void RefreshAll()
    {
        if (playerHealth != null)
        {
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        else
        {
            SetHealthText(0f, 1f);
            SetBarImmediate(healthBar, 0f);
        }

        if (playerExperience != null)
        {
            HandleExperienceChanged(
                playerExperience.CurrentExperience,
                playerExperience.ExperienceToNextLevel
            );

            HandleLevelChanged(playerExperience.CurrentLevel);
        }
        else
        {
            SetXPText(0, 1);
            SetLevelText(1);
            SetBarImmediate(xpBar, 0f);
        }

        if (gameRunStats != null)
        {
            HandleTimerChanged(gameRunStats.ElapsedTime);
            HandleKillCountChanged(gameRunStats.KillCount);
        }
        else
        {
            HandleTimerChanged(0f);
            HandleKillCountChanged(0);
        }
    }

    private void RefreshAllMissingOnly()
    {
        if (playerHealth != null && subscribedHealth == playerHealth)
        {
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (playerExperience != null && subscribedExperience == playerExperience)
        {
            HandleExperienceChanged(
                playerExperience.CurrentExperience,
                playerExperience.ExperienceToNextLevel
            );

            HandleLevelChanged(playerExperience.CurrentLevel);
        }

        if (gameRunStats != null && subscribedRunStats == gameRunStats)
        {
            HandleTimerChanged(gameRunStats.ElapsedTime);
            HandleKillCountChanged(gameRunStats.KillCount);
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            maxHealth = 1f;
        }

        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        healthBar.TargetPercent = healthPercent;
        SetHealthText(currentHealth, maxHealth);

        if (!receivedHealthOnce)
        {
            receivedHealthOnce = true;
            SetBarImmediate(healthBar, healthPercent);
        }
    }

    private void HandlePlayerDied()
    {
        healthBar.TargetPercent = 0f;

        if (healthValueText != null)
        {
            healthValueText.text = "0 / " + Mathf.CeilToInt(playerHealth != null ? playerHealth.MaxHealth : 1f);
        }
    }

    private void HandleExperienceChanged(int currentExperience, int experienceToNextLevel)
    {
        if (experienceToNextLevel <= 0)
        {
            experienceToNextLevel = 1;
        }

        float xpPercent = Mathf.Clamp01((float)currentExperience / experienceToNextLevel);

        xpBar.TargetPercent = xpPercent;
        SetXPText(currentExperience, experienceToNextLevel);

        if (!receivedXPOnce)
        {
            receivedXPOnce = true;
            SetBarImmediate(xpBar, xpPercent);
        }
    }

    private void HandleLevelChanged(int currentLevel)
    {
        SetLevelText(currentLevel);

        if (lastLevel < 0)
        {
            lastLevel = currentLevel;
            return;
        }

        if (currentLevel != lastLevel)
        {
            lastLevel = currentLevel;
            xpPanelPopTimer = popDuration;
        }
    }

    private void HandleLevelUp(int currentLevel)
    {
        SetLevelText(currentLevel);
        xpPanelPopTimer = popDuration;
    }

    private void HandleTimerChanged(float elapsedTime)
    {
        if (timerCard == null || timerCard.ValueText == null)
        {
            return;
        }

        timerCard.ValueText.text = FormatTime(elapsedTime);
    }

    private void HandleKillCountChanged(int killCount)
    {
        if (killCard == null || killCard.ValueText == null)
        {
            return;
        }

        killCard.ValueText.text = killCount.ToString();

        if (lastKillCount < 0)
        {
            lastKillCount = killCount;
            return;
        }

        if (killCount != lastKillCount)
        {
            lastKillCount = killCount;
            killCard.PopTimer = popDuration;
        }
    }

    private void HandleRunEnded(float finalTime, int finalKills)
    {
        HandleTimerChanged(finalTime);
        HandleKillCountChanged(finalKills);
    }

    private void BuildHUD()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null && createCanvasIfMissing)
        {
            GameObject canvasObject = new GameObject("Player HUD Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortingOrder;

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            transform.SetParent(canvasObject.transform, false);
        }
        else
        {
            canvas = parentCanvas;
        }

        Transform parent = canvas != null ? canvas.transform : transform;

        GameObject rootObject = new GameObject("Player HUD Overhaul Root");
        rootObject.transform.SetParent(parent, false);

        rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = rootObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        BuildHealthPanel(rootRect);
        BuildXPPanel(rootRect);
        BuildStatCards(rootRect);
    }

    private void BuildHealthPanel(Transform parent)
    {
        Image panelImage = CreateImage(
            parent,
            "Health Panel",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            healthPanelPosition,
            healthPanelSize,
            healthPanelColor
        );

        healthPanelRect = panelImage.rectTransform;

        healthGlowImage = CreateImage(
            healthPanelRect,
            "Health Glow",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(1f, 0.1f, 0.05f, 0.08f)
        );

        healthGlowImage.rectTransform.offsetMin = new Vector2(-8f, -8f);
        healthGlowImage.rectTransform.offsetMax = new Vector2(8f, 8f);
        healthGlowImage.transform.SetAsFirstSibling();

        CreateImage(
            healthPanelRect,
            "Health Accent",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            new Vector2(8f, 0f),
            healthAccentColor
        );

        Text label = CreateText(
            healthPanelRect,
            "Health Label",
            "HEALTH",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -10f),
            new Vector2(150f, 24f),
            labelFontSize,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            labelTextColor
        );

        healthValueText = CreateText(
            healthPanelRect,
            "Health Value",
            "100 / 100",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-18f, -10f),
            new Vector2(190f, 28f),
            mainFontSize,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            mainTextColor
        );

        Image barBack = CreateImage(
            healthPanelRect,
            "Health Bar Back",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(10f, 16f),
            new Vector2(-44f, 22f),
            new Color(0.08f, 0.01f, 0.01f, 0.92f)
        );

        RectTransform barBackRect = barBack.rectTransform;
        barBackRect.offsetMin = new Vector2(24f, 16f);
        barBackRect.offsetMax = new Vector2(-20f, 38f);

        healthBar.ContainerRect = barBackRect;

        Image lagImage = CreateImage(
            barBackRect,
            "Health Lag Fill",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            healthLagColor
        );

        Image fillImage = CreateImage(
            barBackRect,
            "Health Fill",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            healthFillColor
        );

        healthBar.LagFillImage = lagImage;
        healthBar.LagFillRect = lagImage.rectTransform;
        healthBar.FillImage = fillImage;
        healthBar.FillRect = fillImage.rectTransform;
        healthBar.NormalFillColor = healthFillColor;
    }

    private void BuildXPPanel(Transform parent)
    {
        Image panelImage = CreateImage(
            parent,
            "XP Panel",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            xpPanelPosition,
            xpPanelSize,
            xpPanelColor
        );

        xpPanelRect = panelImage.rectTransform;

        xpGlowImage = CreateImage(
            xpPanelRect,
            "XP Glow",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(0.2f, 0.45f, 1f, 0.1f)
        );

        xpGlowImage.rectTransform.offsetMin = new Vector2(-8f, -6f);
        xpGlowImage.rectTransform.offsetMax = new Vector2(8f, 6f);
        xpGlowImage.transform.SetAsFirstSibling();

        CreateImage(
            xpPanelRect,
            "XP Top Accent",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            Vector2.zero,
            new Vector2(0f, 4f),
            xpAccentColor
        );

        Image levelBadge = CreateImage(
            xpPanelRect,
            "Level Badge",
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(18f, 0f),
            new Vector2(110f, 46f),
            new Color(0.08f, 0.13f, 0.25f, 0.95f)
        );

        levelText = CreateText(
            levelBadge.rectTransform,
            "Level Text",
            "LVL 1",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            23,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            mainTextColor
        );

        Text xpLabel = CreateText(
            xpPanelRect,
            "XP Label",
            "EXPERIENCE",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(144f, -10f),
            new Vector2(180f, 22f),
            labelFontSize,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            labelTextColor
        );

        xpValueText = CreateText(
            xpPanelRect,
            "XP Value",
            "0 / 10 XP",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-18f, -10f),
            new Vector2(220f, 22f),
            17,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            mainTextColor
        );

        Image barBack = CreateImage(
            xpPanelRect,
            "XP Bar Back",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            Vector2.zero,
            Vector2.zero,
            new Color(0.035f, 0.055f, 0.095f, 0.95f)
        );

        RectTransform barBackRect = barBack.rectTransform;
        barBackRect.offsetMin = new Vector2(144f, 14f);
        barBackRect.offsetMax = new Vector2(-18f, 34f);

        xpBar.ContainerRect = barBackRect;

        Image lagImage = CreateImage(
            barBackRect,
            "XP Lag Fill",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            xpLagColor
        );

        Image fillImage = CreateImage(
            barBackRect,
            "XP Fill",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            xpFillColor
        );

        xpBar.LagFillImage = lagImage;
        xpBar.LagFillRect = lagImage.rectTransform;
        xpBar.FillImage = fillImage;
        xpBar.FillRect = fillImage.rectTransform;
        xpBar.NormalFillColor = xpFillColor;
    }

    private void BuildStatCards(Transform parent)
    {
        timerCard = CreateStatCard(
            parent,
            "Timer Card",
            "TIME",
            "00:00",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            timerCardPosition,
            timerAccentColor
        );

        killCard = CreateStatCard(
            parent,
            "Kill Card",
            "KILLS",
            "0",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            killCardPosition,
            killAccentColor
        );
    }

    private StatCard CreateStatCard(
        Transform parent,
        string objectName,
        string label,
        string value,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Color accentColor)
    {
        StatCard card = new StatCard();

        Image panelImage = CreateImage(
            parent,
            objectName,
            anchorMin,
            anchorMax,
            pivot,
            anchoredPosition,
            statCardSize,
            statCardColor
        );

        card.RootRect = panelImage.rectTransform;
        card.PanelImage = panelImage;
        card.BaseScale = Vector3.one;

        card.AccentImage = CreateImage(
            card.RootRect,
            "Accent",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            Vector2.zero,
            new Vector2(0f, 4f),
            accentColor
        );

        card.LabelText = CreateText(
            card.RootRect,
            "Label",
            label,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -8f),
            new Vector2(0f, 22f),
            labelFontSize,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            labelTextColor
        );

        card.ValueText = CreateText(
            card.RootRect,
            "Value",
            value,
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 8f),
            new Vector2(0f, 34f),
            mainFontSize,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            mainTextColor
        );

        return card;
    }

    private Image CreateImage(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private Text CreateText(
        Transform parent,
        string objectName,
        string textValue,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Text text = textObject.AddComponent<Text>();
        text.text = textValue;
        text.font = runtimeFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private void UpdateAnimatedBar(AnimatedBar bar)
    {
        if (bar == null || bar.ContainerRect == null || bar.FillRect == null)
        {
            return;
        }

        bar.DisplayedPercent = Mathf.MoveTowards(
            bar.DisplayedPercent,
            bar.TargetPercent,
            frontBarLerpSpeed * Time.unscaledDeltaTime
        );

        if (bar.LagPercent < bar.DisplayedPercent)
        {
            bar.LagPercent = Mathf.MoveTowards(
                bar.LagPercent,
                bar.DisplayedPercent,
                lagBarCatchUpSpeed * Time.unscaledDeltaTime
            );
        }
        else
        {
            bar.LagPercent = Mathf.MoveTowards(
                bar.LagPercent,
                bar.DisplayedPercent,
                lagBarDropSpeed * Time.unscaledDeltaTime
            );
        }

        ApplyBarVisual(bar);
    }

    private void ApplyBarVisual(AnimatedBar bar)
    {
        float containerWidth = bar.ContainerRect.rect.width;

        if (containerWidth <= 0f)
        {
            containerWidth = bar.ContainerRect.sizeDelta.x;
        }

        if (containerWidth <= 0f)
        {
            return;
        }

        if (bar.LagFillRect != null)
        {
            bar.LagFillRect.anchorMin = new Vector2(0f, 0f);
            bar.LagFillRect.anchorMax = new Vector2(0f, 1f);
            bar.LagFillRect.pivot = new Vector2(0f, 0.5f);
            bar.LagFillRect.anchoredPosition = Vector2.zero;
            bar.LagFillRect.sizeDelta = new Vector2(containerWidth * bar.LagPercent, 0f);
        }

        bar.FillRect.anchorMin = new Vector2(0f, 0f);
        bar.FillRect.anchorMax = new Vector2(0f, 1f);
        bar.FillRect.pivot = new Vector2(0f, 0.5f);
        bar.FillRect.anchoredPosition = Vector2.zero;
        bar.FillRect.sizeDelta = new Vector2(containerWidth * bar.DisplayedPercent, 0f);
    }

    private void SetBarImmediate(AnimatedBar bar, float percent)
    {
        percent = Mathf.Clamp01(percent);

        bar.TargetPercent = percent;
        bar.DisplayedPercent = percent;
        bar.LagPercent = percent;

        ApplyBarVisual(bar);
    }

    private void UpdateHealthPulse()
    {
        if (healthBar.FillImage == null)
        {
            return;
        }

        if (healthBar.TargetPercent > lowHealthThreshold)
        {
            healthBar.FillImage.color = healthFillColor;

            if (healthGlowImage != null)
            {
                healthGlowImage.color = new Color(1f, 0.1f, 0.05f, 0.08f);
            }

            return;
        }

        float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;

        healthBar.FillImage.color = Color.Lerp(
            healthFillColor,
            lowHealthPulseColor,
            pulse
        );

        if (healthGlowImage != null)
        {
            healthGlowImage.color = Color.Lerp(
                new Color(1f, 0.1f, 0.05f, 0.08f),
                new Color(1f, 0.1f, 0.05f, 0.28f),
                pulse
            );
        }
    }

    private void UpdatePanelPop(RectTransform panelRect, ref float timer)
    {
        if (panelRect == null)
        {
            return;
        }

        if (timer <= 0f)
        {
            panelRect.localScale = Vector3.one;
            return;
        }

        timer -= Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(timer / popDuration);
        float scale = Mathf.Lerp(1f, popScale, Mathf.Sin(t * Mathf.PI));

        panelRect.localScale = Vector3.one * scale;
    }

    private void UpdateStatCardPop(StatCard card)
    {
        if (card == null || card.RootRect == null)
        {
            return;
        }

        if (card.PopTimer <= 0f)
        {
            card.RootRect.localScale = card.BaseScale;
            return;
        }

        card.PopTimer -= Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(card.PopTimer / popDuration);
        float scale = Mathf.Lerp(1f, popScale, Mathf.Sin(t * Mathf.PI));

        card.RootRect.localScale = card.BaseScale * scale;
    }

    private void SetHealthText(float currentHealth, float maxHealth)
    {
        if (healthValueText == null)
        {
            return;
        }

        healthValueText.text =
            $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private void SetXPText(int currentXP, int neededXP)
    {
        if (xpValueText == null)
        {
            return;
        }

        xpValueText.text = $"{currentXP} / {neededXP} XP";
    }

    private void SetLevelText(int currentLevel)
    {
        if (levelText == null)
        {
            return;
        }

        levelText.text = $"LVL {currentLevel}";
    }

    private string FormatTime(float timeInSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
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
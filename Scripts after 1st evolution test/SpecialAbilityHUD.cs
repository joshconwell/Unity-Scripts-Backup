using UnityEngine;
using UnityEngine.UI;

public class SpecialAbilityHUD : MonoBehaviour
{
    private class AbilitySlot
    {
        public GameObject RootObject;
        public Image BackgroundImage;
        public Text IconText;
        public Text NameText;
        public Text DetailText;
    }

    [Header("References")]
    [SerializeField] private PlayerSpecialAbilities playerSpecialAbilities;

    [Header("HUD Position")]
    [SerializeField] private Vector2 anchoredPosition = new Vector2(20f, 150f);
    [SerializeField] private Vector2 slotSize = new Vector2(82f, 74f);
    [SerializeField] private float spacing = 8f;

    [Header("HUD Style")]
    [SerializeField] private Color panelBackgroundColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color explosiveColor = new Color(1f, 0.45f, 0.08f, 0.85f);
    [SerializeField] private Color lightningColor = new Color(0.25f, 0.75f, 1f, 0.85f);
    [SerializeField] private Color bladeColor = new Color(0.55f, 0.95f, 1f, 0.85f);
    [SerializeField] private Color fireTrailColor = new Color(1f, 0.22f, 0.03f, 0.85f);
    [SerializeField] private Color blackHoleColor = new Color(0.45f, 0.15f, 1f, 0.85f);
    [SerializeField] private Color droneTurretColor = new Color(1f, 0.85f, 0.2f, 0.85f);
    [SerializeField] private Color iceNovaColor = new Color(0.25f, 0.9f, 1f, 0.85f);
    [SerializeField] private Color poisonCloudColor = new Color(0.25f, 1f, 0.1f, 0.85f);
    [SerializeField] private Color ricochetColor = new Color(1f, 0.35f, 0.9f, 0.85f);
    [SerializeField] private Color laserBeamColor = new Color(1f, 0.12f, 0.08f, 0.85f);
    [SerializeField] private Color shockwaveColor = new Color(0.55f, 0.85f, 1f, 0.85f);
    [SerializeField] private Color guardianShieldColor = new Color(0.35f, 0.95f, 1f, 0.85f);
    [SerializeField] private Color meteorStrikeColor = new Color(1f, 0.55f, 0.08f, 0.85f);
    [SerializeField] private Color shrapnelMinesColor = new Color(1f, 0.78f, 0.18f, 0.85f);
    [SerializeField] private Color bloodPactColor = new Color(0.85f, 0.05f, 0.08f, 0.85f);
    [SerializeField] private Color timeFractureColor = new Color(0.75f, 0.35f, 1f, 0.85f);
    [SerializeField] private Color cryoBlastColor = new Color(0.65f, 1f, 1f, 0.95f);
    [SerializeField] private Color textColor = Color.white;

    [Header("Text Sizes")]
    [SerializeField] private int iconFontSize = 20;
    [SerializeField] private int nameFontSize = 12;
    [SerializeField] private int detailFontSize = 11;

    [Header("Settings")]
    [SerializeField] private bool hidePanelWhenNoAbilities = true;
    [SerializeField] private float refreshInterval = 0.15f;

    private RectTransform rootRectTransform;
    private Image rootBackgroundImage;
    private HorizontalLayoutGroup horizontalLayoutGroup;

    private AbilitySlot explosiveSlot;
    private AbilitySlot lightningSlot;
    private AbilitySlot bladeSlot;
    private AbilitySlot fireTrailSlot;
    private AbilitySlot blackHoleSlot;
    private AbilitySlot droneTurretSlot;
    private AbilitySlot iceNovaSlot;
    private AbilitySlot poisonCloudSlot;
    private AbilitySlot ricochetSlot;
    private AbilitySlot laserBeamSlot;
    private AbilitySlot shockwaveSlot;
    private AbilitySlot guardianShieldSlot;
    private AbilitySlot meteorStrikeSlot;
    private AbilitySlot shrapnelMinesSlot;
    private AbilitySlot bloodPactSlot;
    private AbilitySlot timeFractureSlot;
    private AbilitySlot cryoBlastSlot;

    private Font runtimeFont;
    private float refreshTimer;

    private void Awake()
    {
        AutoFindReferences();

        runtimeFont = GetRuntimeFont();

        BuildHUD();
        RefreshHUD();
    }

    private void Update()
    {
        if (playerSpecialAbilities == null)
        {
            AutoFindReferences();
        }

        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer > 0f)
        {
            return;
        }

        refreshTimer = refreshInterval;

        RefreshHUD();
    }

    private void AutoFindReferences()
    {
        if (playerSpecialAbilities != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        playerSpecialAbilities = playerObject.GetComponent<PlayerSpecialAbilities>();
    }

    private void BuildHUD()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Special Ability HUD Canvas");

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            transform.SetParent(canvasObject.transform, false);
        }

        GameObject rootObject = new GameObject("Special Ability HUD Root");
        rootObject.transform.SetParent(canvas.transform, false);

        rootRectTransform = rootObject.AddComponent<RectTransform>();
        rootRectTransform.anchorMin = new Vector2(0f, 0f);
        rootRectTransform.anchorMax = new Vector2(0f, 0f);
        rootRectTransform.pivot = new Vector2(0f, 0f);
        rootRectTransform.anchoredPosition = anchoredPosition;

        rootBackgroundImage = rootObject.AddComponent<Image>();
        rootBackgroundImage.color = panelBackgroundColor;

        horizontalLayoutGroup = rootObject.AddComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayoutGroup.spacing = spacing;
        horizontalLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
        horizontalLayoutGroup.childForceExpandWidth = false;
        horizontalLayoutGroup.childForceExpandHeight = false;
        horizontalLayoutGroup.childControlWidth = false;
        horizontalLayoutGroup.childControlHeight = false;

        ContentSizeFitter contentSizeFitter = rootObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        explosiveSlot = CreateAbilitySlot(rootObject.transform, "EXP", "Explosive", explosiveColor);
        lightningSlot = CreateAbilitySlot(rootObject.transform, "LGT", "Lightning", lightningColor);
        bladeSlot = CreateAbilitySlot(rootObject.transform, "BLD", "Blade", bladeColor);
        fireTrailSlot = CreateAbilitySlot(rootObject.transform, "FIR", "Fire", fireTrailColor);
        blackHoleSlot = CreateAbilitySlot(rootObject.transform, "BLK", "Black Hole", blackHoleColor);
        droneTurretSlot = CreateAbilitySlot(rootObject.transform, "DRN", "Drone", droneTurretColor);
        iceNovaSlot = CreateAbilitySlot(rootObject.transform, "ICE", "Ice", iceNovaColor);
        poisonCloudSlot = CreateAbilitySlot(rootObject.transform, "PSN", "Poison", poisonCloudColor);
        ricochetSlot = CreateAbilitySlot(rootObject.transform, "RIC", "Ricochet", ricochetColor);
        laserBeamSlot = CreateAbilitySlot(rootObject.transform, "LSR", "Laser", laserBeamColor);
        shockwaveSlot = CreateAbilitySlot(rootObject.transform, "SHK", "Shock", shockwaveColor);
        guardianShieldSlot = CreateAbilitySlot(rootObject.transform, "GRD", "Shield", guardianShieldColor);
        meteorStrikeSlot = CreateAbilitySlot(rootObject.transform, "MET", "Meteor", meteorStrikeColor);
        shrapnelMinesSlot = CreateAbilitySlot(rootObject.transform, "MIN", "Mines", shrapnelMinesColor);
        bloodPactSlot = CreateAbilitySlot(rootObject.transform, "PCT", "Pact", bloodPactColor);
        timeFractureSlot = CreateAbilitySlot(rootObject.transform, "TIM", "Time", timeFractureColor);
        cryoBlastSlot = CreateAbilitySlot(rootObject.transform, "CRY", "Cryo+", cryoBlastColor);
    }

    private AbilitySlot CreateAbilitySlot(
        Transform parent,
        string iconText,
        string nameText,
        Color backgroundColor)
    {
        AbilitySlot slot = new AbilitySlot();

        GameObject slotObject = new GameObject(nameText + " Slot");
        slotObject.transform.SetParent(parent, false);

        RectTransform slotRectTransform = slotObject.AddComponent<RectTransform>();
        slotRectTransform.sizeDelta = slotSize;

        LayoutElement layoutElement = slotObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = slotSize.x;
        layoutElement.preferredHeight = slotSize.y;
        layoutElement.minWidth = slotSize.x;
        layoutElement.minHeight = slotSize.y;

        Image slotImage = slotObject.AddComponent<Image>();
        slotImage.color = backgroundColor;

        VerticalLayoutGroup verticalLayoutGroup = slotObject.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        verticalLayoutGroup.spacing = 1f;
        verticalLayoutGroup.padding = new RectOffset(4, 4, 4, 4);
        verticalLayoutGroup.childForceExpandWidth = true;
        verticalLayoutGroup.childForceExpandHeight = false;
        verticalLayoutGroup.childControlWidth = true;
        verticalLayoutGroup.childControlHeight = false;

        slot.RootObject = slotObject;
        slot.BackgroundImage = slotImage;
        slot.IconText = CreateText(slotObject.transform, iconText, iconFontSize, FontStyle.Bold);
        slot.NameText = CreateText(slotObject.transform, nameText, nameFontSize, FontStyle.Bold);
        slot.DetailText = CreateText(slotObject.transform, "", detailFontSize, FontStyle.Normal);

        return slot;
    }

    private Text CreateText(
        Transform parent,
        string startingText,
        int fontSize,
        FontStyle fontStyle)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(slotSize.x, 18f);

        Text text = textObject.AddComponent<Text>();
        text.text = startingText;
        text.font = runtimeFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = textColor;
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

        font = Font.CreateDynamicFontFromOSFont(
            new string[] { "Arial", "Liberation Sans", "Verdana" },
            14
        );

        return font;
    }

    private void RefreshHUD()
    {
        if (rootRectTransform == null)
        {
            return;
        }

        bool hasAnyAbility = false;

        if (playerSpecialAbilities == null)
        {
            SetAllSlotsVisible(false);
            SetRootVisible(false);
            return;
        }

        bool hasExplosiveShots = playerSpecialAbilities.ExplosiveShotsUnlocked;
        bool hasLightningStrike = playerSpecialAbilities.LightningStrikeUnlocked;
        bool hasOrbitingBlade = playerSpecialAbilities.OrbitingBladeUnlocked;
        bool hasFireTrail = playerSpecialAbilities.FireTrailUnlocked;
        bool hasBlackHole = playerSpecialAbilities.BlackHoleUnlocked;
        bool hasDroneTurret = playerSpecialAbilities.DroneTurretUnlocked;
        bool hasIceNova = playerSpecialAbilities.IceNovaUnlocked;
        bool hasPoisonCloud = playerSpecialAbilities.PoisonCloudUnlocked;
        bool hasRicochetRounds = playerSpecialAbilities.RicochetRoundsUnlocked;
        bool hasLaserBeam = playerSpecialAbilities.LaserBeamUnlocked;
        bool hasShockwave = playerSpecialAbilities.ShockwaveUnlocked;
        bool hasGuardianShield = playerSpecialAbilities.GuardianShieldUnlocked;
        bool hasMeteorStrike = playerSpecialAbilities.MeteorStrikeUnlocked;
        bool hasShrapnelMines = playerSpecialAbilities.ShrapnelMinesUnlocked;
        bool hasBloodPact = playerSpecialAbilities.BloodPactUnlocked;
        bool hasTimeFracture = playerSpecialAbilities.TimeFractureUnlocked;
        bool hasCryoBlast = playerSpecialAbilities.CryoBlastUnlocked;

        SetSlotVisible(explosiveSlot, hasExplosiveShots);
        SetSlotVisible(lightningSlot, hasLightningStrike);
        SetSlotVisible(bladeSlot, hasOrbitingBlade);
        SetSlotVisible(fireTrailSlot, hasFireTrail);
        SetSlotVisible(blackHoleSlot, hasBlackHole);
        SetSlotVisible(droneTurretSlot, hasDroneTurret);
        SetSlotVisible(iceNovaSlot, hasIceNova);
        SetSlotVisible(poisonCloudSlot, hasPoisonCloud);
        SetSlotVisible(ricochetSlot, hasRicochetRounds);
        SetSlotVisible(laserBeamSlot, hasLaserBeam);
        SetSlotVisible(shockwaveSlot, hasShockwave);
        SetSlotVisible(guardianShieldSlot, hasGuardianShield);
        SetSlotVisible(meteorStrikeSlot, hasMeteorStrike);
        SetSlotVisible(shrapnelMinesSlot, hasShrapnelMines);
        SetSlotVisible(bloodPactSlot, hasBloodPact);
        SetSlotVisible(timeFractureSlot, hasTimeFracture);
        SetSlotVisible(cryoBlastSlot, hasCryoBlast);

        if (hasExplosiveShots)
        {
            hasAnyAbility = true;
            explosiveSlot.DetailText.text = $"{Mathf.RoundToInt(playerSpecialAbilities.ExplosiveShotDamageMultiplier * 100f)}% dmg";
            explosiveSlot.BackgroundImage.color = explosiveColor;
        }

        if (hasLightningStrike)
        {
            hasAnyAbility = true;

            int strikes = Mathf.Max(1, playerSpecialAbilities.LightningStrikesPerActivation);

            if (strikes > 1)
            {
                lightningSlot.DetailText.text = $"x{strikes}";
            }
            else
            {
                lightningSlot.DetailText.text = $"{playerSpecialAbilities.LightningStrikeCooldown:0.0}s";
            }

            lightningSlot.BackgroundImage.color = lightningColor;
        }

        if (hasOrbitingBlade)
        {
            hasAnyAbility = true;
            bladeSlot.DetailText.text = $"x{playerSpecialAbilities.OrbitingBladeCount}";
            bladeSlot.BackgroundImage.color = bladeColor;
        }

        if (hasFireTrail)
        {
            hasAnyAbility = true;
            fireTrailSlot.DetailText.text = $"{playerSpecialAbilities.FireTrailDamagePerTick:0}/tick";
            fireTrailSlot.BackgroundImage.color = fireTrailColor;
        }

        if (hasBlackHole)
        {
            hasAnyAbility = true;
            blackHoleSlot.DetailText.text = $"{playerSpecialAbilities.BlackHoleCooldown:0.0}s";
            blackHoleSlot.BackgroundImage.color = blackHoleColor;
        }

        if (hasDroneTurret)
        {
            hasAnyAbility = true;
            droneTurretSlot.DetailText.text = $"x{playerSpecialAbilities.DroneTurretCount}";
            droneTurretSlot.BackgroundImage.color = droneTurretColor;
        }

        if (hasIceNova)
        {
            hasAnyAbility = true;
            iceNovaSlot.DetailText.text = $"{playerSpecialAbilities.IceNovaCooldown:0.0}s";
            iceNovaSlot.BackgroundImage.color = iceNovaColor;
        }

        if (hasPoisonCloud)
        {
            hasAnyAbility = true;
            poisonCloudSlot.DetailText.text = $"{playerSpecialAbilities.PoisonCloudDamagePerTick:0}/tick";
            poisonCloudSlot.BackgroundImage.color = poisonCloudColor;
        }

        if (hasRicochetRounds)
        {
            hasAnyAbility = true;
            ricochetSlot.DetailText.text = $"x{playerSpecialAbilities.RicochetBounceCount}";
            ricochetSlot.BackgroundImage.color = ricochetColor;
        }

        if (hasLaserBeam)
        {
            hasAnyAbility = true;
            laserBeamSlot.DetailText.text = $"x{playerSpecialAbilities.LaserBeamCount}";
            laserBeamSlot.BackgroundImage.color = laserBeamColor;
        }

        if (hasShockwave)
        {
            hasAnyAbility = true;
            shockwaveSlot.DetailText.text = $"{playerSpecialAbilities.ShockwaveDamage:0} dmg";
            shockwaveSlot.BackgroundImage.color = shockwaveColor;
        }

        if (hasGuardianShield)
        {
            hasAnyAbility = true;
            guardianShieldSlot.DetailText.text = $"x{playerSpecialAbilities.GuardianShieldCount}";
            guardianShieldSlot.BackgroundImage.color = guardianShieldColor;
        }

        if (hasMeteorStrike)
        {
            hasAnyAbility = true;
            meteorStrikeSlot.DetailText.text = $"{playerSpecialAbilities.MeteorStrikeDamage:0} dmg";
            meteorStrikeSlot.BackgroundImage.color = meteorStrikeColor;
        }

        if (hasShrapnelMines)
        {
            hasAnyAbility = true;
            shrapnelMinesSlot.DetailText.text = $"{playerSpecialAbilities.ShrapnelMineDamage:0} dmg";
            shrapnelMinesSlot.BackgroundImage.color = shrapnelMinesColor;
        }

        if (hasBloodPact)
        {
            hasAnyAbility = true;
            bloodPactSlot.DetailText.text = $"{Mathf.RoundToInt(playerSpecialAbilities.BloodPactHealChance * 100f)}%";
            bloodPactSlot.BackgroundImage.color = bloodPactColor;
        }

        if (hasTimeFracture)
        {
            hasAnyAbility = true;
            timeFractureSlot.DetailText.text = $"{playerSpecialAbilities.TimeFractureCooldown:0.0}s";
            timeFractureSlot.BackgroundImage.color = timeFractureColor;
        }

        if (hasCryoBlast)
        {
            hasAnyAbility = true;
            cryoBlastSlot.DetailText.text = $"{playerSpecialAbilities.CryoBlastDamage:0} dmg";
            cryoBlastSlot.BackgroundImage.color = cryoBlastColor;
        }

        SetRootVisible(hasAnyAbility || !hidePanelWhenNoAbilities);
    }

    private void SetAllSlotsVisible(bool visible)
    {
        SetSlotVisible(explosiveSlot, visible);
        SetSlotVisible(lightningSlot, visible);
        SetSlotVisible(bladeSlot, visible);
        SetSlotVisible(fireTrailSlot, visible);
        SetSlotVisible(blackHoleSlot, visible);
        SetSlotVisible(droneTurretSlot, visible);
        SetSlotVisible(iceNovaSlot, visible);
        SetSlotVisible(poisonCloudSlot, visible);
        SetSlotVisible(ricochetSlot, visible);
        SetSlotVisible(laserBeamSlot, visible);
        SetSlotVisible(shockwaveSlot, visible);
        SetSlotVisible(guardianShieldSlot, visible);
        SetSlotVisible(meteorStrikeSlot, visible);
        SetSlotVisible(shrapnelMinesSlot, visible);
        SetSlotVisible(bloodPactSlot, visible);
        SetSlotVisible(timeFractureSlot, visible);
        SetSlotVisible(cryoBlastSlot, visible);
    }

    private void SetSlotVisible(AbilitySlot slot, bool visible)
    {
        if (slot == null || slot.RootObject == null)
        {
            return;
        }

        slot.RootObject.SetActive(visible);
    }

    private void SetRootVisible(bool visible)
    {
        if (rootRectTransform == null)
        {
            return;
        }

        rootRectTransform.gameObject.SetActive(visible);
    }
}

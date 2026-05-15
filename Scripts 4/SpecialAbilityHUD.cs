using System.Collections.Generic;
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
    [SerializeField] private Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 0.75f);
    [SerializeField] private Color explosiveColor = new Color(1f, 0.45f, 0.08f, 0.85f);
    [SerializeField] private Color lightningColor = new Color(0.25f, 0.75f, 1f, 0.85f);
    [SerializeField] private Color bladeColor = new Color(0.55f, 0.95f, 1f, 0.85f);
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
    }

    private AbilitySlot CreateAbilitySlot(Transform parent, string iconText, string nameText, Color backgroundColor)
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

    private Text CreateText(Transform parent, string startingText, int fontSize, FontStyle fontStyle)
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
            SetSlotVisible(explosiveSlot, false);
            SetSlotVisible(lightningSlot, false);
            SetSlotVisible(bladeSlot, false);
            SetRootVisible(false);
            return;
        }

        bool hasExplosiveShots = playerSpecialAbilities.ExplosiveShotsUnlocked;
        bool hasLightningStrike = playerSpecialAbilities.LightningStrikeUnlocked;
        bool hasOrbitingBlade = playerSpecialAbilities.OrbitingBladeUnlocked;

        SetSlotVisible(explosiveSlot, hasExplosiveShots);
        SetSlotVisible(lightningSlot, hasLightningStrike);
        SetSlotVisible(bladeSlot, hasOrbitingBlade);

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

        SetRootVisible(hasAnyAbility || !hidePanelWhenNoAbilities);
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
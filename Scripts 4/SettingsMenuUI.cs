using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;

    [Header("Screen Shake UI")]
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Slider screenShakeIntensitySlider;
    [SerializeField] private TMP_Text intensityText;

    [Header("Behavior")]
    [SerializeField] private bool openWithEscape = true;
    [SerializeField] private bool pauseGameWhileOpen = true;

    private bool isOpen;
    private float previousTimeScale = 1f;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettingsMenu);

        if (screenShakeToggle != null)
            screenShakeToggle.onValueChanged.AddListener(OnScreenShakeToggleChanged);

        if (screenShakeIntensitySlider != null)
            screenShakeIntensitySlider.onValueChanged.AddListener(OnIntensitySliderChanged);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        isOpen = false;

        SetupSlider();
        RefreshUIFromSettings();
    }

    private void Update()
    {
        if (!openWithEscape)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsMenu();
        }
    }

    public void ToggleSettingsMenu()
    {
        if (isOpen)
            CloseSettingsMenu();
        else
            OpenSettingsMenu();
    }

    public void OpenSettingsMenu()
    {
        if (settingsPanel == null)
            return;

        RefreshUIFromSettings();

        settingsPanel.SetActive(true);
        isOpen = true;

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void CloseSettingsMenu()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);
        isOpen = false;

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private void SetupSlider()
    {
        if (screenShakeIntensitySlider == null)
            return;

        screenShakeIntensitySlider.minValue = 0f;
        screenShakeIntensitySlider.maxValue = 1f;
        screenShakeIntensitySlider.wholeNumbers = false;
    }

    private void RefreshUIFromSettings()
    {
        if (ScreenShakeSettings.Instance == null)
        {
            Debug.LogWarning("SettingsMenuUI could not find ScreenShakeSettings in the scene.");
            return;
        }

        if (screenShakeToggle != null)
            screenShakeToggle.SetIsOnWithoutNotify(ScreenShakeSettings.Instance.screenShakeEnabled);

        if (screenShakeIntensitySlider != null)
            screenShakeIntensitySlider.SetValueWithoutNotify(ScreenShakeSettings.Instance.intensityMultiplier);

        UpdateIntensityText(ScreenShakeSettings.Instance.intensityMultiplier);
        UpdateSliderInteractable(ScreenShakeSettings.Instance.screenShakeEnabled);
    }

    private void OnScreenShakeToggleChanged(bool enabled)
    {
        if (ScreenShakeSettings.Instance == null)
            return;

        ScreenShakeSettings.Instance.SetScreenShakeEnabled(enabled);
        UpdateSliderInteractable(enabled);
    }

    private void OnIntensitySliderChanged(float value)
    {
        if (ScreenShakeSettings.Instance == null)
            return;

        ScreenShakeSettings.Instance.SetIntensityMultiplier(value);
        UpdateIntensityText(value);
    }

    private void UpdateIntensityText(float value)
    {
        if (intensityText == null)
            return;

        int percent = Mathf.RoundToInt(value * 100f);
        intensityText.text = $"Shake Intensity: {percent}%";
    }

    private void UpdateSliderInteractable(bool screenShakeEnabled)
    {
        if (screenShakeIntensitySlider != null)
            screenShakeIntensitySlider.interactable = screenShakeEnabled;
    }
}
using System;
using UnityEngine;

public class ScreenShakeSettings : MonoBehaviour
{
    public static ScreenShakeSettings Instance { get; private set; }

    public static bool HasInstance
    {
        get { return Instance != null; }
    }

    private const string ScreenShakeEnabledKey = "ScreenShakeEnabled";
    private const string ScreenShakeIntensityKey = "ScreenShakeIntensity";

    [Header("Screen Shake")]
    public bool screenShakeEnabled = true;

    [Range(0f, 1f)]
    public float intensityMultiplier = 1f;

    [Header("Persistence")]
    [SerializeField] private bool saveSettings = true;

    public bool ScreenShakeEnabled
    {
        get { return screenShakeEnabled; }
    }

    public float IntensityMultiplier
    {
        get { return intensityMultiplier; }
    }

    public event Action<bool> OnScreenShakeEnabledChanged;
    public event Action<float> OnIntensityMultiplierChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadSettings();
    }

    public void SetScreenShakeEnabled(bool enabled)
    {
        screenShakeEnabled = enabled;

        if (saveSettings)
        {
            PlayerPrefs.SetInt(ScreenShakeEnabledKey, screenShakeEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        OnScreenShakeEnabledChanged?.Invoke(screenShakeEnabled);
    }

    public void SetIntensityMultiplier(float value)
    {
        intensityMultiplier = Mathf.Clamp(value, 0f, 1f);

        if (saveSettings)
        {
            PlayerPrefs.SetFloat(ScreenShakeIntensityKey, intensityMultiplier);
            PlayerPrefs.Save();
        }

        OnIntensityMultiplierChanged?.Invoke(intensityMultiplier);
    }

    public void ResetToDefaults()
    {
        SetScreenShakeEnabled(true);
        SetIntensityMultiplier(1f);
    }

    private void LoadSettings()
    {
        if (!saveSettings)
            return;

        screenShakeEnabled = PlayerPrefs.GetInt(ScreenShakeEnabledKey, screenShakeEnabled ? 1 : 0) == 1;
        intensityMultiplier = PlayerPrefs.GetFloat(ScreenShakeIntensityKey, intensityMultiplier);

        intensityMultiplier = Mathf.Clamp(intensityMultiplier, 0f, 1f);
    }
}
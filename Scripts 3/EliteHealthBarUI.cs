using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EliteHealthBarUI : MonoBehaviour
{
    public static EliteHealthBarUI Instance { get; private set; }

    public static bool HasInstance
    {
        get { return Instance != null; }
    }

    [Header("UI Root")]
    [SerializeField] private GameObject barRoot;

    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [Header("Legacy Text Optional")]
    [SerializeField] private Text eliteNameText;
    [SerializeField] private Text healthValueText;

    [Header("TextMeshPro Text Optional")]
    [SerializeField] private TMP_Text eliteNameTMPText;
    [SerializeField] private TMP_Text healthValueTMPText;

    [Header("Display")]
    [SerializeField] private string defaultEliteName = "ELITE";
    [SerializeField] private bool hideWhenNoElite = true;
    [SerializeField] private float hideAfterDeathDelay = 0.35f;

    [Header("Colors")]
    [SerializeField] private Color normalFillColor = new Color(1f, 0.25f, 0.1f, 1f);
    [SerializeField] private Color lowHealthFillColor = new Color(1f, 0.85f, 0.05f, 1f);
    [SerializeField] private float lowHealthThreshold = 0.25f;

    private Health trackedHealth;
    private GameObject trackedEliteObject;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        Instance = this;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            healthSlider.interactable = false;
        }

        HideBarInstant();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTrackedHealth();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void TrackElite(GameObject eliteObject, Health eliteHealth, string eliteName)
    {
        if (eliteObject == null || eliteHealth == null)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        UnsubscribeFromTrackedHealth();

        trackedEliteObject = eliteObject;
        trackedHealth = eliteHealth;

        trackedHealth.OnHealthChanged += HandleHealthChanged;
        trackedHealth.OnDied += HandleEliteDied;

        SetEliteName(string.IsNullOrWhiteSpace(eliteName) ? defaultEliteName : eliteName);

        ShowBar();
        UpdateHealthBar(trackedHealth.CurrentHealth, trackedHealth.MaxHealth);
    }

    public void StopTrackingElite(GameObject eliteObject)
    {
        if (trackedEliteObject != eliteObject)
        {
            return;
        }

        UnsubscribeFromTrackedHealth();

        trackedEliteObject = null;
        trackedHealth = null;

        if (hideWhenNoElite)
        {
            HideBarInstant();
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }

    private void HandleEliteDied()
    {
        UpdateHealthBar(0f, trackedHealth != null ? trackedHealth.MaxHealth : 1f);

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

        trackedEliteObject = null;
        trackedHealth = null;

        HideBarInstant();

        hideCoroutine = null;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            maxHealth = 1f;
        }

        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        if (fillImage != null)
        {
            fillImage.color = healthPercent <= lowHealthThreshold ? lowHealthFillColor : normalFillColor;
        }

        string healthText = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";

        if (healthValueText != null)
        {
            healthValueText.text = healthText;
        }

        if (healthValueTMPText != null)
        {
            healthValueTMPText.text = healthText;
        }
    }

    private void SetEliteName(string eliteName)
    {
        if (eliteNameText != null)
        {
            eliteNameText.text = eliteName;
        }

        if (eliteNameTMPText != null)
        {
            eliteNameTMPText.text = eliteName;
        }
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
    }

    private void UnsubscribeFromTrackedHealth()
    {
        if (trackedHealth == null)
        {
            return;
        }

        trackedHealth.OnHealthChanged -= HandleHealthChanged;
        trackedHealth.OnDied -= HandleEliteDied;
    }
}
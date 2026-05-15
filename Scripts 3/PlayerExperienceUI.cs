using UnityEngine;
using UnityEngine.UI;

public class PlayerExperienceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private Text levelText;

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged += UpdateExperienceBar;
            playerExperience.OnLevelChanged += UpdateLevelText;
        }
    }

    private void OnDisable()
    {
        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged -= UpdateExperienceBar;
            playerExperience.OnLevelChanged -= UpdateLevelText;
        }
    }

    private void RefreshUI()
    {
        if (playerExperience == null)
        {
            return;
        }

        UpdateExperienceBar(
            playerExperience.CurrentExperience,
            playerExperience.ExperienceToNextLevel
        );

        UpdateLevelText(playerExperience.CurrentLevel);
    }

    private void UpdateExperienceBar(int currentExperience, int experienceToNextLevel)
    {
        if (experienceSlider == null)
        {
            return;
        }

        experienceSlider.minValue = 0;
        experienceSlider.maxValue = experienceToNextLevel;
        experienceSlider.value = currentExperience;
    }

    private void UpdateLevelText(int currentLevel)
    {
        if (levelText == null)
        {
            return;
        }

        levelText.text = $"Level {currentLevel}";
    }
}
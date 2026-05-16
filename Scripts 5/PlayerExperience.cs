using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int experienceToNextLevel = 10;
    [SerializeField] private float experienceGrowthMultiplier = 1.35f;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;

    public event Action<int, int> OnExperienceChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnLevelUp;

    private void Start()
    {
        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        OnLevelChanged?.Invoke(currentLevel);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
        }

        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
    }

    private void LevelUp()
    {
        currentLevel++;

        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * experienceGrowthMultiplier);

        if (experienceToNextLevel < 1)
        {
            experienceToNextLevel = 1;
        }

        Debug.Log($"LEVEL UP! New Level: {currentLevel}");

        OnLevelChanged?.Invoke(currentLevel);
        OnLevelUp?.Invoke(currentLevel);
    }
}
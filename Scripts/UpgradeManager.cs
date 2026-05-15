using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    private enum UpgradeType
    {
        MoveSpeed,
        FireRate,
        ProjectileDamage,
        ProjectileSpeed,
        ProjectileLifetime,
        ProjectileCount,
        CriticalChance,
        CriticalDamage,
        MaxHealth,
        Heal
    }

    private class UpgradeOption
    {
        public string Title;
        public string Description;
        public UpgradeType Type;
        public float Amount;

        public UpgradeOption(string title, string description, UpgradeType type, float amount)
        {
            Title = title;
            Description = description;
            Type = type;
            Amount = amount;
        }
    }

    [Header("Player References")]
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Health playerHealth;

    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private Text[] upgradeButtonTexts;

    [Header("Pause Settings")]
    [SerializeField] private bool pauseWhileChoosing = true;

    private readonly List<UpgradeOption> allUpgrades = new List<UpgradeOption>();
    private readonly List<UpgradeOption> currentChoices = new List<UpgradeOption>();

    private void Awake()
    {
        AutoFindReferences();
        BuildUpgradePool();
        HideUpgradePanel();
    }

    private void OnEnable()
    {
        if (playerExperience != null)
        {
            playerExperience.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (playerExperience != null)
        {
            playerExperience.OnLevelUp -= HandleLevelUp;
        }
    }

    private void AutoFindReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        if (playerExperience == null)
        {
            playerExperience = playerObject.GetComponent<PlayerExperience>();
        }

        if (playerStats == null)
        {
            playerStats = playerObject.GetComponent<PlayerStats>();
        }

        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponent<Health>();
        }
    }

    private void BuildUpgradePool()
    {
        allUpgrades.Clear();

        allUpgrades.Add(new UpgradeOption(
            "Quick Feet",
            "+0.75 move speed",
            UpgradeType.MoveSpeed,
            0.75f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Trigger Finger",
            "+0.75 fire rate",
            UpgradeType.FireRate,
            0.75f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Heavy Rounds",
            "+5 projectile damage",
            UpgradeType.ProjectileDamage,
            5f
        ));

        allUpgrades.Add(new UpgradeOption(
            "High Velocity",
            "+3 projectile speed",
            UpgradeType.ProjectileSpeed,
            3f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Longshot",
            "+0.75 projectile lifetime",
            UpgradeType.ProjectileLifetime,
            0.75f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Split Shot",
            "+1 projectile",
            UpgradeType.ProjectileCount,
            1f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Deadeye",
            "+5% critical chance",
            UpgradeType.CriticalChance,
            0.05f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Sharpened Rounds",
            "+0.25x critical damage",
            UpgradeType.CriticalDamage,
            0.25f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Toughness",
            "+20 max health",
            UpgradeType.MaxHealth,
            20f
        ));

        allUpgrades.Add(new UpgradeOption(
            "Second Wind",
            "Heal 25 health",
            UpgradeType.Heal,
            25f
        ));
    }

    private void HandleLevelUp(int newLevel)
    {
        ShowUpgradeChoices();
    }

    private void ShowUpgradeChoices()
    {
        if (upgradePanel == null)
        {
            Debug.LogWarning("UpgradeManager is missing the upgrade panel.");
            return;
        }

        if (upgradeButtons == null || upgradeButtons.Length < 3)
        {
            Debug.LogWarning("UpgradeManager needs 3 upgrade buttons.");
            return;
        }

        if (upgradeButtonTexts == null || upgradeButtonTexts.Length < 3)
        {
            Debug.LogWarning("UpgradeManager needs 3 upgrade button text fields.");
            return;
        }

        PickRandomChoices(3);

        upgradePanel.SetActive(true);

        if (pauseWhileChoosing)
        {
            Time.timeScale = 0f;
        }

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int choiceIndex = i;

            upgradeButtons[i].onClick.RemoveAllListeners();

            if (i < currentChoices.Count)
            {
                UpgradeOption option = currentChoices[i];

                upgradeButtons[i].gameObject.SetActive(true);
                upgradeButtonTexts[i].text = $"{option.Title}\n{option.Description}";

                upgradeButtons[i].onClick.AddListener(() =>
                {
                    ChooseUpgrade(choiceIndex);
                });
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void PickRandomChoices(int numberOfChoices)
    {
        currentChoices.Clear();

        List<UpgradeOption> temporaryPool = new List<UpgradeOption>(allUpgrades);

        int choicesToPick = Mathf.Min(numberOfChoices, temporaryPool.Count);

        for (int i = 0; i < choicesToPick; i++)
        {
            int randomIndex = Random.Range(0, temporaryPool.Count);

            currentChoices.Add(temporaryPool[randomIndex]);
            temporaryPool.RemoveAt(randomIndex);
        }
    }

    private void ChooseUpgrade(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            return;
        }

        UpgradeOption chosenUpgrade = currentChoices[choiceIndex];

        ApplyUpgrade(chosenUpgrade);

        HideUpgradePanel();

        if (pauseWhileChoosing)
        {
            Time.timeScale = 1f;
        }
    }

    private void ApplyUpgrade(UpgradeOption upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        switch (upgrade.Type)
        {
            case UpgradeType.MoveSpeed:
                if (playerStats != null)
                {
                    playerStats.IncreaseMoveSpeed(upgrade.Amount);
                }
                break;

            case UpgradeType.FireRate:
                if (playerStats != null)
                {
                    playerStats.IncreaseFireRate(upgrade.Amount);
                }
                break;

            case UpgradeType.ProjectileDamage:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileDamage(upgrade.Amount);
                }
                break;

            case UpgradeType.ProjectileSpeed:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileSpeed(upgrade.Amount);
                }
                break;

            case UpgradeType.ProjectileLifetime:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileLifetime(upgrade.Amount);
                }
                break;

            case UpgradeType.ProjectileCount:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileCount(Mathf.RoundToInt(upgrade.Amount));
                }
                break;

            case UpgradeType.CriticalChance:
                if (playerStats != null)
                {
                    playerStats.IncreaseCriticalChance(upgrade.Amount);
                }
                break;

            case UpgradeType.CriticalDamage:
                if (playerStats != null)
                {
                    playerStats.IncreaseCriticalDamageMultiplier(upgrade.Amount);
                }
                break;

            case UpgradeType.MaxHealth:
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(upgrade.Amount, true);
                }
                break;

            case UpgradeType.Heal:
                if (playerHealth != null)
                {
                    playerHealth.Heal(upgrade.Amount);
                }
                break;
        }

        Debug.Log($"Chosen upgrade: {upgrade.Title}");
    }

    private void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
}
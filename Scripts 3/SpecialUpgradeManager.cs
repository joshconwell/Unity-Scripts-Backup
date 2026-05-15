using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum SpecialRewardTier
{
    Elite,
    MiniBoss,
    Boss
}

public class SpecialUpgradeManager : MonoBehaviour
{
    public static SpecialUpgradeManager Instance { get; private set; }

    public static bool HasInstance
    {
        get { return Instance != null; }
    }

    private enum SpecialUpgradeType
    {
        DamagePercent,
        FireRatePercent,
        MoveSpeedPercent,
        CriticalChance,
        ProjectileSize,
        ProjectileCount,
        ProjectilePierce,
        PickupMagnetPercent,
        MaxHealth,
        Heal,
        HealthPickupHealing,
        HealthPickupDropChance,

        ExplosiveShots,
        LightningStrike
    }

    private class SpecialUpgradeOption
    {
        public string Title;
        public string Description;
        public SpecialUpgradeType Type;
        public float Amount;

        public SpecialUpgradeOption(string title, string description, SpecialUpgradeType type, float amount)
        {
            Title = title;
            Description = description;
            Type = type;
            Amount = amount;
        }
    }

    [Header("Player References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerSpecialAbilities playerSpecialAbilities;

    [Header("Reward UI")]
    [SerializeField] private GameObject rewardPanel;

    [Header("Panel Title - Legacy Text Optional")]
    [SerializeField] private Text panelTitleText;

    [Header("Panel Title - TMP Optional")]
    [SerializeField] private TMP_Text panelTitleTMPText;

    [Header("Reward Buttons")]
    [SerializeField] private Button[] rewardButtons;

    [Header("Reward Button Texts - Legacy Text Optional")]
    [SerializeField] private Text[] rewardButtonTexts;

    [Header("Reward Button Texts - TMP Optional")]
    [SerializeField] private TMP_Text[] rewardButtonTMPTexts;

    [Header("Settings")]
    [SerializeField] private int choicesToShow = 3;
    [SerializeField] private bool pauseGameWhileChoosing = true;

    private readonly List<SpecialUpgradeOption> eliteRewardPool = new List<SpecialUpgradeOption>();
    private readonly List<SpecialUpgradeOption> miniBossRewardPool = new List<SpecialUpgradeOption>();
    private readonly List<SpecialUpgradeOption> bossRewardPool = new List<SpecialUpgradeOption>();

    private readonly List<SpecialUpgradeOption> currentChoices = new List<SpecialUpgradeOption>();

    private UnityAction[] activeButtonActions;
    private float previousTimeScale = 1f;
    private bool rewardPanelOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        AutoFindReferences();
        BuildRewardPools();

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool ShowRewardChoices(SpecialRewardTier rewardTier)
    {
        if (rewardPanelOpen)
        {
            return false;
        }

        AutoFindReferences();

        if (rewardPanel == null)
        {
            Debug.LogWarning("SpecialUpgradeManager is missing the Reward Panel.");
            return false;
        }

        if (rewardButtons == null || rewardButtons.Length == 0)
        {
            Debug.LogWarning("SpecialUpgradeManager is missing reward buttons.");
            return false;
        }

        List<SpecialUpgradeOption> sourcePool = GetRewardPool(rewardTier);

        if (sourcePool == null || sourcePool.Count == 0)
        {
            Debug.LogWarning($"No special rewards found for tier: {rewardTier}");
            return false;
        }

        PickRandomChoices(sourcePool);
        OpenRewardPanel(rewardTier);
        RefreshRewardButtons();

        return true;
    }

    private void BuildRewardPools()
    {
        eliteRewardPool.Clear();
        miniBossRewardPool.Clear();
        bossRewardPool.Clear();

        BuildEliteRewardPool();
        BuildMiniBossRewardPool();
        BuildBossRewardPool();
    }

    private void BuildEliteRewardPool()
    {
        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Refined Powder",
            "+10% projectile damage",
            SpecialUpgradeType.DamagePercent,
            0.10f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Quick Fuse",
            "+10% fire rate",
            SpecialUpgradeType.FireRatePercent,
            0.10f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Lightweight Boots",
            "+8% move speed",
            SpecialUpgradeType.MoveSpeedPercent,
            0.08f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Focused Lens",
            "+3% critical chance",
            SpecialUpgradeType.CriticalChance,
            0.03f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Minor Magnet Core",
            "+15% pickup magnet range",
            SpecialUpgradeType.PickupMagnetPercent,
            0.15f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Larger Casings",
            "+10% projectile size",
            SpecialUpgradeType.ProjectileSize,
            0.10f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "First Aid Kit",
            "Heal 25 health",
            SpecialUpgradeType.Heal,
            25f
        ));

        eliteRewardPool.Add(new SpecialUpgradeOption(
            "Lucky Pouch",
            "+3% health pickup drop chance",
            SpecialUpgradeType.HealthPickupDropChance,
            0.03f
        ));
    }

    private void BuildMiniBossRewardPool()
    {
        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Explosive Shots",
            "Projectiles explode on hit, damaging nearby enemies.",
            SpecialUpgradeType.ExplosiveShots,
            1f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Lightning Strike",
            "Every few seconds, lightning strikes a random nearby enemy.",
            SpecialUpgradeType.LightningStrike,
            1f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Power Core",
            "+25% projectile damage",
            SpecialUpgradeType.DamagePercent,
            0.25f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Accelerator Core",
            "+20% fire rate",
            SpecialUpgradeType.FireRatePercent,
            0.20f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Bullet Storm",
            "+2 projectiles",
            SpecialUpgradeType.ProjectileCount,
            2f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Overcharged Rounds",
            "+2 projectile pierce",
            SpecialUpgradeType.ProjectilePierce,
            2f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Heavy Shells",
            "+35% projectile size",
            SpecialUpgradeType.ProjectileSize,
            0.35f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Hunter's Eye",
            "+10% critical chance",
            SpecialUpgradeType.CriticalChance,
            0.10f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Vital Engine",
            "+50 max health",
            SpecialUpgradeType.MaxHealth,
            50f
        ));

        miniBossRewardPool.Add(new SpecialUpgradeOption(
            "Medical Cache",
            "+40% health pickup healing",
            SpecialUpgradeType.HealthPickupHealing,
            0.40f
        ));
    }

    private void BuildBossRewardPool()
    {
        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Devastation",
            "+40% projectile damage",
            SpecialUpgradeType.DamagePercent,
            0.40f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Barrage",
            "+3 projectiles",
            SpecialUpgradeType.ProjectileCount,
            3f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Drill Rounds",
            "+3 projectile pierce",
            SpecialUpgradeType.ProjectilePierce,
            3f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Giant Rounds",
            "+50% projectile size",
            SpecialUpgradeType.ProjectileSize,
            0.50f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Explosive Payload",
            "Explosive Shots become stronger.",
            SpecialUpgradeType.ExplosiveShots,
            1f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Storm Engine",
            "Lightning Strike becomes stronger.",
            SpecialUpgradeType.LightningStrike,
            1f
        ));

        bossRewardPool.Add(new SpecialUpgradeOption(
            "Boss Core: Vitality",
            "+100 max health",
            SpecialUpgradeType.MaxHealth,
            100f
        ));
    }

    private List<SpecialUpgradeOption> GetRewardPool(SpecialRewardTier rewardTier)
    {
        switch (rewardTier)
        {
            case SpecialRewardTier.Elite:
                return eliteRewardPool;

            case SpecialRewardTier.MiniBoss:
                return miniBossRewardPool;

            case SpecialRewardTier.Boss:
                return bossRewardPool;
        }

        return eliteRewardPool;
    }

    private void PickRandomChoices(List<SpecialUpgradeOption> sourcePool)
    {
        currentChoices.Clear();

        List<SpecialUpgradeOption> temporaryPool = new List<SpecialUpgradeOption>(sourcePool);

        int maxChoices = Mathf.Min(choicesToShow, rewardButtons.Length);
        int choicesToPick = Mathf.Min(maxChoices, temporaryPool.Count);

        for (int i = 0; i < choicesToPick; i++)
        {
            int randomIndex = Random.Range(0, temporaryPool.Count);

            currentChoices.Add(temporaryPool[randomIndex]);
            temporaryPool.RemoveAt(randomIndex);
        }
    }

    private void OpenRewardPanel(SpecialRewardTier rewardTier)
    {
        rewardPanelOpen = true;

        SetPanelTitle(GetPanelTitle(rewardTier));

        rewardPanel.SetActive(true);

        if (pauseGameWhileChoosing)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void CloseRewardPanel()
    {
        ClearButtonActions();

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (pauseGameWhileChoosing)
        {
            Time.timeScale = previousTimeScale;
        }

        rewardPanelOpen = false;
    }

    private void RefreshRewardButtons()
    {
        if (rewardButtons == null)
        {
            return;
        }

        if (activeButtonActions == null || activeButtonActions.Length != rewardButtons.Length)
        {
            activeButtonActions = new UnityAction[rewardButtons.Length];
        }

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            Button button = rewardButtons[i];

            if (button == null)
            {
                continue;
            }

            RemoveButtonAction(i);

            bool hasChoice = i < currentChoices.Count;
            button.gameObject.SetActive(hasChoice);

            if (!hasChoice)
            {
                continue;
            }

            SpecialUpgradeOption option = currentChoices[i];

            SetButtonText(i, $"{option.Title}\n{option.Description}");

            int capturedIndex = i;
            activeButtonActions[i] = () => ChooseReward(capturedIndex);
            button.onClick.AddListener(activeButtonActions[i]);
        }
    }

    private void ChooseReward(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            return;
        }

        SpecialUpgradeOption chosenReward = currentChoices[choiceIndex];

        ApplySpecialUpgrade(chosenReward);

        Debug.Log($"Chosen special reward: {chosenReward.Title}");

        CloseRewardPanel();
    }

    private void ApplySpecialUpgrade(SpecialUpgradeOption upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        AutoFindReferences();

        switch (upgrade.Type)
        {
            case SpecialUpgradeType.DamagePercent:
                if (playerStats != null)
                {
                    float damageIncrease = playerStats.ProjectileDamage * upgrade.Amount;
                    playerStats.IncreaseProjectileDamage(damageIncrease);
                }
                break;

            case SpecialUpgradeType.FireRatePercent:
                if (playerStats != null)
                {
                    float fireRateIncrease = playerStats.FireRate * upgrade.Amount;
                    playerStats.IncreaseFireRate(fireRateIncrease);
                }
                break;

            case SpecialUpgradeType.MoveSpeedPercent:
                if (playerStats != null)
                {
                    float moveSpeedIncrease = playerStats.MoveSpeed * upgrade.Amount;
                    playerStats.IncreaseMoveSpeed(moveSpeedIncrease);
                }
                break;

            case SpecialUpgradeType.CriticalChance:
                if (playerStats != null)
                {
                    playerStats.IncreaseCriticalChance(upgrade.Amount);
                }
                break;

            case SpecialUpgradeType.ProjectileSize:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileSizeMultiplier(upgrade.Amount);
                }
                break;

            case SpecialUpgradeType.ProjectileCount:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectileCount(Mathf.RoundToInt(upgrade.Amount));
                }
                break;

            case SpecialUpgradeType.ProjectilePierce:
                if (playerStats != null)
                {
                    playerStats.IncreaseProjectilePierce(Mathf.RoundToInt(upgrade.Amount));
                }
                break;

            case SpecialUpgradeType.PickupMagnetPercent:
                if (playerStats != null)
                {
                    float magnetIncrease = playerStats.XPMagnetRange * upgrade.Amount;
                    playerStats.IncreaseXPMagnetRange(magnetIncrease);
                }
                break;

            case SpecialUpgradeType.MaxHealth:
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(upgrade.Amount, true);
                }
                break;

            case SpecialUpgradeType.Heal:
                if (playerHealth != null)
                {
                    playerHealth.Heal(upgrade.Amount);
                }
                break;

            case SpecialUpgradeType.HealthPickupHealing:
                if (playerStats != null)
                {
                    playerStats.IncreaseHealthPickupHealMultiplier(upgrade.Amount);
                }
                break;

            case SpecialUpgradeType.HealthPickupDropChance:
                if (playerStats != null)
                {
                    playerStats.IncreaseHealthPickupDropChanceBonus(upgrade.Amount);
                }
                break;

            case SpecialUpgradeType.ExplosiveShots:
                ApplyExplosiveShotsUpgrade();
                break;

            case SpecialUpgradeType.LightningStrike:
                ApplyLightningStrikeUpgrade();
                break;
        }
    }

    private void ApplyExplosiveShotsUpgrade()
    {
        if (playerSpecialAbilities == null)
        {
            Debug.LogWarning("SpecialUpgradeManager could not find PlayerSpecialAbilities.");
            return;
        }

        if (!playerSpecialAbilities.ExplosiveShotsUnlocked)
        {
            playerSpecialAbilities.UnlockExplosiveShots();
            return;
        }

        playerSpecialAbilities.IncreaseExplosiveShotRadius(0.35f);
        playerSpecialAbilities.IncreaseExplosiveShotDamageMultiplier(0.10f);
    }

    private void ApplyLightningStrikeUpgrade()
    {
        if (playerSpecialAbilities == null)
        {
            Debug.LogWarning("SpecialUpgradeManager could not find PlayerSpecialAbilities.");
            return;
        }

        if (!playerSpecialAbilities.LightningStrikeUnlocked)
        {
            playerSpecialAbilities.UnlockLightningStrike();
            return;
        }

        playerSpecialAbilities.IncreaseLightningStrikeDamage(15f);
        playerSpecialAbilities.IncreaseLightningStrikeRange(1f);
        playerSpecialAbilities.ReduceLightningStrikeCooldown(0.35f);
    }

    private void SetPanelTitle(string title)
    {
        if (panelTitleText != null)
        {
            panelTitleText.text = title;
        }

        if (panelTitleTMPText != null)
        {
            panelTitleTMPText.text = title;
        }
    }

    private void SetButtonText(int index, string text)
    {
        if (rewardButtonTexts != null && index < rewardButtonTexts.Length && rewardButtonTexts[index] != null)
        {
            rewardButtonTexts[index].text = text;
        }

        if (rewardButtonTMPTexts != null && index < rewardButtonTMPTexts.Length && rewardButtonTMPTexts[index] != null)
        {
            rewardButtonTMPTexts[index].text = text;
        }
    }

    private string GetPanelTitle(SpecialRewardTier rewardTier)
    {
        switch (rewardTier)
        {
            case SpecialRewardTier.Elite:
                return "ELITE REWARD";

            case SpecialRewardTier.MiniBoss:
                return "MINI-BOSS CHEST";

            case SpecialRewardTier.Boss:
                return "BOSS CHEST";
        }

        return "SPECIAL REWARD";
    }

    private void ClearButtonActions()
    {
        if (rewardButtons == null || activeButtonActions == null)
        {
            return;
        }

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            RemoveButtonAction(i);
        }
    }

    private void RemoveButtonAction(int index)
    {
        if (rewardButtons == null || activeButtonActions == null)
        {
            return;
        }

        if (index < 0 || index >= rewardButtons.Length)
        {
            return;
        }

        if (index >= activeButtonActions.Length)
        {
            return;
        }

        if (rewardButtons[index] == null || activeButtonActions[index] == null)
        {
            return;
        }

        rewardButtons[index].onClick.RemoveListener(activeButtonActions[index]);
        activeButtonActions[index] = null;
    }

    private void AutoFindReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        if (playerStats == null)
        {
            playerStats = playerObject.GetComponent<PlayerStats>();
        }

        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponent<Health>();
        }

        if (playerSpecialAbilities == null)
        {
            playerSpecialAbilities = playerObject.GetComponent<PlayerSpecialAbilities>();
        }
    }
}
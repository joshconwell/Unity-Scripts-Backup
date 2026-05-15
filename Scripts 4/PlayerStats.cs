using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Weapon")]
    [SerializeField] private float fireRate = 4f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 2.5f;

    [Header("Critical Hits")]
    [Range(0f, 1f)]
    [SerializeField] private float criticalChance = 0.05f;
    [SerializeField] private float criticalDamageMultiplier = 2f;

    [Header("Multi-Shot")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private int maxProjectileCount = 12;
    [SerializeField] private float projectileSpreadAngle = 12f;

    [Header("Pierce")]
    [Tooltip("How many extra enemies a projectile can pass through after the first hit. 0 = hits 1 enemy.")]
    [SerializeField] private int projectilePierce = 0;

    [Tooltip("Maximum pierce allowed from upgrades.")]
    [SerializeField] private int maxProjectilePierce = 6;

    [Header("Projectile Size")]
    [Tooltip("1 = normal projectile size. 1.15 = 15% bigger.")]
    [SerializeField] private float projectileSizeMultiplier = 1f;

    [Tooltip("Maximum projectile size multiplier allowed from upgrades.")]
    [SerializeField] private float maxProjectileSizeMultiplier = 2.25f;

    [Header("XP / Pickup")]
    [Tooltip("How far away XP orbs and health pickups start pulling toward the player.")]
    [SerializeField] private float xpMagnetRange = 4.5f;

    [Tooltip("How close XP orbs and health pickups need to be before they are collected.")]
    [SerializeField] private float xpCollectRadius = 0.65f;

    [Tooltip("Multiplier for how fast XP orbs and health pickups move toward the player.")]
    [SerializeField] private float xpMagnetSpeedMultiplier = 1f;

    [Tooltip("Multiplier for XP gained from XP orbs. 1 = normal, 1.15 = 15% more XP.")]
    [SerializeField] private float experienceGainMultiplier = 1f;

    [Header("Health Pickups")]
    [Tooltip("Multiplier for health pickup healing. 1 = normal, 1.25 = 25% more healing.")]
    [SerializeField] private float healthPickupHealMultiplier = 1f;

    [Tooltip("Added drop chance for health pickups. 0.03 = +3% drop chance.")]
    [SerializeField] private float healthPickupDropChanceBonus = 0f;

    [Tooltip("Maximum bonus health pickup drop chance from upgrades.")]
    [SerializeField] private float maxHealthPickupDropChanceBonus = 0.25f;

    public float MoveSpeed => moveSpeed;
    public float FireRate => fireRate;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDamage => projectileDamage;
    public float ProjectileLifetime => projectileLifetime;
    public float CriticalChance => criticalChance;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
    public int ProjectileCount => projectileCount;
    public float ProjectileSpreadAngle => projectileSpreadAngle;
    public int ProjectilePierce => projectilePierce;
    public float ProjectileSizeMultiplier => projectileSizeMultiplier;

    public float XPMagnetRange => xpMagnetRange;
    public float XPCollectRadius => xpCollectRadius;
    public float XPMagnetSpeedMultiplier => xpMagnetSpeedMultiplier;
    public float ExperienceGainMultiplier => experienceGainMultiplier;

    public float HealthPickupHealMultiplier => healthPickupHealMultiplier;
    public float HealthPickupDropChanceBonus => healthPickupDropChanceBonus;

    public event Action OnStatsChanged;

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;

        if (moveSpeed < 0f)
        {
            moveSpeed = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Move Speed increased to {moveSpeed}");
    }

    public void IncreaseFireRate(float amount)
    {
        fireRate += amount;

        if (fireRate < 0.1f)
        {
            fireRate = 0.1f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Fire Rate increased to {fireRate}");
    }

    public void IncreaseProjectileSpeed(float amount)
    {
        projectileSpeed += amount;

        if (projectileSpeed < 0f)
        {
            projectileSpeed = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Speed increased to {projectileSpeed}");
    }

    public void IncreaseProjectileDamage(float amount)
    {
        projectileDamage += amount;

        if (projectileDamage < 0f)
        {
            projectileDamage = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Damage increased to {projectileDamage}");
    }

    public void IncreaseProjectileLifetime(float amount)
    {
        projectileLifetime += amount;

        if (projectileLifetime < 0.1f)
        {
            projectileLifetime = 0.1f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Lifetime increased to {projectileLifetime}");
    }

    public void IncreaseCriticalChance(float amount)
    {
        criticalChance += amount;
        criticalChance = Mathf.Clamp01(criticalChance);

        OnStatsChanged?.Invoke();
        Debug.Log($"Critical Chance increased to {criticalChance * 100f:0}%");
    }

    public void IncreaseCriticalDamageMultiplier(float amount)
    {
        criticalDamageMultiplier += amount;

        if (criticalDamageMultiplier < 1f)
        {
            criticalDamageMultiplier = 1f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Critical Damage Multiplier increased to {criticalDamageMultiplier:0.00}x");
    }

    public void IncreaseProjectileCount(int amount)
    {
        projectileCount += amount;

        if (projectileCount > maxProjectileCount)
        {
            projectileCount = maxProjectileCount;
        }

        if (projectileCount < 1)
        {
            projectileCount = 1;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Count increased to {projectileCount}");
    }

    public void IncreaseProjectilePierce(int amount)
    {
        projectilePierce += amount;

        if (projectilePierce > maxProjectilePierce)
        {
            projectilePierce = maxProjectilePierce;
        }

        if (projectilePierce < 0)
        {
            projectilePierce = 0;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Pierce increased to {projectilePierce}");
    }

    public void IncreaseProjectileSizeMultiplier(float amount)
    {
        projectileSizeMultiplier += amount;

        if (projectileSizeMultiplier > maxProjectileSizeMultiplier)
        {
            projectileSizeMultiplier = maxProjectileSizeMultiplier;
        }

        if (projectileSizeMultiplier < 0.25f)
        {
            projectileSizeMultiplier = 0.25f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Projectile Size Multiplier increased to {projectileSizeMultiplier:0.00}x");
    }

    public void IncreaseXPMagnetRange(float amount)
    {
        xpMagnetRange += amount;

        if (xpMagnetRange < 0f)
        {
            xpMagnetRange = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"XP Magnet Range increased to {xpMagnetRange}");
    }

    public void IncreaseXPCollectRadius(float amount)
    {
        xpCollectRadius += amount;

        if (xpCollectRadius < 0.05f)
        {
            xpCollectRadius = 0.05f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"XP Collect Radius increased to {xpCollectRadius}");
    }

    public void IncreaseXPMagnetSpeedMultiplier(float amount)
    {
        xpMagnetSpeedMultiplier += amount;

        if (xpMagnetSpeedMultiplier < 0f)
        {
            xpMagnetSpeedMultiplier = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"XP Magnet Speed Multiplier increased to {xpMagnetSpeedMultiplier:0.00}x");
    }

    public void IncreaseExperienceGainMultiplier(float amount)
    {
        experienceGainMultiplier += amount;

        if (experienceGainMultiplier < 0.1f)
        {
            experienceGainMultiplier = 0.1f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Experience Gain Multiplier increased to {experienceGainMultiplier:0.00}x");
    }

    public void IncreaseHealthPickupHealMultiplier(float amount)
    {
        healthPickupHealMultiplier += amount;

        if (healthPickupHealMultiplier < 0.1f)
        {
            healthPickupHealMultiplier = 0.1f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Health Pickup Heal Multiplier increased to {healthPickupHealMultiplier:0.00}x");
    }

    public void IncreaseHealthPickupDropChanceBonus(float amount)
    {
        healthPickupDropChanceBonus += amount;

        if (healthPickupDropChanceBonus > maxHealthPickupDropChanceBonus)
        {
            healthPickupDropChanceBonus = maxHealthPickupDropChanceBonus;
        }

        if (healthPickupDropChanceBonus < 0f)
        {
            healthPickupDropChanceBonus = 0f;
        }

        OnStatsChanged?.Invoke();
        Debug.Log($"Health Pickup Drop Chance Bonus increased to {healthPickupDropChanceBonus * 100f:0}%");
    }

    public int GetFinalExperienceAmount(int baseExperienceAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseExperienceAmount * experienceGainMultiplier);

        if (finalAmount < 1)
        {
            finalAmount = 1;
        }

        return finalAmount;
    }

    public float GetFinalHealthPickupHealAmount(float baseHealAmount)
    {
        float finalAmount = baseHealAmount * healthPickupHealMultiplier;

        if (finalAmount < 1f)
        {
            finalAmount = 1f;
        }

        return finalAmount;
    }

    public float GetFinalHealthPickupDropChance(float baseDropChance)
    {
        float finalChance = baseDropChance + healthPickupDropChanceBonus;
        return Mathf.Clamp01(finalChance);
    }
}
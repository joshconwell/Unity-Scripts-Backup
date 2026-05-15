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

    [Header("XP / Pickup")]
    [Tooltip("How far away XP orbs start pulling toward the player.")]
    [SerializeField] private float xpMagnetRange = 4.5f;

    [Tooltip("How close XP orbs need to be before they are collected.")]
    [SerializeField] private float xpCollectRadius = 0.65f;

    [Tooltip("Multiplier for how fast XP orbs move toward the player.")]
    [SerializeField] private float xpMagnetSpeedMultiplier = 1f;

    [Tooltip("Multiplier for XP gained from XP orbs. 1 = normal, 1.15 = 15% more XP.")]
    [SerializeField] private float experienceGainMultiplier = 1f;

    public float MoveSpeed => moveSpeed;
    public float FireRate => fireRate;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDamage => projectileDamage;
    public float ProjectileLifetime => projectileLifetime;
    public float CriticalChance => criticalChance;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
    public int ProjectileCount => projectileCount;
    public float ProjectileSpreadAngle => projectileSpreadAngle;

    public float XPMagnetRange => xpMagnetRange;
    public float XPCollectRadius => xpCollectRadius;
    public float XPMagnetSpeedMultiplier => xpMagnetSpeedMultiplier;
    public float ExperienceGainMultiplier => experienceGainMultiplier;

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

    public int GetFinalExperienceAmount(int baseExperienceAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseExperienceAmount * experienceGainMultiplier);

        if (finalAmount < 1)
        {
            finalAmount = 1;
        }

        return finalAmount;
    }
}
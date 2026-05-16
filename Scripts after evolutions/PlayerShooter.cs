using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    private enum ShootingMode
    {
        ManualMouse,
        AutomaticNearestEnemy,
        HybridAutoOrManual
    }

    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private PlayerSpecialAbilities playerSpecialAbilities;

    [Header("Shooting Mode")]
    [SerializeField] private ShootingMode shootingMode = ShootingMode.AutomaticNearestEnemy;

    [Header("Auto Aim Settings")]
    [SerializeField] private float autoAimRange = 18f;
    [SerializeField] private float targetRefreshInterval = 0.1f;
    [SerializeField] private bool rotatePlayerTowardAutoTarget = true;
    [SerializeField] private bool aimAtMouseWhenNoAutoTarget = true;

    [Header("Projectile Spawn Settings")]
    [SerializeField] private bool useDirectionalSpawnPosition = true;
    [SerializeField] private float projectileSpawnDistance = 0.75f;

    [Header("Fallback Weapon Settings")]
    [SerializeField] private float fallbackFireRate = 4f;
    [SerializeField] private float fallbackProjectileSpeed = 12f;
    [SerializeField] private float fallbackProjectileDamage = 10f;
    [SerializeField] private float fallbackProjectileLifetime = 2.5f;
    [SerializeField] private int fallbackProjectileCount = 1;
    [SerializeField] private float fallbackProjectileSpreadAngle = 12f;
    [SerializeField] private float fallbackCriticalChance = 0.05f;
    [SerializeField] private float fallbackCriticalDamageMultiplier = 2f;
    [SerializeField] private int fallbackProjectilePierce = 0;
    [SerializeField] private float fallbackProjectileSizeMultiplier = 1f;

    private float nextFireTime;
    private float nextTargetRefreshTime;
    private Transform currentTarget;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController2D>();
        }

        if (playerSpecialAbilities == null)
        {
            playerSpecialAbilities = GetComponent<PlayerSpecialAbilities>();
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        RefreshTargetIfNeeded();

        switch (shootingMode)
        {
            case ShootingMode.ManualMouse:
                HandleManualShooting();
                break;

            case ShootingMode.AutomaticNearestEnemy:
                HandleAutomaticShooting();
                break;

            case ShootingMode.HybridAutoOrManual:
                HandleHybridShooting();
                break;
        }
    }

    private void HandleManualShooting()
    {
        if (playerController != null)
        {
            playerController.UseMouseAim();
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 aimDirection = GetMouseAimDirection();
            TryShoot(aimDirection);
        }
    }

    private void HandleAutomaticShooting()
    {
        if (currentTarget == null)
        {
            if (aimAtMouseWhenNoAutoTarget && playerController != null)
            {
                playerController.UseMouseAim();
            }

            return;
        }

        Vector2 aimDirection = GetDirectionToTarget(currentTarget);

        if (rotatePlayerTowardAutoTarget && playerController != null)
        {
            playerController.UseExternalAimDirection(aimDirection);
        }

        TryShoot(aimDirection);
    }

    private void HandleHybridShooting()
    {
        if (currentTarget != null)
        {
            Vector2 aimDirection = GetDirectionToTarget(currentTarget);

            if (rotatePlayerTowardAutoTarget && playerController != null)
            {
                playerController.UseExternalAimDirection(aimDirection);
            }

            TryShoot(aimDirection);
            return;
        }

        if (aimAtMouseWhenNoAutoTarget && playerController != null)
        {
            playerController.UseMouseAim();
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 aimDirection = GetMouseAimDirection();
            TryShoot(aimDirection);
        }
    }

    private void RefreshTargetIfNeeded()
    {
        if (Time.time < nextTargetRefreshTime)
        {
            return;
        }

        currentTarget = FindNearestEnemyInRange();
        nextTargetRefreshTime = Time.time + targetRefreshInterval;
    }

    private Transform FindNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearestEnemy = null;
        float nearestDistanceSquared = autoAimRange * autoAimRange;
        Vector2 playerPosition = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemyObject = enemies[i];

            if (enemyObject == null)
            {
                continue;
            }

            Health enemyHealth = enemyObject.GetComponent<Health>();

            if (enemyHealth != null && enemyHealth.IsDead)
            {
                continue;
            }

            Vector2 enemyPosition = enemyObject.transform.position;
            float distanceSquared = (enemyPosition - playerPosition).sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestEnemy = enemyObject.transform;
            }
        }

        return nearestEnemy;
    }

    private void TryShoot(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float fireRate = GetFireRate();

        if (Time.time < nextFireTime)
        {
            return;
        }

        Shoot(aimDirection.normalized);

        nextFireTime = Time.time + 1f / fireRate;
    }

    private void Shoot(Vector2 aimDirection)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerShooter is missing a projectile prefab.");
            return;
        }

        int projectileCount = GetProjectileCount();
        float spreadAngle = GetProjectileSpreadAngle();

        if (projectileCount <= 1)
        {
            SpawnProjectile(aimDirection);
            return;
        }

        float totalSpread = spreadAngle * (projectileCount - 1);
        float startingAngle = -totalSpread / 2f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = startingAngle + spreadAngle * i;
            Vector2 shotDirection = RotateVector(aimDirection, angleOffset);
            SpawnProjectile(shotDirection);
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        Vector3 spawnPosition = GetProjectileSpawnPosition(direction);

        GameObject projectileObject = null;

        if (PoolManager.HasInstance)
        {
            projectileObject = PoolManager.Instance.Spawn(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (projectileObject == null)
        {
            projectileObject = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            float baseDamage = GetProjectileDamage();
            bool isCriticalHit = RollCriticalHit();

            float finalDamage = baseDamage;

            if (isCriticalHit)
            {
                finalDamage *= GetCriticalDamageMultiplier();
            }

            projectile.Launch(
                direction,
                GetProjectileSpeed(),
                finalDamage,
                GetProjectileLifetime(),
                isCriticalHit,
                GetProjectilePierce(),
                GetProjectileSizeMultiplier(),
                playerSpecialAbilities
            );
        }
    }

    private bool RollCriticalHit()
    {
        float criticalChance = GetCriticalChance();
        return Random.value <= criticalChance;
    }

    private Vector3 GetProjectileSpawnPosition(Vector2 direction)
    {
        if (useDirectionalSpawnPosition)
        {
            Vector3 directionalOffset = new Vector3(direction.x, direction.y, 0f) * projectileSpawnDistance;
            return transform.position + directionalOffset;
        }

        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position;
    }

    private Vector2 GetMouseAimDirection()
    {
        if (mainCamera == null)
        {
            return transform.right;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 playerPosition = transform.position;
        Vector2 targetPosition = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        Vector2 aimDirection = targetPosition - playerPosition;

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            aimDirection = transform.right;
        }

        return aimDirection.normalized;
    }

    private Vector2 GetDirectionToTarget(Transform target)
    {
        if (target == null)
        {
            return transform.right;
        }

        Vector2 playerPosition = transform.position;
        Vector2 targetPosition = target.position;

        Vector2 direction = targetPosition - playerPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return transform.right;
        }

        return direction.normalized;
    }

    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float rotatedX = vector.x * cos - vector.y * sin;
        float rotatedY = vector.x * sin + vector.y * cos;

        return new Vector2(rotatedX, rotatedY).normalized;
    }

    private float GetFireRate()
    {
        if (playerStats != null)
        {
            return Mathf.Max(0.1f, playerStats.FireRate);
        }

        return Mathf.Max(0.1f, fallbackFireRate);
    }

    private float GetProjectileSpeed()
    {
        if (playerStats != null)
        {
            return playerStats.ProjectileSpeed;
        }

        return fallbackProjectileSpeed;
    }

    private float GetProjectileDamage()
    {
        if (playerStats != null)
        {
            return playerStats.ProjectileDamage;
        }

        return fallbackProjectileDamage;
    }

    private float GetProjectileLifetime()
    {
        if (playerStats != null)
        {
            return playerStats.ProjectileLifetime;
        }

        return fallbackProjectileLifetime;
    }

    private int GetProjectileCount()
    {
        if (playerStats != null)
        {
            return Mathf.Max(1, playerStats.ProjectileCount);
        }

        return Mathf.Max(1, fallbackProjectileCount);
    }

    private float GetProjectileSpreadAngle()
    {
        if (playerStats != null)
        {
            return Mathf.Max(0f, playerStats.ProjectileSpreadAngle);
        }

        return Mathf.Max(0f, fallbackProjectileSpreadAngle);
    }

    private float GetCriticalChance()
    {
        if (playerStats != null)
        {
            return Mathf.Clamp01(playerStats.CriticalChance);
        }

        return Mathf.Clamp01(fallbackCriticalChance);
    }

    private float GetCriticalDamageMultiplier()
    {
        if (playerStats != null)
        {
            return Mathf.Max(1f, playerStats.CriticalDamageMultiplier);
        }

        return Mathf.Max(1f, fallbackCriticalDamageMultiplier);
    }

    private int GetProjectilePierce()
    {
        if (playerStats != null)
        {
            return Mathf.Max(0, playerStats.ProjectilePierce);
        }

        return Mathf.Max(0, fallbackProjectilePierce);
    }

    private float GetProjectileSizeMultiplier()
    {
        if (playerStats != null)
        {
            return Mathf.Max(0.25f, playerStats.ProjectileSizeMultiplier);
        }

        return Mathf.Max(0.25f, fallbackProjectileSizeMultiplier);
    }
}
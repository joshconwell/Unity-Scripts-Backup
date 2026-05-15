using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Shooting")]
    [SerializeField] private float shootRange = 12f;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileLifetime = 4f;

    [Header("Accuracy")]
    [SerializeField] private float aimErrorDegrees = 4f;

    [Header("Burst Settings")]
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float spreadAngle = 12f;

    private float nextShootTime;

    private void Start()
    {
        FindTargetIfNeeded();
    }

    private void OnEnable()
    {
        FindTargetIfNeeded();
        nextShootTime = Time.time + Random.Range(0.25f, shootInterval);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (target == null)
        {
            FindTargetIfNeeded();

            if (target == null)
            {
                return;
            }
        }

        if (!IsTargetInRange())
        {
            return;
        }

        if (Time.time < nextShootTime)
        {
            return;
        }

        ShootAtTarget();
        nextShootTime = Time.time + shootInterval;
    }

    private void FindTargetIfNeeded()
    {
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private bool IsTargetInRange()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        return distanceToTarget <= shootRange;
    }

    private void ShootAtTarget()
    {
        if (enemyProjectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing an enemy projectile prefab.");
            return;
        }

        Vector2 directionToTarget = GetDirectionToTarget();

        int safeProjectileCount = Mathf.Max(1, projectilesPerShot);

        if (safeProjectileCount == 1)
        {
            Vector2 shotDirection = RotateVector(directionToTarget, Random.Range(-aimErrorDegrees, aimErrorDegrees));
            SpawnProjectile(shotDirection);
            return;
        }

        float totalSpread = spreadAngle * (safeProjectileCount - 1);
        float startingAngle = -totalSpread / 2f;

        for (int i = 0; i < safeProjectileCount; i++)
        {
            float spreadOffset = startingAngle + spreadAngle * i;
            float randomError = Random.Range(-aimErrorDegrees, aimErrorDegrees);
            Vector2 shotDirection = RotateVector(directionToTarget, spreadOffset + randomError);

            SpawnProjectile(shotDirection);
        }
    }

    private Vector2 GetDirectionToTarget()
    {
        Vector2 shooterPosition = transform.position;
        Vector2 targetPosition = target.position;

        Vector2 direction = targetPosition - shooterPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return transform.right;
        }

        return direction.normalized;
    }

    private void SpawnProjectile(Vector2 direction)
    {
        Vector3 spawnPosition = GetFirePosition(direction);

        GameObject projectileObject = null;

        if (PoolManager.HasInstance)
        {
            projectileObject = PoolManager.Instance.Spawn(
                enemyProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (projectileObject == null)
        {
            projectileObject = Instantiate(
                enemyProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        EnemyProjectile enemyProjectile = projectileObject.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
        {
            enemyProjectile.Launch(
                direction,
                projectileSpeed,
                projectileDamage,
                projectileLifetime
            );
        }
    }

    private Vector3 GetFirePosition(Vector2 direction)
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        Vector3 offset = new Vector3(direction.x, direction.y, 0f) * 0.7f;
        return transform.position + offset;
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
}
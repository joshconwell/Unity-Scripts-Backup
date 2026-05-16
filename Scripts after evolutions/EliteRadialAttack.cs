using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EliteRadialAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float activationRange = 18f;

    [Header("Attack Timing")]
    [SerializeField] private float firstAttackDelay = 2.5f;
    [SerializeField] private float attackInterval = 4.5f;
    [SerializeField] private float warningDuration = 0.6f;

    [Header("Radial Pattern")]
    [SerializeField] private int projectilesPerRing = 12;
    [SerializeField] private int ringsPerAttack = 1;
    [SerializeField] private float delayBetweenRings = 0.15f;
    [SerializeField] private float angleOffsetPerRing = 15f;
    [SerializeField] private bool randomizeStartingAngle = true;

    [Header("Projectile Stats")]
    [SerializeField] private float projectileSpeed = 5.5f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Warning Flash")]
    [SerializeField] private bool flashBeforeAttack = true;
    [SerializeField] private Color warningColor = Color.white;
    [SerializeField] private float flashInterval = 0.08f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugAttackKey = true;
    [SerializeField] private KeyCode debugAttackKey = KeyCode.F7;

    private Health health;
    private Transform player;
    private Coroutine attackRoutine;

    private Color[] originalColors;
    private bool isAttacking;
    private bool isDead;

    private void Awake()
    {
        health = GetComponent<Health>();
        AutoFindSpriteRenderers();
        CacheOriginalColors();
    }

    private void OnEnable()
    {
        isDead = false;
        isAttacking = false;

        FindPlayerIfNeeded();
        RestoreOriginalColors();

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.OnDied += HandleDied;
        }

        attackRoutine = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }

        StopAttackRoutine();
        RestoreOriginalColors();
    }

    private void Update()
    {
        if (allowDebugAttackKey && Input.GetKeyDown(debugAttackKey))
        {
            StartCoroutine(PerformRadialAttack());
        }
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(firstAttackDelay);

        while (!isDead)
        {
            FindPlayerIfNeeded();

            if (player != null && IsPlayerInRange())
            {
                yield return PerformRadialAttack();
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator PerformRadialAttack()
    {
        if (isAttacking || isDead)
        {
            yield break;
        }

        if (enemyProjectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing an enemy projectile prefab.");
            yield break;
        }

        isAttacking = true;

        if (warningDuration > 0f)
        {
            yield return WarningFlashRoutine();
        }

        float startingAngle = randomizeStartingAngle ? Random.Range(0f, 360f) : 0f;

        for (int ringIndex = 0; ringIndex < ringsPerAttack; ringIndex++)
        {
            float ringAngleOffset = startingAngle + angleOffsetPerRing * ringIndex;
            FireRing(ringAngleOffset);

            if (ringIndex < ringsPerAttack - 1 && delayBetweenRings > 0f)
            {
                yield return new WaitForSeconds(delayBetweenRings);
            }
        }

        isAttacking = false;
    }

    private IEnumerator WarningFlashRoutine()
    {
        if (!flashBeforeAttack || spriteRenderers == null || spriteRenderers.Length == 0)
        {
            yield return new WaitForSeconds(warningDuration);
            yield break;
        }

        float elapsedTime = 0f;
        bool useWarningColor = true;

        while (elapsedTime < warningDuration)
        {
            SetRendererColors(useWarningColor ? warningColor : Color.clear, useWarningColor);
            useWarningColor = !useWarningColor;

            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        RestoreOriginalColors();
    }

    private void FireRing(float startingAngle)
    {
        int safeProjectileCount = Mathf.Max(1, projectilesPerRing);
        float angleStep = 360f / safeProjectileCount;

        for (int i = 0; i < safeProjectileCount; i++)
        {
            float angle = startingAngle + angleStep * i;
            Vector2 direction = AngleToDirection(angle);

            SpawnProjectile(direction);
        }
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

        Vector3 offset = new Vector3(direction.x, direction.y, 0f) * 0.75f;
        return transform.position + offset;
    }

    private Vector2 AngleToDirection(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        ).normalized;
    }

    private bool IsPlayerInRange()
    {
        if (player == null)
        {
            return false;
        }

        float distanceSquared = (player.position - transform.position).sqrMagnitude;
        return distanceSquared <= activationRange * activationRange;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void HandleDied()
    {
        isDead = true;
        isAttacking = false;

        StopAttackRoutine();
        RestoreOriginalColors();
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private void AutoFindSpriteRenderers()
    {
        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            return;
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void CacheOriginalColors()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
    }

    private void SetRendererColors(Color color, bool useOverrideColor)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            if (useOverrideColor)
            {
                spriteRenderers[i].color = color;
            }
            else if (originalColors != null && i < originalColors.Length)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < originalColors.Length)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
}
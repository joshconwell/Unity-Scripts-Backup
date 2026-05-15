using System.Collections.Generic;
using UnityEngine;

public class PlayerSpecialAbilities : MonoBehaviour
{
    [Header("Explosive Shots")]
    [SerializeField] private bool explosiveShotsUnlocked = false;

    [Tooltip("Radius around the hit enemy that takes explosion damage.")]
    [SerializeField] private float explosiveShotRadius = 2.25f;

    [Tooltip("Explosion damage as a percentage of projectile damage. 0.4 = 40%.")]
    [SerializeField] private float explosiveShotDamageMultiplier = 0.4f;

    [Tooltip("If true, explosive damage can also be marked as critical when the original projectile was critical.")]
    [SerializeField] private bool explosionsCanCrit = false;

    [Tooltip("Layer mask used for explosion damage. Leave as Everything if you are unsure.")]
    [SerializeField] private LayerMask explosionHitMask = ~0;

    [Header("Explosion Visual")]
    [SerializeField] private GameObject explosionVisualPrefab;
    [SerializeField] private bool spawnExplosionVisual = true;
    [SerializeField] private float explosionVisualDuration = 0.22f;
    [SerializeField] private Color explosionVisualStartColor = new Color(1f, 0.65f, 0.1f, 0.75f);
    [SerializeField] private Color explosionVisualEndColor = new Color(1f, 0.1f, 0f, 0f);

    [Header("Lightning Strike")]
    [SerializeField] private bool lightningStrikeUnlocked = false;

    [Tooltip("How often lightning strikes while unlocked.")]
    [SerializeField] private float lightningStrikeCooldown = 4f;

    [Tooltip("How far from the player lightning can target enemies.")]
    [SerializeField] private float lightningStrikeRange = 12f;

    [Tooltip("Damage dealt by each lightning strike.")]
    [SerializeField] private float lightningStrikeDamage = 35f;

    [Tooltip("How many enemies are struck each time lightning activates.")]
    [SerializeField] private int lightningStrikesPerActivation = 1;

    [Tooltip("Layer mask used for lightning targeting. Leave as Everything if unsure.")]
    [SerializeField] private LayerMask lightningHitMask = ~0;

    [Header("Lightning Visual")]
    [SerializeField] private GameObject lightningStrikeVisualPrefab;
    [SerializeField] private bool spawnLightningVisual = true;
    [SerializeField] private float lightningVisualBoltLength = 3.5f;
    [SerializeField] private float lightningVisualDuration = 0.16f;
    [SerializeField] private float lightningVisualWidth = 0.14f;
    [SerializeField] private int lightningVisualSegments = 6;
    [SerializeField] private float lightningVisualJaggedness = 0.35f;
    [SerializeField] private Color lightningVisualStartColor = new Color(0.4f, 0.9f, 1f, 1f);
    [SerializeField] private Color lightningVisualEndColor = new Color(1f, 1f, 1f, 0f);

    [Header("Debug")]
    [SerializeField] private bool drawExplosionDebugCircle = false;
    [SerializeField] private float debugCircleDuration = 0.2f;

    private float lightningTimer;

    private readonly List<Health> lightningTargets = new List<Health>();
    private readonly HashSet<Health> lightningTargetSet = new HashSet<Health>();

    public bool ExplosiveShotsUnlocked => explosiveShotsUnlocked;
    public float ExplosiveShotRadius => explosiveShotRadius;
    public float ExplosiveShotDamageMultiplier => explosiveShotDamageMultiplier;

    public bool LightningStrikeUnlocked => lightningStrikeUnlocked;
    public float LightningStrikeCooldown => lightningStrikeCooldown;
    public float LightningStrikeRange => lightningStrikeRange;
    public float LightningStrikeDamage => lightningStrikeDamage;
    public int LightningStrikesPerActivation => lightningStrikesPerActivation;

    private void OnEnable()
    {
        lightningTimer = lightningStrikeCooldown;
    }

    private void Update()
    {
        HandleLightningStrikeTimer();
    }

    private void HandleLightningStrikeTimer()
    {
        if (!lightningStrikeUnlocked)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        lightningTimer -= Time.deltaTime;

        if (lightningTimer > 0f)
        {
            return;
        }

        PerformLightningStrike();
        lightningTimer = lightningStrikeCooldown;
    }

    public void UnlockExplosiveShots()
    {
        explosiveShotsUnlocked = true;

        Debug.Log("Special Ability Unlocked: Explosive Shots");
    }

    public void IncreaseExplosiveShotRadius(float amount)
    {
        explosiveShotRadius += amount;

        if (explosiveShotRadius < 0.25f)
        {
            explosiveShotRadius = 0.25f;
        }

        Debug.Log($"Explosive Shot Radius increased to {explosiveShotRadius:0.00}");
    }

    public void IncreaseExplosiveShotDamageMultiplier(float amount)
    {
        explosiveShotDamageMultiplier += amount;

        if (explosiveShotDamageMultiplier < 0.05f)
        {
            explosiveShotDamageMultiplier = 0.05f;
        }

        Debug.Log($"Explosive Shot Damage Multiplier increased to {explosiveShotDamageMultiplier:0.00}");
    }

    public void UnlockLightningStrike()
    {
        lightningStrikeUnlocked = true;
        lightningTimer = 0.25f;

        Debug.Log("Special Ability Unlocked: Lightning Strike");
    }

    public void IncreaseLightningStrikeDamage(float amount)
    {
        lightningStrikeDamage += amount;

        if (lightningStrikeDamage < 1f)
        {
            lightningStrikeDamage = 1f;
        }

        Debug.Log($"Lightning Strike Damage increased to {lightningStrikeDamage:0}");
    }

    public void IncreaseLightningStrikeRange(float amount)
    {
        lightningStrikeRange += amount;

        if (lightningStrikeRange < 1f)
        {
            lightningStrikeRange = 1f;
        }

        Debug.Log($"Lightning Strike Range increased to {lightningStrikeRange:0.00}");
    }

    public void ReduceLightningStrikeCooldown(float amount)
    {
        lightningStrikeCooldown -= amount;

        if (lightningStrikeCooldown < 1f)
        {
            lightningStrikeCooldown = 1f;
        }

        Debug.Log($"Lightning Strike Cooldown reduced to {lightningStrikeCooldown:0.00}");
    }

    public void IncreaseLightningStrikesPerActivation(int amount)
    {
        lightningStrikesPerActivation += amount;

        if (lightningStrikesPerActivation < 1)
        {
            lightningStrikesPerActivation = 1;
        }

        Debug.Log($"Lightning Strikes Per Activation increased to {lightningStrikesPerActivation}");
    }

    public void TriggerExplosiveShot(Vector3 explosionPosition, float projectileDamage, Health primaryTarget, bool originalHitWasCritical)
    {
        if (!explosiveShotsUnlocked)
        {
            return;
        }

        float explosionDamage = projectileDamage * explosiveShotDamageMultiplier;

        if (explosionDamage <= 0f)
        {
            return;
        }

        SpawnExplosionVisual(explosionPosition);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            explosionPosition,
            explosiveShotRadius,
            explosionHitMask
        );

        HashSet<Health> damagedEnemies = new HashSet<Health>();

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!hitCollider.CompareTag("Enemy"))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null)
            {
                continue;
            }

            if (enemyHealth == primaryTarget)
            {
                continue;
            }

            if (damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            damagedEnemies.Add(enemyHealth);

            bool explosionIsCritical = explosionsCanCrit && originalHitWasCritical;
            enemyHealth.TakeDamage(explosionDamage, explosionIsCritical);
        }

        if (drawExplosionDebugCircle)
        {
            DrawDebugCircle(explosionPosition, explosiveShotRadius, debugCircleDuration);
        }
    }

    private void PerformLightningStrike()
    {
        FindLightningTargets();

        if (lightningTargets.Count == 0)
        {
            return;
        }

        int strikesToPerform = Mathf.Max(1, lightningStrikesPerActivation);

        for (int i = 0; i < strikesToPerform; i++)
        {
            if (lightningTargets.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, lightningTargets.Count);
            Health targetHealth = lightningTargets[randomIndex];
            lightningTargets.RemoveAt(randomIndex);

            if (targetHealth == null)
            {
                continue;
            }

            targetHealth.TakeDamage(lightningStrikeDamage, false);
            SpawnLightningVisual(targetHealth.transform.position);
        }
    }

    private void FindLightningTargets()
    {
        lightningTargets.Clear();
        lightningTargetSet.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            lightningStrikeRange,
            lightningHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!hitCollider.CompareTag("Enemy"))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null)
            {
                continue;
            }

            if (enemyHealth.IsDead)
            {
                continue;
            }

            if (lightningTargetSet.Contains(enemyHealth))
            {
                continue;
            }

            lightningTargetSet.Add(enemyHealth);
            lightningTargets.Add(enemyHealth);
        }
    }

    private void SpawnExplosionVisual(Vector3 explosionPosition)
    {
        if (!spawnExplosionVisual)
        {
            return;
        }

        if (explosionVisualPrefab == null)
        {
            return;
        }

        GameObject visualObject = null;

        if (PoolManager.HasInstance)
        {
            visualObject = PoolManager.Instance.Spawn(
                explosionVisualPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            visualObject = Instantiate(
                explosionVisualPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            return;
        }

        ExplosionVisual explosionVisual = visualObject.GetComponent<ExplosionVisual>();

        if (explosionVisual != null)
        {
            explosionVisual.Play(
                explosiveShotRadius,
                explosionVisualDuration,
                explosionVisualStartColor,
                explosionVisualEndColor
            );
        }
    }

    private void SpawnLightningVisual(Vector3 strikePosition)
    {
        if (!spawnLightningVisual)
        {
            return;
        }

        if (lightningStrikeVisualPrefab == null)
        {
            return;
        }

        GameObject visualObject = null;

        if (PoolManager.HasInstance)
        {
            visualObject = PoolManager.Instance.Spawn(
                lightningStrikeVisualPrefab,
                strikePosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            visualObject = Instantiate(
                lightningStrikeVisualPrefab,
                strikePosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            return;
        }

        LightningStrikeVisual lightningVisual = visualObject.GetComponent<LightningStrikeVisual>();

        if (lightningVisual != null)
        {
            lightningVisual.Play(
                strikePosition,
                lightningVisualBoltLength,
                lightningVisualDuration,
                lightningVisualStartColor,
                lightningVisualEndColor,
                lightningVisualWidth,
                lightningVisualSegments,
                lightningVisualJaggedness
            );
        }
    }

    private void DrawDebugCircle(Vector3 center, float radius, float duration)
    {
        int segments = 32;
        float angleStep = 360f / segments;

        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            Debug.DrawLine(previousPoint, nextPoint, Color.yellow, duration);
            previousPoint = nextPoint;
        }
    }
}
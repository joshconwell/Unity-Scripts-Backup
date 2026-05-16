using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitingBlade : MonoBehaviour
{
    [Header("Runtime Debug")]
    [SerializeField] private bool drawHitRadius = false;

    private Transform target;
    private int orbitIndex;
    private int orbitCount = 1;

    private float orbitRadius = 2.25f;
    private float orbitSpeedDegreesPerSecond = 180f;
    private float damage = 18f;
    private float hitRadius = 0.55f;
    private float hitCooldown = 0.35f;
    private LayerMask enemyHitMask = ~0;

    private LineRenderer lineRenderer;

    private readonly Dictionary<Health, float> nextAllowedHitTimes = new Dictionary<Health, float>();
    private readonly List<Health> staleEnemies = new List<Health>();

    private void Awake()
    {
        SetupDefaultVisual();
    }

    private void OnEnable()
    {
        nextAllowedHitTimes.Clear();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        UpdateOrbitPosition();

        if (Time.timeScale <= 0f)
        {
            return;
        }

        DamageEnemiesInsideBlade();
        CleanOldHitEntries();
    }

    public void Initialize(
        Transform newTarget,
        int newOrbitIndex,
        int newOrbitCount,
        float newOrbitRadius,
        float newOrbitSpeedDegreesPerSecond,
        float newDamage,
        float newHitRadius,
        float newHitCooldown,
        LayerMask newEnemyHitMask)
    {
        target = newTarget;
        orbitIndex = Mathf.Max(0, newOrbitIndex);
        orbitCount = Mathf.Max(1, newOrbitCount);

        orbitRadius = Mathf.Max(0.25f, newOrbitRadius);
        orbitSpeedDegreesPerSecond = newOrbitSpeedDegreesPerSecond;
        damage = Mathf.Max(0f, newDamage);
        hitRadius = Mathf.Max(0.05f, newHitRadius);
        hitCooldown = Mathf.Max(0.05f, newHitCooldown);
        enemyHitMask = newEnemyHitMask;

        UpdateOrbitPosition();
    }

    private void UpdateOrbitPosition()
    {
        float spacingAngle = 360f / Mathf.Max(1, orbitCount);
        float angle = (Time.time * orbitSpeedDegreesPerSecond) + (spacingAngle * orbitIndex);
        float radians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            Mathf.Sin(radians) * orbitRadius,
            0f
        );

        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }

    private void DamageEnemiesInsideBlade()
    {
        if (damage <= 0f)
        {
            return;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            hitRadius,
            enemyHitMask
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

            if (nextAllowedHitTimes.TryGetValue(enemyHealth, out float nextAllowedTime))
            {
                if (Time.time < nextAllowedTime)
                {
                    continue;
                }
            }

            enemyHealth.TakeDamage(damage, false);
            nextAllowedHitTimes[enemyHealth] = Time.time + hitCooldown;
        }
    }

    private void CleanOldHitEntries()
    {
        staleEnemies.Clear();

        foreach (KeyValuePair<Health, float> entry in nextAllowedHitTimes)
        {
            Health enemyHealth = entry.Key;

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                staleEnemies.Add(enemyHealth);
                continue;
            }

            if (Time.time > entry.Value + 3f)
            {
                staleEnemies.Add(enemyHealth);
            }
        }

        for (int i = 0; i < staleEnemies.Count; i++)
        {
            nextAllowedHitTimes.Remove(staleEnemies[i]);
        }
    }

    private void SetupDefaultVisual()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.startColor = new Color(0.75f, 0.95f, 1f, 1f);
        lineRenderer.endColor = new Color(0.35f, 0.75f, 1f, 1f);

        if (lineRenderer.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                lineRenderer.sharedMaterial = new Material(spriteShader);
            }
        }

        lineRenderer.SetPosition(0, new Vector3(0f, 0.45f, 0f));
        lineRenderer.SetPosition(1, new Vector3(0.25f, 0f, 0f));
        lineRenderer.SetPosition(2, new Vector3(0f, -0.45f, 0f));
        lineRenderer.SetPosition(3, new Vector3(-0.25f, 0f, 0f));
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawHitRadius)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
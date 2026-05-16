using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FireTrailZone : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool drawDamageRadius = false;

    private float radius = 1.15f;
    private float damagePerTick = 8f;
    private float tickInterval = 0.35f;
    private float lifetime = 3f;
    private LayerMask enemyHitMask = ~0;

    private Color startColor = new Color(1f, 0.38f, 0.05f, 0.85f);
    private Color endColor = new Color(1f, 0.05f, 0f, 0f);
    private float lineWidth = 0.18f;
    private int circleSegments = 48;

    private float lifeTimer;
    private float tickTimer;
    private bool initialized;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    private readonly HashSet<Health> damagedThisTick = new HashSet<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        tickTimer = 0f;
        damagedThisTick.Clear();
    }

    private void OnDisable()
    {
        initialized = false;
        damagedThisTick.Clear();
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
            lineMaterial = null;
        }
    }

    public void Initialize(
        Vector3 position,
        float newRadius,
        float newDamagePerTick,
        float newTickInterval,
        float newLifetime,
        LayerMask newEnemyHitMask,
        Color newStartColor,
        Color newEndColor,
        float newLineWidth,
        int newCircleSegments)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        radius = Mathf.Max(0.1f, newRadius);
        damagePerTick = Mathf.Max(0f, newDamagePerTick);
        tickInterval = Mathf.Max(0.05f, newTickInterval);
        lifetime = Mathf.Max(0.1f, newLifetime);
        enemyHitMask = newEnemyHitMask;

        startColor = newStartColor;
        endColor = newEndColor;
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        circleSegments = Mathf.Max(12, newCircleSegments);

        lifeTimer = 0f;
        tickTimer = 0f;
        initialized = true;

        SetupLineRenderer();
        UpdateVisual(0f);
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        lifeTimer += Time.deltaTime;
        tickTimer -= Time.deltaTime;

        float lifetimePercent = Mathf.Clamp01(lifeTimer / lifetime);
        UpdateVisual(lifetimePercent);

        if (tickTimer <= 0f)
        {
            DamageEnemiesInsideZone();
            tickTimer = tickInterval;
        }

        if (lifeTimer >= lifetime)
        {
            ReturnOrDestroy();
        }
    }

    private void DamageEnemiesInsideZone()
    {
        if (damagePerTick <= 0f)
        {
            return;
        }

        damagedThisTick.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!IsEnemyCollider(hitCollider))
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

            if (damagedThisTick.Contains(enemyHealth))
            {
                continue;
            }

            damagedThisTick.Add(enemyHealth);
            enemyHealth.TakeDamage(damagePerTick, false);
        }
    }

    private bool IsEnemyCollider(Collider2D hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }

        Transform currentTransform = hitCollider.transform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag("Enemy"))
            {
                return true;
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = Mathf.Max(12, circleSegments);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.sortingOrder = 35;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                lineMaterial = new Material(spriteShader);
                lineRenderer.sharedMaterial = lineMaterial;
            }
        }

        UpdateCirclePoints(radius);
    }

    private void UpdateVisual(float lifetimePercent)
    {
        if (lineRenderer == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * 14f) * 0.035f;
        float currentRadius = radius * pulse;

        Color currentColor = Color.Lerp(startColor, endColor, lifetimePercent);

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        float currentWidth = Mathf.Lerp(lineWidth, lineWidth * 0.35f, lifetimePercent);
        lineRenderer.startWidth = currentWidth;
        lineRenderer.endWidth = currentWidth;

        UpdateCirclePoints(currentRadius);
    }

    private void UpdateCirclePoints(float currentRadius)
    {
        if (lineRenderer == null)
        {
            return;
        }

        int safeSegments = Mathf.Max(12, circleSegments);

        if (lineRenderer.positionCount != safeSegments)
        {
            lineRenderer.positionCount = safeSegments;
        }

        for (int i = 0; i < safeSegments; i++)
        {
            float angle = ((float)i / safeSegments) * Mathf.PI * 2f;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                Mathf.Sin(angle) * currentRadius,
                0f
            );

            lineRenderer.SetPosition(i, point);
        }
    }

    private void ReturnOrDestroy()
    {
        initialized = false;

        PooledObject pooledObject = GetComponent<PooledObject>();

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDamageRadius)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
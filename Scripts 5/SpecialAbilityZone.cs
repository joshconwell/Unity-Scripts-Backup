using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpecialAbilityZone : MonoBehaviour
{
    public enum ZoneType
    {
        BlackHole,
        PoisonCloud,
        IceNova
    }

    private ZoneType zoneType = ZoneType.PoisonCloud;

    private float radius = 2f;
    private float lifetime = 3f;
    private float damagePerTick = 5f;
    private float tickInterval = 0.35f;
    private float pullStrength = 0f;
    private float knockbackForce = 0f;
    private float velocityDamping = 1f;
    private bool damageEachEnemyOnlyOnce = false;
    private LayerMask enemyHitMask = ~0;

    private Color startColor = Color.white;
    private Color endColor = Color.clear;
    private float lineWidth = 0.18f;
    private int circleSegments = 64;
    private bool expandVisual = false;

    private float lifeTimer;
    private float tickTimer;
    private bool initialized;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    private readonly HashSet<Health> damagedThisTick = new HashSet<Health>();
    private readonly HashSet<Health> damagedThisZone = new HashSet<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        tickTimer = 0f;
        damagedThisTick.Clear();
        damagedThisZone.Clear();
    }

    private void OnDisable()
    {
        initialized = false;
        damagedThisTick.Clear();
        damagedThisZone.Clear();
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
        ZoneType newZoneType,
        Vector3 position,
        float newRadius,
        float newLifetime,
        float newDamagePerTick,
        float newTickInterval,
        LayerMask newEnemyHitMask,
        Color newStartColor,
        Color newEndColor,
        float newLineWidth,
        int newCircleSegments,
        float newPullStrength = 0f,
        float newKnockbackForce = 0f,
        float newVelocityDamping = 1f,
        bool newDamageEachEnemyOnlyOnce = false,
        bool newExpandVisual = false)
    {
        zoneType = newZoneType;

        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        radius = Mathf.Max(0.1f, newRadius);
        lifetime = Mathf.Max(0.05f, newLifetime);
        damagePerTick = Mathf.Max(0f, newDamagePerTick);
        tickInterval = Mathf.Max(0.05f, newTickInterval);
        enemyHitMask = newEnemyHitMask;

        startColor = newStartColor;
        endColor = newEndColor;
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        circleSegments = Mathf.Max(12, newCircleSegments);

        pullStrength = Mathf.Max(0f, newPullStrength);
        knockbackForce = Mathf.Max(0f, newKnockbackForce);
        velocityDamping = Mathf.Clamp(newVelocityDamping, 0f, 1f);
        damageEachEnemyOnlyOnce = newDamageEachEnemyOnlyOnce;
        expandVisual = newExpandVisual;

        lifeTimer = 0f;
        tickTimer = 0f;
        damagedThisTick.Clear();
        damagedThisZone.Clear();
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

        if (zoneType == ZoneType.BlackHole)
        {
            ApplyBlackHolePull();
        }

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

    private void ApplyBlackHolePull()
    {
        if (pullStrength <= 0f)
        {
            return;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null || !IsEnemyCollider(hitCollider))
            {
                continue;
            }

            Rigidbody2D enemyRigidbody = hitCollider.attachedRigidbody;

            if (enemyRigidbody == null)
            {
                enemyRigidbody = hitCollider.GetComponentInParent<Rigidbody2D>();
            }

            if (enemyRigidbody == null)
            {
                continue;
            }

            Vector2 pullDirection = (Vector2)transform.position - enemyRigidbody.position;
            float distance = pullDirection.magnitude;

            if (distance <= 0.05f)
            {
                enemyRigidbody.linearVelocity *= 0.85f;
                continue;
            }

            float distancePercent = Mathf.Clamp01(distance / radius);
            float strengthMultiplier = 1f - distancePercent;
            Vector2 force = pullDirection.normalized * pullStrength * Mathf.Lerp(0.35f, 1f, strengthMultiplier);

            enemyRigidbody.AddForce(force, ForceMode2D.Force);
        }
    }

    private void DamageEnemiesInsideZone()
    {
        if (damagePerTick <= 0f && knockbackForce <= 0f && velocityDamping >= 0.999f)
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

            if (hitCollider == null || !IsEnemyCollider(hitCollider))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            if (damagedThisTick.Contains(enemyHealth))
            {
                continue;
            }

            if (damageEachEnemyOnlyOnce && damagedThisZone.Contains(enemyHealth))
            {
                continue;
            }

            damagedThisTick.Add(enemyHealth);
            damagedThisZone.Add(enemyHealth);

            if (damagePerTick > 0f)
            {
                enemyHealth.TakeDamage(damagePerTick, false);
            }

            ApplyExtraHitEffects(hitCollider);
        }
    }

    private void ApplyExtraHitEffects(Collider2D hitCollider)
    {
        Rigidbody2D enemyRigidbody = hitCollider.attachedRigidbody;

        if (enemyRigidbody == null)
        {
            enemyRigidbody = hitCollider.GetComponentInParent<Rigidbody2D>();
        }

        if (enemyRigidbody == null)
        {
            return;
        }

        if (velocityDamping < 0.999f)
        {
            enemyRigidbody.linearVelocity *= velocityDamping;
        }

        if (knockbackForce > 0f)
        {
            Vector2 knockbackDirection = enemyRigidbody.position - (Vector2)transform.position;

            if (knockbackDirection.sqrMagnitude <= 0.01f)
            {
                knockbackDirection = Random.insideUnitCircle.normalized;
            }

            enemyRigidbody.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private bool IsEnemyCollider(Collider2D hitCollider)
    {
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
        lineRenderer.sortingOrder = GetSortingOrder();

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

    private int GetSortingOrder()
    {
        switch (zoneType)
        {
            case ZoneType.BlackHole:
                return 38;

            case ZoneType.IceNova:
                return 42;

            case ZoneType.PoisonCloud:
                return 36;
        }

        return 36;
    }

    private void UpdateVisual(float lifetimePercent)
    {
        if (lineRenderer == null)
        {
            return;
        }

        float currentRadius = radius;

        if (expandVisual)
        {
            currentRadius = Mathf.Lerp(radius * 0.12f, radius, lifetimePercent);
        }
        else
        {
            float pulseSpeed = zoneType == ZoneType.BlackHole ? 18f : 10f;
            float pulseAmount = zoneType == ZoneType.BlackHole ? 0.08f : 0.04f;
            currentRadius = radius * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
        }

        Color currentColor = Color.Lerp(startColor, endColor, lifetimePercent);

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        float widthMultiplier = zoneType == ZoneType.IceNova ? Mathf.Lerp(1.25f, 0.2f, lifetimePercent) : Mathf.Lerp(1f, 0.4f, lifetimePercent);
        lineRenderer.startWidth = lineWidth * widthMultiplier;
        lineRenderer.endWidth = lineWidth * widthMultiplier;

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
}

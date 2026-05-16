using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShrapnelMine : MonoBehaviour
{
    private float triggerRadius = 1f;
    private float blastRadius = 2f;
    private float damage = 24f;
    private float armTime = 0.45f;
    private float lifetime = 5f;
    private float explosionVisualDuration = 0.25f;
    private float knockbackForce = 2.5f;
    private LayerMask enemyHitMask = ~0;

    private Color idleColor = new Color(1f, 0.8f, 0.15f, 0.9f);
    private Color armedColor = new Color(1f, 0.2f, 0.05f, 0.95f);
    private Color blastColor = new Color(1f, 0.65f, 0.1f, 1f);
    private float lineWidth = 0.11f;
    private int circleSegments = 32;

    private float lifeTimer;
    private float explosionTimer;
    private bool initialized;
    private bool exploded;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    private readonly HashSet<Health> damagedEnemies = new HashSet<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        explosionTimer = 0f;
        initialized = false;
        exploded = false;
        damagedEnemies.Clear();
    }

    private void OnDisable()
    {
        initialized = false;
        damagedEnemies.Clear();
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
        float newTriggerRadius,
        float newBlastRadius,
        float newDamage,
        float newArmTime,
        float newLifetime,
        float newExplosionVisualDuration,
        LayerMask newEnemyHitMask,
        Color newIdleColor,
        Color newArmedColor,
        Color newBlastColor,
        float newLineWidth,
        int newCircleSegments,
        float newKnockbackForce)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        triggerRadius = Mathf.Max(0.1f, newTriggerRadius);
        blastRadius = Mathf.Max(0.1f, newBlastRadius);
        damage = Mathf.Max(0f, newDamage);
        armTime = Mathf.Max(0f, newArmTime);
        lifetime = Mathf.Max(0.25f, newLifetime);
        explosionVisualDuration = Mathf.Max(0.05f, newExplosionVisualDuration);
        enemyHitMask = newEnemyHitMask;
        idleColor = newIdleColor;
        armedColor = newArmedColor;
        blastColor = newBlastColor;
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        circleSegments = Mathf.Max(12, newCircleSegments);
        knockbackForce = Mathf.Max(0f, newKnockbackForce);

        lifeTimer = 0f;
        explosionTimer = 0f;
        exploded = false;
        initialized = true;
        damagedEnemies.Clear();

        SetupLineRenderer();
        UpdateMineVisual();
    }

    private void Update()
    {
        if (!initialized || Time.timeScale <= 0f)
        {
            return;
        }

        if (exploded)
        {
            explosionTimer += Time.deltaTime;
            UpdateExplosionVisual(Mathf.Clamp01(explosionTimer / explosionVisualDuration));

            if (explosionTimer >= explosionVisualDuration)
            {
                ReturnOrDestroy();
            }

            return;
        }

        lifeTimer += Time.deltaTime;
        UpdateMineVisual();

        if (lifeTimer >= armTime && HasEnemyInsideTrigger())
        {
            Explode();
            return;
        }

        if (lifeTimer >= lifetime)
        {
            Explode();
        }
    }

    private bool HasEnemyInsideTrigger()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, triggerRadius, enemyHitMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i] != null && IsEnemyCollider(hitColliders[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void Explode()
    {
        if (exploded)
        {
            return;
        }

        exploded = true;
        explosionTimer = 0f;
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        damagedEnemies.Clear();
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, blastRadius, enemyHitMask);

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

            if (enemyHealth == null || enemyHealth.IsDead || damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            damagedEnemies.Add(enemyHealth);
            enemyHealth.TakeDamage(damage, false);
            ApplyKnockback(hitCollider);
        }
    }

    private void ApplyKnockback(Collider2D hitCollider)
    {
        if (knockbackForce <= 0f)
        {
            return;
        }

        Rigidbody2D rb = hitCollider.attachedRigidbody;

        if (rb == null)
        {
            rb = hitCollider.GetComponentInParent<Rigidbody2D>();
        }

        if (rb == null)
        {
            return;
        }

        Vector2 direction = rb.position - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.01f)
        {
            direction = Random.insideUnitCircle.normalized;
        }

        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
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
        lineRenderer.sortingOrder = 44;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                lineMaterial = new Material(spriteShader);
                lineRenderer.sharedMaterial = lineMaterial;
            }
        }
    }

    private void UpdateMineVisual()
    {
        if (lineRenderer == null)
        {
            return;
        }

        bool armed = lifeTimer >= armTime;
        float pulse = armed ? 1f + Mathf.Sin(Time.time * 12f) * 0.08f : 1f;
        float radius = triggerRadius * 0.45f * pulse;
        Color color = armed ? armedColor : idleColor;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        UpdateCirclePoints(radius);
    }

    private void UpdateExplosionVisual(float percent)
    {
        if (lineRenderer == null)
        {
            return;
        }

        float radius = Mathf.Lerp(triggerRadius * 0.35f, blastRadius, percent);
        Color color = blastColor;
        color.a = Mathf.Lerp(blastColor.a, 0f, percent);

        lineRenderer.startWidth = Mathf.Lerp(lineWidth * 2f, lineWidth * 0.25f, percent);
        lineRenderer.endWidth = lineRenderer.startWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        UpdateCirclePoints(radius);
    }

    private void UpdateCirclePoints(float radius)
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
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
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

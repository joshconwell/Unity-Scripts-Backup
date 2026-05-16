using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpecialAbilityDelayedBlast : MonoBehaviour
{
    private enum BlastState
    {
        Warning,
        Blast
    }

    private float warningRadius = 2f;
    private float warningDuration = 0.8f;
    private float blastRadius = 2f;
    private float blastDamage = 30f;
    private float blastVisualDuration = 0.25f;
    private float knockbackForce = 0f;
    private LayerMask enemyHitMask = ~0;

    private Color warningColor = new Color(1f, 0.45f, 0.05f, 0.85f);
    private Color blastColor = new Color(1f, 0.8f, 0.25f, 1f);
    private float lineWidth = 0.18f;
    private int circleSegments = 64;

    private float timer;
    private bool initialized;
    private bool hasDamaged;
    private BlastState state = BlastState.Warning;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    private readonly HashSet<Health> damagedEnemies = new HashSet<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        timer = 0f;
        initialized = false;
        hasDamaged = false;
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
        float newWarningRadius,
        float newWarningDuration,
        float newBlastRadius,
        float newBlastDamage,
        float newBlastVisualDuration,
        LayerMask newEnemyHitMask,
        Color newWarningColor,
        Color newBlastColor,
        float newLineWidth,
        int newCircleSegments,
        float newKnockbackForce = 0f)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        warningRadius = Mathf.Max(0.1f, newWarningRadius);
        warningDuration = Mathf.Max(0.05f, newWarningDuration);
        blastRadius = Mathf.Max(0.1f, newBlastRadius);
        blastDamage = Mathf.Max(0f, newBlastDamage);
        blastVisualDuration = Mathf.Max(0.05f, newBlastVisualDuration);
        enemyHitMask = newEnemyHitMask;
        warningColor = newWarningColor;
        blastColor = newBlastColor;
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        circleSegments = Mathf.Max(12, newCircleSegments);
        knockbackForce = Mathf.Max(0f, newKnockbackForce);

        timer = 0f;
        state = BlastState.Warning;
        hasDamaged = false;
        damagedEnemies.Clear();
        initialized = true;

        SetupLineRenderer();
        UpdateWarningVisual(0f);
    }

    private void Update()
    {
        if (!initialized || Time.timeScale <= 0f)
        {
            return;
        }

        timer += Time.deltaTime;

        if (state == BlastState.Warning)
        {
            float t = Mathf.Clamp01(timer / warningDuration);
            UpdateWarningVisual(t);

            if (timer >= warningDuration)
            {
                state = BlastState.Blast;
                timer = 0f;
                DamageEnemies();
            }

            return;
        }

        float blastPercent = Mathf.Clamp01(timer / blastVisualDuration);
        UpdateBlastVisual(blastPercent);

        if (timer >= blastVisualDuration)
        {
            ReturnOrDestroy();
        }
    }

    private void DamageEnemies()
    {
        if (hasDamaged)
        {
            return;
        }

        hasDamaged = true;
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
            enemyHealth.TakeDamage(blastDamage, false);
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
        lineRenderer.sortingOrder = 48;

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

    private void UpdateWarningVisual(float percent)
    {
        if (lineRenderer == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.06f;
        float radius = warningRadius * pulse;
        Color color = warningColor;
        color.a = Mathf.Lerp(0.25f, warningColor.a, percent);

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        UpdateCirclePoints(radius);
    }

    private void UpdateBlastVisual(float percent)
    {
        if (lineRenderer == null)
        {
            return;
        }

        float radius = Mathf.Lerp(blastRadius * 0.12f, blastRadius, percent);
        Color color = blastColor;
        color.a = Mathf.Lerp(blastColor.a, 0f, percent);

        lineRenderer.startWidth = Mathf.Lerp(lineWidth * 1.6f, lineWidth * 0.25f, percent);
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

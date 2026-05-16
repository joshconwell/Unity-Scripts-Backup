using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Health))]
public class EnemyBomberController2D : MonoBehaviour
{
    private enum BomberState
    {
        Chasing,
        Priming,
        Exploding
    }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.1f;
    [SerializeField] private float stoppingDistance = 0.15f;
    [SerializeField] private bool rotateTowardMovement = true;

    [Header("Explosion Trigger")]
    [SerializeField] private float primeRange = 2.2f;
    [SerializeField] private float fuseDuration = 0.85f;
    [SerializeField] private bool explodeOnDeath = true;
    [SerializeField] private bool explodeOnlyOnce = true;

    [Header("Explosion Damage")]
    [SerializeField] private float explosionRadius = 2.6f;
    [SerializeField] private float explosionDamage = 22f;
    [SerializeField] private LayerMask explosionHitMask = ~0;

    [Header("Explosion Visual")]
    [SerializeField] private bool showWarningCircle = true;
    [SerializeField] private Color warningCircleColor = new Color(1f, 0.18f, 0.08f, 0.55f);
    [SerializeField] private Color explosionCircleColor = new Color(1f, 0.8f, 0.2f, 0.85f);
    [SerializeField] private float warningLineWidth = 0.08f;
    [SerializeField] private float explosionLineWidth = 0.16f;
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private int visualSortingOrder = 35;
    [SerializeField] private float explosionVisualDuration = 0.18f;

    [Header("Flashing")]
    [SerializeField] private bool flashDuringFuse = true;
    [SerializeField] private Color fuseFlashColor = new Color(1f, 0.12f, 0.05f, 1f);
    [SerializeField] private float flashSpeed = 14f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("After Explosion")]
    [SerializeField] private bool destroyAfterExplosion = true;
    [SerializeField] private float destroyDelay = 0.02f;

    [Header("Debug")]
    [SerializeField] private bool allowDebugExplodeKey = false;
    [SerializeField] private KeyCode debugExplodeKey = KeyCode.F4;

    private Rigidbody2D rb;
    private Health health;
    private Collider2D ownCollider;
    private LineRenderer warningCircleRenderer;

    private BomberState state = BomberState.Chasing;

    private Coroutine primeRoutine;
    private Coroutine explosionVisualRoutine;

    private Color[] originalSpriteColors;

    private bool hasExploded;
    private bool deathTriggeredExplosion;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        ownCollider = GetComponent<Collider2D>();

        AutoFindSpriteRenderers();
        CacheOriginalSpriteColors();
        BuildWarningCircle();
    }

    private void OnEnable()
    {
        FindTargetIfNeeded();

        state = BomberState.Chasing;
        hasExploded = false;
        deathTriggeredExplosion = false;

        RestoreSpriteColors();
        HideWarningCircle();

        if (ownCollider != null)
        {
            ownCollider.enabled = true;
        }

        if (health != null)
        {
            health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }

        if (primeRoutine != null)
        {
            StopCoroutine(primeRoutine);
            primeRoutine = null;
        }

        if (explosionVisualRoutine != null)
        {
            StopCoroutine(explosionVisualRoutine);
            explosionVisualRoutine = null;
        }

        RestoreSpriteColors();
        HideWarningCircle();
    }

    private void Update()
    {
        if (allowDebugExplodeKey && Input.GetKeyDown(debugExplodeKey))
        {
            BeginPriming();
        }

        UpdateFuseFlash();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FindTargetIfNeeded();

        if (target == null)
        {
            return;
        }

        if (state == BomberState.Chasing)
        {
            HandleChasing();
        }
    }

    private void HandleChasing()
    {
        Vector2 enemyPosition = rb.position;
        Vector2 targetPosition = target.position;
        Vector2 toTarget = targetPosition - enemyPosition;

        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= primeRange)
        {
            BeginPriming();
            return;
        }

        if (distanceToTarget > stoppingDistance)
        {
            Vector2 moveDirection = toTarget.normalized;
            Vector2 newPosition = enemyPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;

            rb.MovePosition(newPosition);

            if (rotateTowardMovement)
            {
                RotateTowardDirection(moveDirection);
            }
        }
    }

    private void BeginPriming()
    {
        if (state != BomberState.Chasing)
        {
            return;
        }

        if (explodeOnlyOnce && hasExploded)
        {
            return;
        }

        state = BomberState.Priming;

        if (primeRoutine != null)
        {
            StopCoroutine(primeRoutine);
        }

        primeRoutine = StartCoroutine(PrimeRoutine());
    }

    private IEnumerator PrimeRoutine()
    {
        rb.linearVelocity = Vector2.zero;

        if (showWarningCircle)
        {
            ShowWarningCircle(warningCircleColor, warningLineWidth, explosionRadius);
        }

        float timer = 0f;

        while (timer < fuseDuration)
        {
            timer += Time.deltaTime;

            if (showWarningCircle)
            {
                ShowWarningCircle(warningCircleColor, warningLineWidth, explosionRadius);
            }

            yield return null;
        }

        Explode();

        primeRoutine = null;
    }

    private void Explode()
    {
        if (explodeOnlyOnce && hasExploded)
        {
            return;
        }

        hasExploded = true;
        state = BomberState.Exploding;

        rb.linearVelocity = Vector2.zero;

        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        DamagePlayerInRadius();

        if (explosionVisualRoutine != null)
        {
            StopCoroutine(explosionVisualRoutine);
        }

        explosionVisualRoutine = StartCoroutine(ExplosionVisualRoutine());

        if (destroyAfterExplosion)
        {
            StartCoroutine(DestroyAfterExplosionRoutine());
        }
    }

    private void DamagePlayerInRadius()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            explosionHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!IsPlayerCollider(hitCollider))
            {
                continue;
            }

            Health playerHealth = hitCollider.GetComponent<Health>();

            if (playerHealth == null)
            {
                playerHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (playerHealth == null)
            {
                continue;
            }

            playerHealth.TakeDamage(explosionDamage);
            return;
        }
    }

    private bool IsPlayerCollider(Collider2D hitCollider)
    {
        if (hitCollider.CompareTag(playerTag))
        {
            return true;
        }

        Transform currentTransform = hitCollider.transform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag(playerTag))
            {
                return true;
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private IEnumerator ExplosionVisualRoutine()
    {
        ShowWarningCircle(explosionCircleColor, explosionLineWidth, explosionRadius);

        float timer = 0f;

        while (timer < explosionVisualDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / explosionVisualDuration);
            float radius = Mathf.Lerp(explosionRadius * 0.55f, explosionRadius * 1.25f, t);

            Color finalColor = explosionCircleColor;
            finalColor.a = Mathf.Lerp(explosionCircleColor.a, 0f, t);

            ShowWarningCircle(finalColor, explosionLineWidth, radius);

            yield return null;
        }

        HideWarningCircle();
        explosionVisualRoutine = null;
    }

    private IEnumerator DestroyAfterExplosionRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (gameObject == null)
        {
            yield break;
        }

        PooledObject pooledObject = GetComponent<PooledObject>();

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
            yield break;
        }

        Destroy(gameObject);
    }

    private void HandleDied()
    {
        if (deathTriggeredExplosion)
        {
            return;
        }

        deathTriggeredExplosion = true;

        if (explodeOnDeath && !hasExploded)
        {
            Explode();
        }
    }

    private void FindTargetIfNeeded()
    {
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void RotateTowardDirection(Vector2 direction)
    {
        if (!rotateTowardMovement)
        {
            return;
        }

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }

    private void BuildWarningCircle()
    {
        if (!showWarningCircle)
        {
            return;
        }

        if (warningCircleRenderer != null)
        {
            return;
        }

        GameObject circleObject = new GameObject("Bomber Explosion Warning Circle");
        circleObject.transform.SetParent(transform, false);
        circleObject.transform.localPosition = Vector3.zero;
        circleObject.transform.localRotation = Quaternion.identity;
        circleObject.transform.localScale = Vector3.one;

        warningCircleRenderer = circleObject.AddComponent<LineRenderer>();
        warningCircleRenderer.useWorldSpace = true;
        warningCircleRenderer.loop = true;
        warningCircleRenderer.positionCount = Mathf.Max(8, circleSegments);
        warningCircleRenderer.startWidth = warningLineWidth;
        warningCircleRenderer.endWidth = warningLineWidth;
        warningCircleRenderer.startColor = warningCircleColor;
        warningCircleRenderer.endColor = warningCircleColor;
        warningCircleRenderer.sortingOrder = visualSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            warningCircleRenderer.material = new Material(spriteShader);
        }

        HideWarningCircle();
    }

    private void ShowWarningCircle(Color color, float width, float radius)
    {
        if (!showWarningCircle)
        {
            return;
        }

        if (warningCircleRenderer == null)
        {
            BuildWarningCircle();
        }

        if (warningCircleRenderer == null)
        {
            return;
        }

        warningCircleRenderer.gameObject.SetActive(true);
        warningCircleRenderer.startColor = color;
        warningCircleRenderer.endColor = color;
        warningCircleRenderer.startWidth = width;
        warningCircleRenderer.endWidth = width;

        int safeSegments = Mathf.Max(8, circleSegments);
        warningCircleRenderer.positionCount = safeSegments;

        for (int i = 0; i < safeSegments; i++)
        {
            float t = (float)i / safeSegments;
            float angle = t * Mathf.PI * 2f;

            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            warningCircleRenderer.SetPosition(i, point);
        }
    }

    private void HideWarningCircle()
    {
        if (warningCircleRenderer != null)
        {
            warningCircleRenderer.gameObject.SetActive(false);
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

    private void CacheOriginalSpriteColors()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        originalSpriteColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalSpriteColors[i] = spriteRenderers[i].color;
            }
        }
    }

    private void UpdateFuseFlash()
    {
        if (!flashDuringFuse)
        {
            return;
        }

        if (state != BomberState.Priming)
        {
            return;
        }

        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        Color finalColor = Color.Lerp(Color.white, fuseFlashColor, pulse);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = finalColor;
            }
        }
    }

    private void RestoreSpriteColors()
    {
        if (spriteRenderers == null || originalSpriteColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < originalSpriteColors.Length)
            {
                spriteRenderers[i].color = originalSpriteColors[i];
            }
        }
    }
}
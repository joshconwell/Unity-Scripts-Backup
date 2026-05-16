using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossMortarRainAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float activationRange = 28f;
    [SerializeField] private bool predictPlayerMovement = true;
    [SerializeField] private float predictionTime = 0.45f;
    [SerializeField] private float randomOffsetRadius = 1.65f;

    [Header("Attack Timing")]
    [SerializeField] private float firstAttackDelay = 2.25f;
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private int strikesPerVolley = 5;
    [SerializeField] private float timeBetweenStrikes = 0.22f;
    [SerializeField] private float warningDuration = 0.9f;

    [Header("Damage")]
    [SerializeField] private float explosionDamage = 22f;
    [SerializeField] private float explosionRadius = 1.65f;
    [Tooltip("Leave as Everything if you are unsure.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Warning Visual")]
    [SerializeField] private Color warningColor = new Color(1f, 0.2f, 0.05f, 0.9f);
    [SerializeField] private float warningLineWidth = 0.13f;
    [SerializeField] private float warningPulseSpeed = 8f;

    [Header("Explosion Visual")]
    [SerializeField] private Color explosionColor = new Color(1f, 0.65f, 0.12f, 1f);
    [SerializeField] private float explosionLineWidth = 0.3f;
    [SerializeField] private float explosionVisualDuration = 0.22f;
    [SerializeField] private float explosionVisualScaleMultiplier = 1.25f;

    [Header("Line Renderer Settings")]
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private int visualSortingOrder = 55;

    [Header("Boss Flash")]
    [SerializeField] private bool flashBeforeVolley = true;
    [SerializeField] private Color flashColor = new Color(1f, 0.25f, 0.05f, 1f);
    [SerializeField] private float flashInterval = 0.08f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugAttackKey = true;
    [SerializeField] private KeyCode debugAttackKey = KeyCode.F10;

    private Health health;
    private Transform player;
    private Rigidbody2D playerRigidbody;
    private Coroutine attackRoutine;
    private Coroutine flashRoutine;

    private bool isDead;
    private bool isAttacking;

    private Color[] originalRendererColors;
    private Material lineMaterial;

    private readonly List<GameObject> activeVisualObjects = new List<GameObject>();
    private readonly HashSet<Health> damagedThisExplosion = new HashSet<Health>();

    private void Awake()
    {
        health = GetComponent<Health>();

        AutoFindSpriteRenderers();
        CacheOriginalRendererColors();
    }

    private void OnEnable()
    {
        isDead = false;
        isAttacking = false;

        FindPlayerIfNeeded();
        RestoreOriginalRendererColors();
        ClearActiveVisuals();

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
        StopFlashRoutine();
        RestoreOriginalRendererColors();
        ClearActiveVisuals();
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
            lineMaterial = null;
        }
    }

    private void Update()
    {
        if (allowDebugAttackKey && Input.GetKeyDown(debugAttackKey))
        {
            StartCoroutine(PerformMortarVolley());
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
                yield return PerformMortarVolley();
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator PerformMortarVolley()
    {
        if (isAttacking || isDead)
        {
            yield break;
        }

        FindPlayerIfNeeded();

        if (player == null)
        {
            yield break;
        }

        isAttacking = true;

        if (flashBeforeVolley)
        {
            StopFlashRoutine();
            flashRoutine = StartCoroutine(FlashRoutine(warningDuration));
        }

        int safeStrikeCount = Mathf.Max(1, strikesPerVolley);

        for (int i = 0; i < safeStrikeCount; i++)
        {
            if (isDead)
            {
                break;
            }

            Vector3 strikePosition = GetMortarTargetPosition();
            StartCoroutine(SingleMortarStrikeRoutine(strikePosition));

            if (i < safeStrikeCount - 1 && timeBetweenStrikes > 0f)
            {
                yield return new WaitForSeconds(timeBetweenStrikes);
            }
        }

        float cleanupWait = warningDuration + explosionVisualDuration;

        if (cleanupWait > 0f)
        {
            yield return new WaitForSeconds(cleanupWait);
        }

        RestoreOriginalRendererColors();
        isAttacking = false;
    }

    private IEnumerator SingleMortarStrikeRoutine(Vector3 strikePosition)
    {
        GameObject warningCircle = CreateCircleVisual(
            "Mortar Warning Circle",
            strikePosition,
            explosionRadius,
            warningColor,
            warningLineWidth
        );

        LineRenderer warningLine = null;

        if (warningCircle != null)
        {
            warningLine = warningCircle.GetComponent<LineRenderer>();
        }

        float timer = 0f;

        while (timer < warningDuration && !isDead)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, warningDuration));
            float pulse = 1f + Mathf.Sin(Time.time * warningPulseSpeed) * 0.08f;

            if (warningCircle != null)
            {
                warningCircle.transform.localScale = Vector3.one * pulse;
            }

            if (warningLine != null)
            {
                Color currentColor = warningColor;
                currentColor.a = Mathf.Lerp(0.35f, warningColor.a, t);
                SetLineColor(warningLine, currentColor);
            }

            yield return null;
        }

        DestroyVisual(warningCircle);

        if (isDead)
        {
            yield break;
        }

        DamageAtPosition(strikePosition);
        StartCoroutine(ExplosionVisualRoutine(strikePosition));
    }

    private IEnumerator ExplosionVisualRoutine(Vector3 explosionPosition)
    {
        GameObject explosionCircle = CreateCircleVisual(
            "Mortar Explosion Circle",
            explosionPosition,
            0.15f,
            explosionColor,
            explosionLineWidth
        );

        LineRenderer explosionLine = null;

        if (explosionCircle != null)
        {
            explosionLine = explosionCircle.GetComponent<LineRenderer>();
        }

        float timer = 0f;
        float safeDuration = Mathf.Max(0.01f, explosionVisualDuration);
        float targetScale = explosionRadius * explosionVisualScaleMultiplier;

        while (timer < safeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / safeDuration);
            float radius = Mathf.Lerp(0.15f, targetScale, t);

            if (explosionLine != null)
            {
                UpdateCirclePoints(explosionLine, radius);

                Color currentColor = explosionColor;
                currentColor.a = Mathf.Lerp(explosionColor.a, 0f, t);
                SetLineColor(explosionLine, currentColor);
            }

            yield return null;
        }

        DestroyVisual(explosionCircle);
    }

    private void DamageAtPosition(Vector3 explosionPosition)
    {
        damagedThisExplosion.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            explosionPosition,
            explosionRadius,
            hitMask
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

            if (damagedThisExplosion.Contains(playerHealth))
            {
                continue;
            }

            damagedThisExplosion.Add(playerHealth);
            playerHealth.TakeDamage(explosionDamage);
        }
    }

    private Vector3 GetMortarTargetPosition()
    {
        Vector3 targetPosition = player.position;

        if (predictPlayerMovement && playerRigidbody != null)
        {
            targetPosition += new Vector3(
                playerRigidbody.linearVelocity.x,
                playerRigidbody.linearVelocity.y,
                0f
            ) * predictionTime;
        }

        Vector2 randomOffset = Random.insideUnitCircle * randomOffsetRadius;

        targetPosition += new Vector3(randomOffset.x, randomOffset.y, 0f);
        targetPosition.z = transform.position.z;

        return targetPosition;
    }

    private GameObject CreateCircleVisual(
        string objectName,
        Vector3 centerPosition,
        float radius,
        Color color,
        float lineWidth
    )
    {
        GameObject circleObject = new GameObject(objectName);
        circleObject.transform.position = centerPosition;
        circleObject.transform.rotation = Quaternion.identity;
        circleObject.transform.localScale = Vector3.one;

        LineRenderer lineRenderer = circleObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = Mathf.Max(12, circleSegments);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.sortingOrder = visualSortingOrder;
        lineRenderer.material = GetLineMaterial();

        SetLineColor(lineRenderer, color);
        UpdateCirclePoints(lineRenderer, radius);

        activeVisualObjects.Add(circleObject);

        return circleObject;
    }

    private void UpdateCirclePoints(LineRenderer lineRenderer, float radius)
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

    private void SetLineColor(LineRenderer lineRenderer, Color color)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private Material GetLineMaterial()
    {
        if (lineMaterial != null)
        {
            return lineMaterial;
        }

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            lineMaterial = new Material(spriteShader);
        }
        else
        {
            lineMaterial = new Material(Shader.Find("Default-Line"));
        }

        return lineMaterial;
    }

    private void DestroyVisual(GameObject visualObject)
    {
        if (visualObject == null)
        {
            return;
        }

        activeVisualObjects.Remove(visualObject);
        Destroy(visualObject);
    }

    private void ClearActiveVisuals()
    {
        for (int i = activeVisualObjects.Count - 1; i >= 0; i--)
        {
            if (activeVisualObjects[i] != null)
            {
                Destroy(activeVisualObjects[i]);
            }
        }

        activeVisualObjects.Clear();
    }

    private bool IsPlayerCollider(Collider2D hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }

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
            if (playerRigidbody == null)
            {
                playerRigidbody = player.GetComponent<Rigidbody2D>();
            }

            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        }
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            yield break;
        }

        float elapsedTime = 0f;
        bool useFlashColor = true;

        while (elapsedTime < duration && !isDead)
        {
            SetRendererColors(useFlashColor ? flashColor : Color.white, useFlashColor);

            useFlashColor = !useFlashColor;

            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        RestoreOriginalRendererColors();
        flashRoutine = null;
    }

    private void StopFlashRoutine()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
    }

    private void HandleDied()
    {
        isDead = true;
        isAttacking = false;

        StopAttackRoutine();
        StopFlashRoutine();
        RestoreOriginalRendererColors();
        ClearActiveVisuals();
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

    private void CacheOriginalRendererColors()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        originalRendererColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalRendererColors[i] = spriteRenderers[i].color;
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
            else if (originalRendererColors != null && i < originalRendererColors.Length)
            {
                spriteRenderers[i].color = originalRendererColors[i];
            }
        }
    }

    private void RestoreOriginalRendererColors()
    {
        if (spriteRenderers == null || originalRendererColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < originalRendererColors.Length)
            {
                spriteRenderers[i].color = originalRendererColors[i];
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Health))]
public class EnemyShielderController2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.9f;
    [SerializeField] private float fleeDistance = 5.25f;
    [SerializeField] private float followDistance = 9.5f;
    [SerializeField] private bool useStrafing = true;
    [SerializeField] private float strafeSpeedMultiplier = 0.4f;
    [SerializeField] private float strafeDirectionChangeInterval = 2.5f;

    [Header("Shield Aura")]
    [SerializeField] private float shieldRadius = 5.25f;
    [SerializeField] private float shieldInterval = 2.75f;
    [SerializeField] private float shieldDuration = 3.25f;

    [Tooltip("0.35 means shielded enemies take 35% less damage.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float damageReductionPercent = 0.35f;

    [SerializeField] private int maxEnemiesShieldedPerPulse = 6;
    [SerializeField] private bool canShieldSelf = false;

    [Tooltip("Leave as Everything if unsure.")]
    [SerializeField] private LayerMask shieldTargetMask = ~0;

    [Header("Aura Visual")]
    [SerializeField] private bool showAuraPulse = true;
    [SerializeField] private Color auraColor = new Color(0.2f, 0.6f, 1f, 0.6f);
    [SerializeField] private Color shieldRingColor = new Color(0.35f, 0.8f, 1f, 0.75f);
    [SerializeField] private float auraLineWidth = 0.08f;
    [SerializeField] private int auraSegments = 72;
    [SerializeField] private int auraSortingOrder = 32;
    [SerializeField] private float auraVisualDuration = 0.3f;

    [Header("Shielded Enemy Ring")]
    [SerializeField] private float shieldedEnemyRingRadius = 0.78f;
    [SerializeField] private float shieldedEnemyRingWidth = 0.07f;
    [SerializeField] private int shieldedEnemySortingOrder = 33;

    [Header("Sprite Flash")]
    [SerializeField] private bool flashWhenShielding = true;
    [SerializeField] private Color shieldFlashColor = new Color(0.4f, 0.75f, 1f, 1f);
    [SerializeField] private float flashDuration = 0.18f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugShieldKey = false;
    [SerializeField] private KeyCode debugShieldKey = KeyCode.F3;

    private Rigidbody2D rb;
    private Health ownHealth;
    private LineRenderer auraRenderer;

    private readonly List<Health> shieldTargets = new List<Health>();
    private readonly HashSet<Health> uniqueShieldTargets = new HashSet<Health>();

    private float nextShieldTime;
    private float nextStrafeChangeTime;
    private int strafeDirection = 1;

    private Color[] originalSpriteColors;
    private Coroutine auraRoutine;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ownHealth = GetComponent<Health>();

        AutoFindSpriteRenderers();
        CacheOriginalSpriteColors();
        BuildAuraRing();
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();

        nextShieldTime = Time.time + Random.Range(0.75f, shieldInterval);
        nextStrafeChangeTime = Time.time + Random.Range(0.25f, strafeDirectionChangeInterval);
        strafeDirection = Random.value < 0.5f ? -1 : 1;

        RestoreSpriteColors();
        HideAuraRing();
    }

    private void OnDisable()
    {
        if (auraRoutine != null)
        {
            StopCoroutine(auraRoutine);
            auraRoutine = null;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        RestoreSpriteColors();
        HideAuraRing();
    }

    private void Update()
    {
        if (allowDebugShieldKey && Input.GetKeyDown(debugShieldKey))
        {
            PerformShieldPulse();
        }

        if (ownHealth != null && ownHealth.IsDead)
        {
            return;
        }

        if (Time.time >= nextShieldTime)
        {
            PerformShieldPulse();
            nextShieldTime = Time.time + shieldInterval;
        }
    }

    private void FixedUpdate()
    {
        if (ownHealth != null && ownHealth.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FindPlayerIfNeeded();

        if (playerTarget == null)
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 shielderPosition = rb.position;
        Vector2 playerPosition = playerTarget.position;
        Vector2 toPlayer = playerPosition - shielderPosition;

        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer <= 0.001f)
        {
            return;
        }

        Vector2 directionToPlayer = toPlayer.normalized;
        Vector2 moveDirection = Vector2.zero;

        if (distanceToPlayer < fleeDistance)
        {
            moveDirection = -directionToPlayer;
        }
        else if (distanceToPlayer > followDistance)
        {
            moveDirection = directionToPlayer;
        }

        if (useStrafing)
        {
            if (Time.time >= nextStrafeChangeTime)
            {
                strafeDirection *= -1;
                nextStrafeChangeTime = Time.time + strafeDirectionChangeInterval + Random.Range(-0.35f, 0.35f);
            }

            Vector2 strafeDirectionVector = new Vector2(-directionToPlayer.y, directionToPlayer.x) * strafeDirection;
            moveDirection += strafeDirectionVector * strafeSpeedMultiplier;
        }

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        moveDirection.Normalize();

        Vector2 newPosition = shielderPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        RotateTowardDirection(moveDirection);
    }

    private void PerformShieldPulse()
    {
        FindShieldTargets();

        int shieldedCount = 0;

        for (int i = 0; i < shieldTargets.Count; i++)
        {
            Health targetHealth = shieldTargets[i];

            if (targetHealth == null)
            {
                continue;
            }

            if (targetHealth.IsDead)
            {
                continue;
            }

            if (!canShieldSelf && targetHealth == ownHealth)
            {
                continue;
            }

            DamageReductionStatus status = targetHealth.GetComponent<DamageReductionStatus>();

            if (status == null)
            {
                status = targetHealth.gameObject.AddComponent<DamageReductionStatus>();
            }

            status.ApplyShield(
                damageReductionPercent,
                shieldDuration,
                shieldRingColor,
                shieldedEnemyRingRadius,
                shieldedEnemyRingWidth,
                shieldedEnemySortingOrder
            );

            shieldedCount++;

            if (shieldedCount >= maxEnemiesShieldedPerPulse)
            {
                break;
            }
        }

        PlayAuraVisual(shieldedCount > 0);
    }

    private void FindShieldTargets()
    {
        shieldTargets.Clear();
        uniqueShieldTargets.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            shieldRadius,
            shieldTargetMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!hitCollider.CompareTag(enemyTag))
            {
                continue;
            }

            Health targetHealth = hitCollider.GetComponent<Health>();

            if (targetHealth == null)
            {
                targetHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (targetHealth == null)
            {
                continue;
            }

            if (!canShieldSelf && targetHealth == ownHealth)
            {
                continue;
            }

            if (targetHealth.IsDead)
            {
                continue;
            }

            if (uniqueShieldTargets.Contains(targetHealth))
            {
                continue;
            }

            uniqueShieldTargets.Add(targetHealth);
            shieldTargets.Add(targetHealth);
        }

        shieldTargets.Sort((a, b) =>
        {
            if (a == null || b == null)
            {
                return 0;
            }

            float distanceA = (a.transform.position - transform.position).sqrMagnitude;
            float distanceB = (b.transform.position - transform.position).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });
    }

    private void PlayAuraVisual(bool shieldedSomething)
    {
        if (auraRoutine != null)
        {
            StopCoroutine(auraRoutine);
        }

        auraRoutine = StartCoroutine(AuraVisualRoutine(shieldedSomething));

        if (flashWhenShielding && shieldedSomething)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator AuraVisualRoutine(bool shieldedSomething)
    {
        if (!showAuraPulse)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < auraVisualDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / auraVisualDuration);
            float radius = Mathf.Lerp(shieldRadius * 0.55f, shieldRadius, t);

            Color color = shieldedSomething ? shieldRingColor : auraColor;
            color.a = Mathf.Lerp(color.a, 0f, t);

            ShowAuraRing(color, auraLineWidth, radius);

            yield return null;
        }

        HideAuraRing();
        auraRoutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        SetSpriteColors(shieldFlashColor);

        yield return new WaitForSeconds(flashDuration);

        RestoreSpriteColors();
        flashRoutine = null;
    }

    private void BuildAuraRing()
    {
        if (!showAuraPulse)
        {
            return;
        }

        if (auraRenderer != null)
        {
            return;
        }

        GameObject ringObject = new GameObject("Shielder Aura Pulse Ring");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;
        ringObject.transform.localRotation = Quaternion.identity;
        ringObject.transform.localScale = Vector3.one;

        auraRenderer = ringObject.AddComponent<LineRenderer>();
        auraRenderer.useWorldSpace = true;
        auraRenderer.loop = true;
        auraRenderer.positionCount = Mathf.Max(8, auraSegments);
        auraRenderer.startWidth = auraLineWidth;
        auraRenderer.endWidth = auraLineWidth;
        auraRenderer.startColor = auraColor;
        auraRenderer.endColor = auraColor;
        auraRenderer.sortingOrder = auraSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            auraRenderer.material = new Material(spriteShader);
        }

        HideAuraRing();
    }

    private void ShowAuraRing(Color color, float width, float radius)
    {
        if (!showAuraPulse)
        {
            return;
        }

        if (auraRenderer == null)
        {
            BuildAuraRing();
        }

        if (auraRenderer == null)
        {
            return;
        }

        auraRenderer.gameObject.SetActive(true);
        auraRenderer.startColor = color;
        auraRenderer.endColor = color;
        auraRenderer.startWidth = width;
        auraRenderer.endWidth = width;

        int safeSegments = Mathf.Max(8, auraSegments);
        auraRenderer.positionCount = safeSegments;

        for (int i = 0; i < safeSegments; i++)
        {
            float percent = (float)i / safeSegments;
            float angle = percent * Mathf.PI * 2f;

            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            auraRenderer.SetPosition(i, point);
        }
    }

    private void HideAuraRing()
    {
        if (auraRenderer != null)
        {
            auraRenderer.gameObject.SetActive(false);
        }
    }

    private void RotateTowardDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTarget != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
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

    private void SetSpriteColors(Color color)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = color;
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
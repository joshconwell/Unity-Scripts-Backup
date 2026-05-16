using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Health))]
public class EnemyHealerController2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.1f;

    [Tooltip("If the healer is closer than this, it backs away from the player.")]
    [SerializeField] private float fleeDistance = 5f;

    [Tooltip("If the healer is farther than this, it moves toward the player/fight.")]
    [SerializeField] private float followDistance = 9f;

    [Tooltip("Small side movement so the healer does not just sit perfectly still.")]
    [SerializeField] private bool useStrafing = true;

    [SerializeField] private float strafeSpeedMultiplier = 0.45f;
    [SerializeField] private float strafeDirectionChangeInterval = 2.25f;

    [Header("Healing")]
    [SerializeField] private float healRadius = 5f;
    [SerializeField] private float healAmount = 8f;
    [SerializeField] private float healInterval = 3.5f;

    [Tooltip("Maximum enemies healed per pulse. Set high if you want no practical limit.")]
    [SerializeField] private int maxEnemiesHealedPerPulse = 6;

    [SerializeField] private bool canHealSelf = false;

    [Tooltip("Leave as Everything if you are unsure.")]
    [SerializeField] private LayerMask healTargetMask = ~0;

    [Header("Heal Visual Ring")]
    [SerializeField] private bool showHealRing = true;
    [SerializeField] private Color healRingColor = new Color(0.25f, 1f, 0.45f, 0.75f);
    [SerializeField] private Color healPulseColor = new Color(0.75f, 1f, 0.75f, 1f);
    [SerializeField] private float ringLineWidth = 0.08f;
    [SerializeField] private int ringSegments = 72;
    [SerializeField] private int ringSortingOrder = 32;
    [SerializeField] private float pulseVisualDuration = 0.28f;

    [Header("Sprite Flash")]
    [SerializeField] private bool flashWhenHealing = true;
    [SerializeField] private Color healFlashColor = new Color(0.35f, 1f, 0.45f, 1f);
    [SerializeField] private float flashDuration = 0.18f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugHealKey = false;
    [SerializeField] private KeyCode debugHealKey = KeyCode.F3;

    private Rigidbody2D rb;
    private Health ownHealth;
    private LineRenderer healRingRenderer;

    private readonly List<Health> healTargets = new List<Health>();
    private readonly HashSet<Health> uniqueHealTargets = new HashSet<Health>();

    private float nextHealTime;
    private float nextStrafeChangeTime;
    private int strafeDirection = 1;

    private Color[] originalSpriteColors;
    private Coroutine healVisualRoutine;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ownHealth = GetComponent<Health>();

        AutoFindSpriteRenderers();
        CacheOriginalSpriteColors();
        BuildHealRing();
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();

        nextHealTime = Time.time + Random.Range(0.75f, healInterval);
        nextStrafeChangeTime = Time.time + Random.Range(0.25f, strafeDirectionChangeInterval);
        strafeDirection = Random.value < 0.5f ? -1 : 1;

        RestoreSpriteColors();
        HideHealRing();
    }

    private void OnDisable()
    {
        if (healVisualRoutine != null)
        {
            StopCoroutine(healVisualRoutine);
            healVisualRoutine = null;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        RestoreSpriteColors();
        HideHealRing();
    }

    private void Update()
    {
        if (allowDebugHealKey && Input.GetKeyDown(debugHealKey))
        {
            PerformHealPulse();
        }

        if (ownHealth != null && ownHealth.IsDead)
        {
            return;
        }

        if (Time.time >= nextHealTime)
        {
            PerformHealPulse();
            nextHealTime = Time.time + healInterval;
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
        Vector2 healerPosition = rb.position;
        Vector2 playerPosition = playerTarget.position;
        Vector2 toPlayer = playerPosition - healerPosition;

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

        Vector2 newPosition = healerPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        RotateTowardDirection(moveDirection);
    }

    private void PerformHealPulse()
    {
        FindHealTargets();

        if (healTargets.Count <= 0)
        {
            if (showHealRing)
            {
                PlayHealVisual(false);
            }

            return;
        }

        int healedCount = 0;

        for (int i = 0; i < healTargets.Count; i++)
        {
            Health targetHealth = healTargets[i];

            if (targetHealth == null)
            {
                continue;
            }

            if (targetHealth.IsDead)
            {
                continue;
            }

            if (!canHealSelf && targetHealth == ownHealth)
            {
                continue;
            }

            if (targetHealth.CurrentHealth >= targetHealth.MaxHealth)
            {
                continue;
            }

            targetHealth.Heal(healAmount);
            healedCount++;

            if (healedCount >= maxEnemiesHealedPerPulse)
            {
                break;
            }
        }

        PlayHealVisual(healedCount > 0);
    }

    private void FindHealTargets()
    {
        healTargets.Clear();
        uniqueHealTargets.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            healRadius,
            healTargetMask
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

            if (!canHealSelf && targetHealth == ownHealth)
            {
                continue;
            }

            if (targetHealth.IsDead)
            {
                continue;
            }

            if (targetHealth.CurrentHealth >= targetHealth.MaxHealth)
            {
                continue;
            }

            if (uniqueHealTargets.Contains(targetHealth))
            {
                continue;
            }

            uniqueHealTargets.Add(targetHealth);
            healTargets.Add(targetHealth);
        }

        healTargets.Sort((a, b) =>
        {
            if (a == null || b == null)
            {
                return 0;
            }

            float aPercent = a.CurrentHealth / Mathf.Max(1f, a.MaxHealth);
            float bPercent = b.CurrentHealth / Mathf.Max(1f, b.MaxHealth);

            return aPercent.CompareTo(bPercent);
        });
    }

    private void PlayHealVisual(bool healedSomething)
    {
        if (healVisualRoutine != null)
        {
            StopCoroutine(healVisualRoutine);
        }

        healVisualRoutine = StartCoroutine(HealVisualRoutine(healedSomething));

        if (flashWhenHealing && healedSomething)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator HealVisualRoutine(bool healedSomething)
    {
        if (!showHealRing)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < pulseVisualDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / pulseVisualDuration);
            float radius = Mathf.Lerp(healRadius * 0.65f, healRadius, t);

            Color color = healedSomething ? healPulseColor : healRingColor;
            color.a = Mathf.Lerp(color.a, 0f, t);

            ShowHealRing(color, ringLineWidth, radius);

            yield return null;
        }

        HideHealRing();
        healVisualRoutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        SetSpriteColors(healFlashColor);

        yield return new WaitForSeconds(flashDuration);

        RestoreSpriteColors();
        flashRoutine = null;
    }

    private void BuildHealRing()
    {
        if (!showHealRing)
        {
            return;
        }

        if (healRingRenderer != null)
        {
            return;
        }

        GameObject ringObject = new GameObject("Healer Pulse Ring");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;
        ringObject.transform.localRotation = Quaternion.identity;
        ringObject.transform.localScale = Vector3.one;

        healRingRenderer = ringObject.AddComponent<LineRenderer>();
        healRingRenderer.useWorldSpace = true;
        healRingRenderer.loop = true;
        healRingRenderer.positionCount = Mathf.Max(8, ringSegments);
        healRingRenderer.startWidth = ringLineWidth;
        healRingRenderer.endWidth = ringLineWidth;
        healRingRenderer.startColor = healRingColor;
        healRingRenderer.endColor = healRingColor;
        healRingRenderer.sortingOrder = ringSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            healRingRenderer.material = new Material(spriteShader);
        }

        HideHealRing();
    }

    private void ShowHealRing(Color color, float width, float radius)
    {
        if (!showHealRing)
        {
            return;
        }

        if (healRingRenderer == null)
        {
            BuildHealRing();
        }

        if (healRingRenderer == null)
        {
            return;
        }

        healRingRenderer.gameObject.SetActive(true);
        healRingRenderer.startColor = color;
        healRingRenderer.endColor = color;
        healRingRenderer.startWidth = width;
        healRingRenderer.endWidth = width;

        int safeSegments = Mathf.Max(8, ringSegments);
        healRingRenderer.positionCount = safeSegments;

        for (int i = 0; i < safeSegments; i++)
        {
            float t = (float)i / safeSegments;
            float angle = t * Mathf.PI * 2f;

            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            healRingRenderer.SetPosition(i, point);
        }
    }

    private void HideHealRing()
    {
        if (healRingRenderer != null)
        {
            healRingRenderer.gameObject.SetActive(false);
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
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Health))]
public class EnemySniperController2D : MonoBehaviour
{
    private enum SniperState
    {
        Moving,
        Aiming,
        Firing,
        Recovering
    }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.85f;
    [SerializeField] private float preferredDistance = 11f;
    [SerializeField] private float distanceTolerance = 1.5f;
    [SerializeField] private bool useStrafing = true;
    [SerializeField] private float strafeSpeedMultiplier = 0.35f;
    [SerializeField] private float strafeDirectionChangeInterval = 2.25f;
    [SerializeField] private bool rotateTowardAim = true;

    [Header("Sniper Attack")]
    [SerializeField] private float attackRange = 18f;
    [SerializeField] private float firstShotDelay = 1.75f;
    [SerializeField] private float shotCooldown = 4.25f;
    [SerializeField] private float aimDuration = 1.15f;

    [Tooltip("The aim tracks the player until this much time is left, then locks in.")]
    [SerializeField] private float lockAimWhenTimeRemaining = 0.25f;

    [SerializeField] private float shotDamage = 18f;
    [SerializeField] private float shotLength = 26f;
    [SerializeField] private float shotHitRadius = 0.24f;

    [Tooltip("Leave as Everything if unsure.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Line Visual")]
    [SerializeField] private bool showAimLine = true;
    [SerializeField] private Color aimLineColor = new Color(1f, 0.1f, 0.05f, 0.65f);
    [SerializeField] private Color lockedAimLineColor = new Color(1f, 0.8f, 0.05f, 0.8f);
    [SerializeField] private Color shotLineColor = new Color(1f, 0.95f, 0.25f, 1f);
    [SerializeField] private float aimLineWidth = 0.08f;
    [SerializeField] private float shotLineWidth = 0.22f;
    [SerializeField] private float shotVisualDuration = 0.12f;
    [SerializeField] private int lineSortingOrder = 34;

    [Header("Sprite Flash")]
    [SerializeField] private bool flashWhileAiming = true;
    [SerializeField] private Color aimingFlashColor = new Color(1f, 0.2f, 0.12f, 1f);
    [SerializeField] private Color firingFlashColor = new Color(1f, 0.9f, 0.25f, 1f);
    [SerializeField] private float flashSpeed = 10f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugShotKey = false;
    [SerializeField] private KeyCode debugShotKey = KeyCode.F1;

    private Rigidbody2D rb;
    private Health health;
    private LineRenderer aimLine;

    private SniperState state = SniperState.Moving;

    private Vector2 aimDirection = Vector2.right;
    private float stateTimer;
    private float nextShotReadyTime;
    private float nextStrafeChangeTime;
    private int strafeDirection = 1;

    private Color[] originalSpriteColors;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        AutoFindSpriteRenderers();
        CacheOriginalSpriteColors();
        BuildAimLine();
    }

    private void OnEnable()
    {
        FindTargetIfNeeded();

        state = SniperState.Moving;
        stateTimer = 0f;
        nextShotReadyTime = Time.time + firstShotDelay + Random.Range(0f, 0.75f);
        nextStrafeChangeTime = Time.time + Random.Range(0.25f, strafeDirectionChangeInterval);
        strafeDirection = Random.value < 0.5f ? -1 : 1;

        RestoreSpriteColors();
        HideAimLine();
    }

    private void OnDisable()
    {
        RestoreSpriteColors();
        HideAimLine();
    }

    private void Update()
    {
        if (allowDebugShotKey && Input.GetKeyDown(debugShotKey))
        {
            TryBeginAiming();
        }

        UpdateStateTimers();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            HideAimLine();
            return;
        }

        FindTargetIfNeeded();

        if (target == null)
        {
            return;
        }

        if (state == SniperState.Moving)
        {
            HandleMovement();
        }
    }

    private void UpdateStateTimers()
    {
        if (health != null && health.IsDead)
        {
            return;
        }

        if (target == null)
        {
            FindTargetIfNeeded();
            return;
        }

        switch (state)
        {
            case SniperState.Moving:
                if (CanShoot())
                {
                    TryBeginAiming();
                }
                break;

            case SniperState.Aiming:
                HandleAimingUpdate();
                break;

            case SniperState.Firing:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                {
                    BeginRecovery();
                }
                break;

            case SniperState.Recovering:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                {
                    state = SniperState.Moving;
                    nextShotReadyTime = Time.time + shotCooldown;
                    RestoreSpriteColors();
                    HideAimLine();
                }
                break;
        }
    }

    private void HandleMovement()
    {
        Vector2 enemyPosition = rb.position;
        Vector2 targetPosition = target.position;
        Vector2 toTarget = targetPosition - enemyPosition;

        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= 0.001f)
        {
            return;
        }

        Vector2 directionToTarget = toTarget.normalized;
        Vector2 moveDirection = Vector2.zero;

        if (distanceToTarget < preferredDistance - distanceTolerance)
        {
            moveDirection = -directionToTarget;
        }
        else if (distanceToTarget > preferredDistance + distanceTolerance)
        {
            moveDirection = directionToTarget;
        }

        if (useStrafing)
        {
            if (Time.time >= nextStrafeChangeTime)
            {
                strafeDirection *= -1;
                nextStrafeChangeTime = Time.time + strafeDirectionChangeInterval + Random.Range(-0.35f, 0.35f);
            }

            Vector2 strafeVector = new Vector2(-directionToTarget.y, directionToTarget.x) * strafeDirection;
            moveDirection += strafeVector * strafeSpeedMultiplier;
        }

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            moveDirection.Normalize();

            Vector2 newPosition = enemyPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }

        aimDirection = directionToTarget;

        if (rotateTowardAim)
        {
            RotateTowardDirection(aimDirection);
        }

        HideAimLine();
    }

    private bool CanShoot()
    {
        if (Time.time < nextShotReadyTime)
        {
            return false;
        }

        if (target == null)
        {
            return false;
        }

        float distanceSquared = (target.position - transform.position).sqrMagnitude;
        return distanceSquared <= attackRange * attackRange;
    }

    private void TryBeginAiming()
    {
        if (state != SniperState.Moving)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        aimDirection = GetDirectionToTarget();

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            aimDirection = transform.right;
        }

        aimDirection.Normalize();

        state = SniperState.Aiming;
        stateTimer = aimDuration;

        rb.linearVelocity = Vector2.zero;

        UpdateAimLine(aimLineColor, aimLineWidth);

        if (rotateTowardAim)
        {
            RotateTowardDirection(aimDirection);
        }
    }

    private void HandleAimingUpdate()
    {
        stateTimer -= Time.deltaTime;

        bool aimLocked = stateTimer <= lockAimWhenTimeRemaining;

        if (!aimLocked)
        {
            aimDirection = GetDirectionToTarget();
        }

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            aimDirection = transform.right;
        }

        aimDirection.Normalize();

        if (rotateTowardAim)
        {
            RotateTowardDirection(aimDirection);
        }

        UpdateAimLine(
            aimLocked ? lockedAimLineColor : aimLineColor,
            aimLocked ? aimLineWidth * 1.35f : aimLineWidth
        );

        if (stateTimer <= 0f)
        {
            FireShot();
        }
    }

    private void FireShot()
    {
        state = SniperState.Firing;
        stateTimer = shotVisualDuration;

        DamagePlayerAlongShot();

        UpdateAimLine(shotLineColor, shotLineWidth);
        SetSpriteColors(firingFlashColor);
    }

    private void BeginRecovery()
    {
        state = SniperState.Recovering;
        stateTimer = 0.25f;

        HideAimLine();
        RestoreSpriteColors();
    }

    private void DamagePlayerAlongShot()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position,
            shotHitRadius,
            aimDirection.normalized,
            shotLength,
            hitMask
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;

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

            playerHealth.TakeDamage(shotDamage);
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

    private Vector2 GetDirectionToTarget()
    {
        if (target == null)
        {
            return Vector2.right;
        }

        Vector2 direction = target.position - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return Vector2.right;
        }

        return direction.normalized;
    }

    private void RotateTowardDirection(Vector2 direction)
    {
        if (!rotateTowardAim)
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

    private void BuildAimLine()
    {
        if (!showAimLine)
        {
            return;
        }

        if (aimLine != null)
        {
            return;
        }

        GameObject lineObject = new GameObject("Sniper Aim Line");
        lineObject.transform.SetParent(transform, false);
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localRotation = Quaternion.identity;
        lineObject.transform.localScale = Vector3.one;

        aimLine = lineObject.AddComponent<LineRenderer>();
        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;
        aimLine.startWidth = aimLineWidth;
        aimLine.endWidth = aimLineWidth;
        aimLine.startColor = aimLineColor;
        aimLine.endColor = aimLineColor;
        aimLine.sortingOrder = lineSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            aimLine.material = new Material(spriteShader);
        }

        HideAimLine();
    }

    private void UpdateAimLine(Color color, float width)
    {
        if (!showAimLine)
        {
            return;
        }

        if (aimLine == null)
        {
            BuildAimLine();
        }

        if (aimLine == null)
        {
            return;
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(aimDirection.x, aimDirection.y, 0f) * shotLength;

        aimLine.gameObject.SetActive(true);
        aimLine.SetPosition(0, startPosition);
        aimLine.SetPosition(1, endPosition);
        aimLine.startColor = color;
        aimLine.endColor = color;
        aimLine.startWidth = width;
        aimLine.endWidth = width;
    }

    private void HideAimLine()
    {
        if (aimLine != null)
        {
            aimLine.gameObject.SetActive(false);
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

    private void UpdateVisuals()
    {
        if (state != SniperState.Aiming)
        {
            return;
        }

        if (!flashWhileAiming)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        Color finalColor = Color.Lerp(Color.white, aimingFlashColor, pulse);
        SetSpriteColors(finalColor);
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
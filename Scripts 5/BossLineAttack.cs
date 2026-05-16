using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossLineAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float activationRange = 22f;

    [Header("Attack Timing")]
    [SerializeField] private float firstAttackDelay = 2f;
    [SerializeField] private float attackInterval = 4f;
    [SerializeField] private float warningDuration = 0.75f;
    [SerializeField] private float beamDuration = 0.12f;
    [SerializeField] private float delayBetweenBursts = 0.35f;

    [Header("Beam Pattern")]
    [SerializeField] private int burstsPerAttack = 3;

    [Tooltip("1 = one aimed beam. 3 = a spread of three beams.")]
    [SerializeField] private int beamsPerBurst = 1;

    [Tooltip("Only matters when Beams Per Burst is higher than 1.")]
    [SerializeField] private float spreadAngle = 35f;

    [SerializeField] private float beamLength = 30f;

    [Tooltip("How wide the beam hit detection is.")]
    [SerializeField] private float beamHitRadius = 0.25f;

    [Header("Damage")]
    [SerializeField] private float beamDamage = 18f;

    [Tooltip("Leave as Everything if you are not sure.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visuals")]
    [SerializeField] private Color warningLineColor = new Color(1f, 0.25f, 0.15f, 0.55f);
    [SerializeField] private Color beamLineColor = new Color(1f, 0.9f, 0.35f, 1f);
    [SerializeField] private float warningLineWidth = 0.12f;
    [SerializeField] private float beamLineWidth = 0.32f;
    [SerializeField] private int lineSortingOrder = 40;

    [Header("Warning Flash")]
    [SerializeField] private bool flashBeforeAttack = true;
    [SerializeField] private Color warningFlashColor = Color.red;
    [SerializeField] private float flashInterval = 0.08f;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Debug")]
    [SerializeField] private bool allowDebugAttackKey = true;
    [SerializeField] private KeyCode debugAttackKey = KeyCode.F6;

    private Health health;
    private Transform player;
    private Coroutine attackRoutine;
    private Color[] originalColors;

    private bool isDead;
    private bool isAttacking;

    private readonly List<GameObject> activeLineObjects = new List<GameObject>();
    private readonly HashSet<Health> damagedThisBeam = new HashSet<Health>();

    private void Awake()
    {
        health = GetComponent<Health>();

        AutoFindSpriteRenderers();
        CacheOriginalColors();
    }

    private void OnEnable()
    {
        isDead = false;
        isAttacking = false;

        FindPlayerIfNeeded();
        RestoreOriginalColors();
        ClearActiveLines();

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
        RestoreOriginalColors();
        ClearActiveLines();
    }

    private void Update()
    {
        if (allowDebugAttackKey && Input.GetKeyDown(debugAttackKey))
        {
            StartCoroutine(PerformLineAttack());
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
                yield return PerformLineAttack();
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator PerformLineAttack()
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

        int safeBurstCount = Mathf.Max(1, burstsPerAttack);

        for (int burstIndex = 0; burstIndex < safeBurstCount; burstIndex++)
        {
            if (isDead)
            {
                break;
            }

            Vector3 startPosition = GetFirePosition();
            Vector2 aimDirection = GetDirectionToPlayer(startPosition);

            if (aimDirection.sqrMagnitude <= 0.001f)
            {
                aimDirection = Vector2.right;
            }

            List<GameObject> burstLines = CreateWarningLines(startPosition, aimDirection);

            if (flashBeforeAttack)
            {
                StartCoroutine(WarningFlashRoutine(warningDuration));
            }

            if (warningDuration > 0f)
            {
                yield return new WaitForSeconds(warningDuration);
            }

            FireBurstLines(burstLines, startPosition, aimDirection);

            if (beamDuration > 0f)
            {
                yield return new WaitForSeconds(beamDuration);
            }

            DestroyLines(burstLines);

            if (burstIndex < safeBurstCount - 1 && delayBetweenBursts > 0f)
            {
                yield return new WaitForSeconds(delayBetweenBursts);
            }
        }

        RestoreOriginalColors();
        isAttacking = false;
    }

    private List<GameObject> CreateWarningLines(Vector3 startPosition, Vector2 centerDirection)
    {
        List<GameObject> lineObjects = new List<GameObject>();

        List<Vector2> directions = GetBeamDirections(centerDirection);

        for (int i = 0; i < directions.Count; i++)
        {
            Vector2 direction = directions[i];
            Vector3 endPosition = startPosition + new Vector3(direction.x, direction.y, 0f) * beamLength;

            GameObject lineObject = CreateLineObject(
                "Boss Beam Warning",
                startPosition,
                endPosition,
                warningLineColor,
                warningLineWidth
            );

            if (lineObject != null)
            {
                lineObjects.Add(lineObject);
            }
        }

        return lineObjects;
    }

    private void FireBurstLines(List<GameObject> burstLines, Vector3 startPosition, Vector2 centerDirection)
    {
        List<Vector2> directions = GetBeamDirections(centerDirection);

        for (int i = 0; i < directions.Count; i++)
        {
            Vector2 direction = directions[i];

            if (i < burstLines.Count && burstLines[i] != null)
            {
                SetLineVisual(burstLines[i], beamLineColor, beamLineWidth);
            }

            DamageAlongBeam(startPosition, direction);
        }
    }

    private List<Vector2> GetBeamDirections(Vector2 centerDirection)
    {
        List<Vector2> directions = new List<Vector2>();

        int safeBeamCount = Mathf.Max(1, beamsPerBurst);

        if (safeBeamCount == 1)
        {
            directions.Add(centerDirection.normalized);
            return directions;
        }

        float totalSpread = Mathf.Max(0f, spreadAngle);
        float angleStep = totalSpread / (safeBeamCount - 1);
        float startingAngle = -totalSpread * 0.5f;

        for (int i = 0; i < safeBeamCount; i++)
        {
            float angleOffset = startingAngle + angleStep * i;
            directions.Add(RotateDirection(centerDirection, angleOffset));
        }

        return directions;
    }

    private void DamageAlongBeam(Vector3 startPosition, Vector2 direction)
    {
        damagedThisBeam.Clear();

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            startPosition,
            beamHitRadius,
            direction.normalized,
            beamLength,
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

            if (damagedThisBeam.Contains(playerHealth))
            {
                continue;
            }

            damagedThisBeam.Add(playerHealth);
            playerHealth.TakeDamage(beamDamage);
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

    private GameObject CreateLineObject(string objectName, Vector3 startPosition, Vector3 endPosition, Color color, float width)
    {
        GameObject lineObject = new GameObject(objectName);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.sortingOrder = lineSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            lineRenderer.material = new Material(spriteShader);
        }

        activeLineObjects.Add(lineObject);

        return lineObject;
    }

    private void SetLineVisual(GameObject lineObject, Color color, float width)
    {
        if (lineObject == null)
        {
            return;
        }

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    private void DestroyLines(List<GameObject> lineObjects)
    {
        if (lineObjects == null)
        {
            return;
        }

        for (int i = 0; i < lineObjects.Count; i++)
        {
            GameObject lineObject = lineObjects[i];

            if (lineObject == null)
            {
                continue;
            }

            activeLineObjects.Remove(lineObject);
            Destroy(lineObject);
        }

        lineObjects.Clear();
    }

    private void ClearActiveLines()
    {
        for (int i = activeLineObjects.Count - 1; i >= 0; i--)
        {
            if (activeLineObjects[i] != null)
            {
                Destroy(activeLineObjects[i]);
            }
        }

        activeLineObjects.Clear();
    }

    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position;
    }

    private Vector2 GetDirectionToPlayer(Vector3 startPosition)
    {
        if (player == null)
        {
            return Vector2.right;
        }

        Vector3 directionToPlayer = player.position - startPosition;

        return new Vector2(directionToPlayer.x, directionToPlayer.y).normalized;
    }

    private Vector2 RotateDirection(Vector2 direction, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
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
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private IEnumerator WarningFlashRoutine(float duration)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            yield break;
        }

        float elapsedTime = 0f;
        bool useWarningColor = true;

        while (elapsedTime < duration && !isDead)
        {
            SetRendererColors(useWarningColor ? warningFlashColor : Color.white, useWarningColor);

            useWarningColor = !useWarningColor;

            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        RestoreOriginalColors();
    }

    private void HandleDied()
    {
        isDead = true;
        isAttacking = false;

        StopAttackRoutine();
        RestoreOriginalColors();
        ClearActiveLines();
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

    private void CacheOriginalColors()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
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
            else if (originalColors != null && i < originalColors.Length)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < originalColors.Length)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
}
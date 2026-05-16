using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RotatingLaserBeam : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool drawHitDebug = false;

    private Transform owner;
    private int beamIndex;
    private int totalBeams = 1;

    private float damage = 9f;
    private float beamLength = 6.5f;
    private float rotationSpeed = 105f;
    private float hitWidth = 0.34f;
    private float hitCooldown = 0.22f;
    private LayerMask hitMask = ~0;

    private Color beamColor = new Color(1f, 0.1f, 0.08f, 0.9f);
    private Color coreColor = new Color(1f, 0.75f, 0.3f, 0.95f);
    private float lineWidth = 0.18f;
    private float coreRadius = 0.25f;

    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private float angleOffset;

    private readonly Dictionary<Health, float> lastHitTimesByEnemy = new Dictionary<Health, float>();
    private readonly List<Health> removalBuffer = new List<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        lastHitTimesByEnemy.Clear();
    }

    private void OnDisable()
    {
        lastHitTimesByEnemy.Clear();
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
        Transform newOwner,
        int newBeamIndex,
        int newTotalBeams,
        float newDamage,
        float newBeamLength,
        float newRotationSpeed,
        float newHitWidth,
        float newHitCooldown,
        LayerMask newHitMask,
        Color newBeamColor,
        Color newCoreColor,
        float newLineWidth,
        float newCoreRadius)
    {
        owner = newOwner;
        beamIndex = Mathf.Max(0, newBeamIndex);
        totalBeams = Mathf.Max(1, newTotalBeams);

        damage = Mathf.Max(0f, newDamage);
        beamLength = Mathf.Max(0.25f, newBeamLength);
        rotationSpeed = newRotationSpeed;
        hitWidth = Mathf.Max(0.05f, newHitWidth);
        hitCooldown = Mathf.Max(0.03f, newHitCooldown);
        hitMask = newHitMask;

        beamColor = newBeamColor;
        coreColor = newCoreColor;
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        coreRadius = Mathf.Max(0.01f, newCoreRadius);

        angleOffset = (360f / totalBeams) * beamIndex;

        SetupLineRenderer();
        RefreshVisual();

        if (owner != null)
        {
            transform.position = owner.position;
        }
    }

    private void Update()
    {
        if (owner == null)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        transform.position = owner.position;

        float angle = Time.time * rotationSpeed + angleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        RefreshVisual();
        DamageEnemiesInsideBeam();
        CleanupDeadCooldownEntries();
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
        lineRenderer.loop = false;
        lineRenderer.positionCount = 3;
        lineRenderer.numCapVertices = 8;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.sortingOrder = 45;

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

    private void RefreshVisual()
    {
        if (lineRenderer == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * 20f + beamIndex) * 0.08f;
        float currentWidth = lineWidth * pulse;

        lineRenderer.startWidth = currentWidth * 1.3f;
        lineRenderer.endWidth = currentWidth;
        lineRenderer.startColor = coreColor;
        lineRenderer.endColor = beamColor;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, new Vector3(coreRadius, 0f, 0f));
        lineRenderer.SetPosition(2, new Vector3(beamLength, 0f, 0f));
    }

    private void DamageEnemiesInsideBeam()
    {
        if (damage <= 0f)
        {
            return;
        }

        Vector2 start = transform.position;
        Vector2 end = transform.position + transform.right * beamLength;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(start, beamLength + hitWidth, hitMask);

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

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            if (!IsColliderCloseEnoughToBeam(hitCollider, start, end))
            {
                continue;
            }

            if (!CanHitEnemy(enemyHealth))
            {
                continue;
            }

            lastHitTimesByEnemy[enemyHealth] = Time.time;
            enemyHealth.TakeDamage(damage, false);
        }

        if (drawHitDebug)
        {
            Debug.DrawLine(start, end, Color.red, 0.02f);
        }
    }

    private bool IsColliderCloseEnoughToBeam(Collider2D hitCollider, Vector2 beamStart, Vector2 beamEnd)
    {
        Vector2 colliderCenter = hitCollider.bounds.center;
        Vector2 closestPointOnBeam = GetClosestPointOnSegment(beamStart, beamEnd, colliderCenter);
        Vector2 closestPointOnCollider = hitCollider.ClosestPoint(closestPointOnBeam);

        float distanceSquared = (closestPointOnCollider - closestPointOnBeam).sqrMagnitude;
        return distanceSquared <= hitWidth * hitWidth;
    }

    private Vector2 GetClosestPointOnSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared <= 0.0001f)
        {
            return segmentStart;
        }

        float t = Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared;
        t = Mathf.Clamp01(t);

        return segmentStart + segment * t;
    }

    private bool CanHitEnemy(Health enemyHealth)
    {
        if (enemyHealth == null)
        {
            return false;
        }

        float lastHitTime;

        if (!lastHitTimesByEnemy.TryGetValue(enemyHealth, out lastHitTime))
        {
            return true;
        }

        return Time.time - lastHitTime >= hitCooldown;
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

    private void CleanupDeadCooldownEntries()
    {
        removalBuffer.Clear();

        foreach (KeyValuePair<Health, float> entry in lastHitTimesByEnemy)
        {
            if (entry.Key == null || entry.Key.IsDead)
            {
                removalBuffer.Add(entry.Key);
            }
        }

        for (int i = 0; i < removalBuffer.Count; i++)
        {
            lastHitTimesByEnemy.Remove(removalBuffer[i]);
        }
    }
}

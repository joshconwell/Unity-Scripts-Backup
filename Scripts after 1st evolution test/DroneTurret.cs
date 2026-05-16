using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DroneTurret : MonoBehaviour
{
    private Transform owner;
    private int droneIndex;
    private int droneCount = 1;

    private float orbitRadius = 1.75f;
    private float orbitSpeed = 120f;
    private float followSharpness = 18f;
    private float damage = 12f;
    private float fireCooldown = 0.55f;
    private float range = 10f;
    private LayerMask enemyHitMask = ~0;

    private Color bodyColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    private Color beamColor = new Color(1f, 0.85f, 0.2f, 0.85f);
    private float bodyRadius = 0.28f;
    private float bodyLineWidth = 0.08f;
    private float beamWidth = 0.08f;
    private int circleSegments = 20;

    private float fireTimer;
    private float beamTimer;
    private float beamVisibleTime = 0.06f;

    private LineRenderer bodyLine;
    private LineRenderer beamLine;
    private Material lineMaterial;

    private void Awake()
    {
        SetupVisuals();
    }

    private void OnEnable()
    {
        fireTimer = Random.Range(0f, 0.2f);
        beamTimer = 0f;

        if (beamLine != null)
        {
            beamLine.enabled = false;
        }
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
        int newDroneIndex,
        int newDroneCount,
        float newOrbitRadius,
        float newOrbitSpeed,
        float newFollowSharpness,
        float newDamage,
        float newFireCooldown,
        float newRange,
        LayerMask newEnemyHitMask,
        Color newBodyColor,
        Color newBeamColor,
        float newBodyRadius,
        float newBodyLineWidth,
        float newBeamWidth)
    {
        owner = newOwner;
        droneIndex = Mathf.Max(0, newDroneIndex);
        droneCount = Mathf.Max(1, newDroneCount);

        orbitRadius = Mathf.Max(0.25f, newOrbitRadius);
        orbitSpeed = newOrbitSpeed;
        followSharpness = Mathf.Max(1f, newFollowSharpness);
        damage = Mathf.Max(0f, newDamage);
        fireCooldown = Mathf.Max(0.05f, newFireCooldown);
        range = Mathf.Max(0.5f, newRange);
        enemyHitMask = newEnemyHitMask;

        bodyColor = newBodyColor;
        beamColor = newBeamColor;
        bodyRadius = Mathf.Max(0.05f, newBodyRadius);
        bodyLineWidth = Mathf.Max(0.01f, newBodyLineWidth);
        beamWidth = Mathf.Max(0.01f, newBeamWidth);

        SetupVisuals();
        DrawBodyCircle();
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        UpdatePosition();
        UpdateBeamVisibility();
        HandleShooting();
    }

    private void UpdatePosition()
    {
        float spacingAngle = 360f / Mathf.Max(1, droneCount);
        float angle = Time.time * orbitSpeed + spacingAngle * droneIndex;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f
        ) * orbitRadius;

        Vector3 targetPosition = owner.position + offset;
        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
    }

    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
        {
            return;
        }

        Health target = FindNearestEnemy();

        if (target == null)
        {
            fireTimer = 0.1f;
            return;
        }

        target.TakeDamage(damage, false);
        ShowBeam(target.transform.position);

        fireTimer = fireCooldown;
    }

    private Health FindNearestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            range,
            enemyHitMask
        );

        Health bestTarget = null;
        float bestDistanceSquared = range * range;

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

            float distanceSquared = (enemyHealth.transform.position - transform.position).sqrMagnitude;

            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                bestTarget = enemyHealth;
            }
        }

        return bestTarget;
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

    private void SetupVisuals()
    {
        if (bodyLine == null)
        {
            bodyLine = GetComponent<LineRenderer>();
        }

        if (lineMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                lineMaterial = new Material(spriteShader);
            }
        }

        if (bodyLine != null)
        {
            bodyLine.useWorldSpace = false;
            bodyLine.loop = true;
            bodyLine.positionCount = Mathf.Max(12, circleSegments);
            bodyLine.startWidth = bodyLineWidth;
            bodyLine.endWidth = bodyLineWidth;
            bodyLine.numCapVertices = 4;
            bodyLine.numCornerVertices = 4;
            bodyLine.sortingOrder = 48;
            bodyLine.startColor = bodyColor;
            bodyLine.endColor = bodyColor;

            if (lineMaterial != null)
            {
                bodyLine.sharedMaterial = lineMaterial;
            }
        }

        if (beamLine == null)
        {
            GameObject beamObject = new GameObject("Drone Beam");
            beamObject.transform.SetParent(transform);
            beamObject.transform.localPosition = Vector3.zero;
            beamObject.transform.localRotation = Quaternion.identity;
            beamObject.transform.localScale = Vector3.one;

            beamLine = beamObject.AddComponent<LineRenderer>();
        }

        if (beamLine != null)
        {
            beamLine.useWorldSpace = true;
            beamLine.loop = false;
            beamLine.positionCount = 2;
            beamLine.startWidth = beamWidth;
            beamLine.endWidth = beamWidth * 0.5f;
            beamLine.numCapVertices = 4;
            beamLine.numCornerVertices = 4;
            beamLine.sortingOrder = 49;
            beamLine.startColor = beamColor;
            beamLine.endColor = beamColor;

            if (lineMaterial != null)
            {
                beamLine.sharedMaterial = lineMaterial;
            }

            beamLine.enabled = false;
        }
    }

    private void DrawBodyCircle()
    {
        if (bodyLine == null)
        {
            return;
        }

        int safeSegments = Mathf.Max(12, circleSegments);

        if (bodyLine.positionCount != safeSegments)
        {
            bodyLine.positionCount = safeSegments;
        }

        for (int i = 0; i < safeSegments; i++)
        {
            float angle = ((float)i / safeSegments) * Mathf.PI * 2f;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * bodyRadius,
                Mathf.Sin(angle) * bodyRadius,
                0f
            );

            bodyLine.SetPosition(i, point);
        }
    }

    private void ShowBeam(Vector3 targetPosition)
    {
        if (beamLine == null)
        {
            return;
        }

        beamLine.enabled = true;
        beamLine.SetPosition(0, transform.position);
        beamLine.SetPosition(1, targetPosition);
        beamTimer = beamVisibleTime;
    }

    private void UpdateBeamVisibility()
    {
        if (beamLine == null || !beamLine.enabled)
        {
            return;
        }

        beamTimer -= Time.deltaTime;

        if (beamTimer <= 0f)
        {
            beamLine.enabled = false;
        }
    }
}

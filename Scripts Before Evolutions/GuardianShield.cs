using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuardianShield : MonoBehaviour
{
    private Transform player;
    private int shieldIndex;
    private int shieldCount;

    private float orbitRadius = 1.45f;
    private float orbitSpeed = 160f;
    private float damage = 10f;
    private float hitRadius = 0.42f;
    private float enemyHitCooldown = 0.35f;
    private LayerMask enemyHitMask = ~0;
    private LayerMask projectileHitMask = ~0;
    private string enemyProjectileTag = "EnemyProjectile";

    private Color shieldColor = new Color(0.35f, 0.95f, 1f, 0.9f);
    private Color blockFlashColor = Color.white;
    private float visualRadius = 0.32f;
    private float lineWidth = 0.1f;
    private int circleSegments = 32;

    private float currentAngle;
    private float flashTimer;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    private readonly Dictionary<Health, float> lastHitTimes = new Dictionary<Health, float>();
    private readonly List<Health> cooldownCleanup = new List<Health>();

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void OnEnable()
    {
        lastHitTimes.Clear();
        flashTimer = 0f;
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
        Transform newPlayer,
        int newShieldIndex,
        int newShieldCount,
        float newOrbitRadius,
        float newOrbitSpeed,
        float newDamage,
        float newHitRadius,
        float newEnemyHitCooldown,
        LayerMask newEnemyHitMask,
        LayerMask newProjectileHitMask,
        string newEnemyProjectileTag,
        Color newShieldColor,
        Color newBlockFlashColor,
        float newVisualRadius,
        float newLineWidth,
        int newCircleSegments)
    {
        player = newPlayer;
        shieldIndex = Mathf.Max(0, newShieldIndex);
        shieldCount = Mathf.Max(1, newShieldCount);
        orbitRadius = Mathf.Max(0.25f, newOrbitRadius);
        orbitSpeed = Mathf.Max(0f, newOrbitSpeed);
        damage = Mathf.Max(0f, newDamage);
        hitRadius = Mathf.Max(0.1f, newHitRadius);
        enemyHitCooldown = Mathf.Max(0.05f, newEnemyHitCooldown);
        enemyHitMask = newEnemyHitMask;
        projectileHitMask = newProjectileHitMask;

        enemyProjectileTag = string.IsNullOrEmpty(newEnemyProjectileTag)
            ? "EnemyProjectile"
            : newEnemyProjectileTag;

        shieldColor = newShieldColor;
        blockFlashColor = newBlockFlashColor;
        visualRadius = Mathf.Max(0.08f, newVisualRadius);
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        circleSegments = Mathf.Max(12, newCircleSegments);

        float spacingAngle = 360f / shieldCount;
        currentAngle = spacingAngle * shieldIndex;

        SetupLineRenderer();
        UpdateVisualCircle();
        UpdatePosition(0f);
    }

    private void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        UpdatePosition(Time.deltaTime);
        DamageEnemiesTouchingShield();
        BlockEnemyProjectiles();
        CleanupOldCooldownEntries();

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
        }

        UpdateVisualColor();
    }

    private void UpdatePosition(float deltaTime)
    {
        currentAngle += orbitSpeed * deltaTime;

        float angleRadians = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(angleRadians) * orbitRadius,
            Mathf.Sin(angleRadians) * orbitRadius,
            0f
        );

        transform.position = player.position + offset;
        transform.rotation = Quaternion.identity;
    }

    private void DamageEnemiesTouchingShield()
    {
        if (damage <= 0f)
        {
            return;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            hitRadius,
            enemyHitMask
        );

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

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            float lastHitTime;

            if (lastHitTimes.TryGetValue(enemyHealth, out lastHitTime))
            {
                if (Time.time - lastHitTime < enemyHitCooldown)
                {
                    continue;
                }
            }

            lastHitTimes[enemyHealth] = Time.time;
            enemyHealth.TakeDamage(damage, false);
        }
    }

    private void BlockEnemyProjectiles()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            hitRadius,
            projectileHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
            {
                continue;
            }

            if (!IsEnemyProjectileCollider(hitCollider))
            {
                continue;
            }

            GameObject projectileObject = hitCollider.attachedRigidbody != null
                ? hitCollider.attachedRigidbody.gameObject
                : hitCollider.gameObject;

            PooledObject pooledObject = projectileObject.GetComponent<PooledObject>();

            if (pooledObject == null)
            {
                pooledObject = projectileObject.GetComponentInParent<PooledObject>();
            }

            if (pooledObject != null)
            {
                pooledObject.ReturnToPool();
            }
            else
            {
                Destroy(projectileObject);
            }

            flashTimer = 0.12f;
        }
    }

    private bool IsEnemyProjectileCollider(Collider2D hitCollider)
    {
        Transform currentTransform = hitCollider.transform;

        while (currentTransform != null)
        {
            // Safer than CompareTag().
            // CompareTag throws an error if the tag does not exist in Unity's Tag Manager.
            // This string comparison will not crash if the tag is missing.
            if (!string.IsNullOrEmpty(enemyProjectileTag) &&
                currentTransform.gameObject.tag == enemyProjectileTag)
            {
                return true;
            }

            string objectName = currentTransform.name.ToLowerInvariant();

            if (objectName.Contains("enemy") &&
                (objectName.Contains("projectile") || objectName.Contains("bullet")))
            {
                return true;
            }

            MonoBehaviour[] behaviours = currentTransform.GetComponents<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                {
                    continue;
                }

                string typeName = behaviours[i].GetType().Name.ToLowerInvariant();

                if (typeName.Contains("enemy") &&
                    (typeName.Contains("projectile") || typeName.Contains("bullet")))
                {
                    return true;
                }
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private bool IsEnemyCollider(Collider2D hitCollider)
    {
        Transform currentTransform = hitCollider.transform;

        while (currentTransform != null)
        {
            // Safer than CompareTag("Enemy").
            if (currentTransform.gameObject.tag == "Enemy")
            {
                return true;
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private void CleanupOldCooldownEntries()
    {
        cooldownCleanup.Clear();

        foreach (KeyValuePair<Health, float> pair in lastHitTimes)
        {
            if (pair.Key == null ||
                pair.Key.IsDead ||
                Time.time - pair.Value > enemyHitCooldown * 4f)
            {
                cooldownCleanup.Add(pair.Key);
            }
        }

        for (int i = 0; i < cooldownCleanup.Count; i++)
        {
            lastHitTimes.Remove(cooldownCleanup[i]);
        }
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
        lineRenderer.sortingOrder = 52;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                lineMaterial = new Material(spriteShader);
                lineRenderer.sharedMaterial = lineMaterial;
            }
        }

        UpdateVisualCircle();
        UpdateVisualColor();
    }

    private void UpdateVisualCircle()
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
                Mathf.Cos(angle) * visualRadius,
                Mathf.Sin(angle) * visualRadius,
                0f
            );

            lineRenderer.SetPosition(i, point);
        }
    }

    private void UpdateVisualColor()
    {
        if (lineRenderer == null)
        {
            return;
        }

        Color currentColor = flashTimer > 0f ? blockFlashColor : shieldColor;

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
    }
}
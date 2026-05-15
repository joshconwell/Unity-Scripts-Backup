using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RewardChest))]
public class RewardChestVisualPolish : MonoBehaviour
{
    private class SparkleVisual
    {
        public GameObject RootObject;
        public LineRenderer LineRenderer;
        public float AngleOffset;
        public float Radius;
        public float OrbitSpeed;
        public float PulseOffset;
        public float BaseScale;
    }

    [Header("Main Chest Motion")]
    [SerializeField] private bool pulseChest = true;
    [SerializeField] private float pulseAmount = 0.075f;
    [SerializeField] private float pulseSpeed = 4.5f;

    [SerializeField] private bool bobChest = true;
    [SerializeField] private float bobAmount = 0.08f;
    [SerializeField] private float bobSpeed = 2.4f;

    [Header("Chest Tint")]
    [SerializeField] private bool tintChest = true;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.35f, 1f);
    [SerializeField] private float tintStrength = 0.28f;

    [Header("Glow Ring")]
    [SerializeField] private bool showGlowRing = true;
    [SerializeField] private float glowRingRadius = 0.9f;
    [SerializeField] private float glowRingWidth = 0.055f;
    [SerializeField] private float glowPulseSpeed = 3.5f;
    [SerializeField] private float glowSpinSpeed = 45f;
    [SerializeField] private Color glowColor = new Color(1f, 0.7f, 0.15f, 0.65f);

    [Header("Sparkles")]
    [SerializeField] private bool showSparkles = true;
    [SerializeField] private int sparkleCount = 8;
    [SerializeField] private float sparkleRadius = 0.9f;
    [SerializeField] private float sparkleSize = 0.13f;
    [SerializeField] private float sparkleOrbitSpeed = 40f;
    [SerializeField] private float sparklePulseSpeed = 5.5f;
    [SerializeField] private Color sparkleColor = new Color(1f, 0.92f, 0.35f, 0.95f);

    [Header("Sorting")]
    [SerializeField] private int visualSortingOrder = 25;

    private readonly List<SparkleVisual> sparkles = new List<SparkleVisual>();

    private Vector3 baseWorldPosition;
    private Vector3 baseLocalScale;
    private float randomTimeOffset;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalSpriteColors;

    private LineRenderer glowRingRenderer;
    private Transform glowRingTransform;

    private Material lineMaterial;

    private void Awake()
    {
        baseLocalScale = transform.localScale;
        randomTimeOffset = Random.Range(0f, 100f);

        CacheSpriteRenderers();
        BuildLineMaterial();
        BuildGlowRing();
        BuildSparkles();
    }

    private void OnEnable()
    {
        baseWorldPosition = transform.position;

        if (baseLocalScale == Vector3.zero)
        {
            baseLocalScale = transform.localScale;
        }

        CacheSpriteRenderers();
        RestoreSpriteColors();

        SetVisualsActive(true);
    }

    private void OnDisable()
    {
        transform.localScale = baseLocalScale;
        RestoreSpriteColors();
        SetVisualsActive(false);
    }

    private void Update()
    {
        float time = Time.time + randomTimeOffset;

        UpdateChestMotion(time);
        UpdateChestTint(time);
        UpdateGlowRing(time);
        UpdateSparkles(time);
    }

    private void UpdateChestMotion(float time)
    {
        if (pulseChest)
        {
            float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
            transform.localScale = baseLocalScale * pulse;
        }

        if (bobChest)
        {
            float bob = Mathf.Sin(time * bobSpeed) * bobAmount;
            transform.position = baseWorldPosition + new Vector3(0f, bob, 0f);
        }
    }

    private void UpdateChestTint(float time)
    {
        if (!tintChest)
        {
            return;
        }

        if (spriteRenderers == null || originalSpriteColors == null)
        {
            return;
        }

        float pulse = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f;
        float finalTintStrength = pulse * tintStrength;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            if (i >= originalSpriteColors.Length)
            {
                continue;
            }

            spriteRenderers[i].color = Color.Lerp(
                originalSpriteColors[i],
                highlightColor,
                finalTintStrength
            );
        }
    }

    private void UpdateGlowRing(float time)
    {
        if (!showGlowRing || glowRingRenderer == null || glowRingTransform == null)
        {
            return;
        }

        float pulse = (Mathf.Sin(time * glowPulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(0.22f, glowColor.a, pulse);

        Color finalColor = glowColor;
        finalColor.a = alpha;

        glowRingRenderer.startColor = finalColor;
        glowRingRenderer.endColor = finalColor;

        glowRingTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            time * glowSpinSpeed
        );
    }

    private void UpdateSparkles(float time)
    {
        if (!showSparkles)
        {
            return;
        }

        for (int i = 0; i < sparkles.Count; i++)
        {
            SparkleVisual sparkle = sparkles[i];

            if (sparkle == null || sparkle.RootObject == null || sparkle.LineRenderer == null)
            {
                continue;
            }

            float angle = sparkle.AngleOffset + time * sparkle.OrbitSpeed;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 localPosition = new Vector3(
                Mathf.Cos(radians) * sparkle.Radius,
                Mathf.Sin(radians) * sparkle.Radius,
                -0.02f
            );

            sparkle.RootObject.transform.localPosition = localPosition;

            float pulse = (Mathf.Sin(time * sparklePulseSpeed + sparkle.PulseOffset) + 1f) * 0.5f;
            float scale = sparkle.BaseScale * Mathf.Lerp(0.65f, 1.25f, pulse);

            sparkle.RootObject.transform.localScale = Vector3.one * scale;
            sparkle.RootObject.transform.localRotation = Quaternion.Euler(0f, 0f, -angle * 1.5f);

            Color finalColor = sparkleColor;
            finalColor.a = Mathf.Lerp(0.2f, sparkleColor.a, pulse);

            sparkle.LineRenderer.startColor = finalColor;
            sparkle.LineRenderer.endColor = finalColor;
        }
    }

    private void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

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

    private void RestoreSpriteColors()
    {
        if (spriteRenderers == null || originalSpriteColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            if (i >= originalSpriteColors.Length)
            {
                continue;
            }

            spriteRenderers[i].color = originalSpriteColors[i];
        }
    }

    private void BuildLineMaterial()
    {
        if (lineMaterial != null)
        {
            return;
        }

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            lineMaterial = new Material(spriteShader);
        }
    }

    private void BuildGlowRing()
    {
        if (!showGlowRing)
        {
            return;
        }

        if (glowRingRenderer != null)
        {
            return;
        }

        GameObject ringObject = new GameObject("Reward Chest Glow Ring");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;
        ringObject.transform.localRotation = Quaternion.identity;
        ringObject.transform.localScale = Vector3.one;

        glowRingTransform = ringObject.transform;

        glowRingRenderer = ringObject.AddComponent<LineRenderer>();
        glowRingRenderer.useWorldSpace = false;
        glowRingRenderer.loop = true;
        glowRingRenderer.positionCount = 64;
        glowRingRenderer.startWidth = glowRingWidth;
        glowRingRenderer.endWidth = glowRingWidth;
        glowRingRenderer.startColor = glowColor;
        glowRingRenderer.endColor = glowColor;
        glowRingRenderer.sortingOrder = visualSortingOrder;

        if (lineMaterial != null)
        {
            glowRingRenderer.material = lineMaterial;
        }

        for (int i = 0; i < glowRingRenderer.positionCount; i++)
        {
            float t = (float)i / glowRingRenderer.positionCount;
            float angle = t * Mathf.PI * 2f;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * glowRingRadius,
                Mathf.Sin(angle) * glowRingRadius,
                0f
            );

            glowRingRenderer.SetPosition(i, point);
        }
    }

    private void BuildSparkles()
    {
        if (!showSparkles)
        {
            return;
        }

        if (sparkles.Count > 0)
        {
            return;
        }

        int safeSparkleCount = Mathf.Max(0, sparkleCount);

        for (int i = 0; i < safeSparkleCount; i++)
        {
            GameObject sparkleObject = new GameObject("Reward Chest Sparkle");
            sparkleObject.transform.SetParent(transform, false);
            sparkleObject.transform.localPosition = Vector3.zero;
            sparkleObject.transform.localRotation = Quaternion.identity;
            sparkleObject.transform.localScale = Vector3.one * sparkleSize;

            LineRenderer lineRenderer = sparkleObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 5;
            lineRenderer.startWidth = 0.035f;
            lineRenderer.endWidth = 0.035f;
            lineRenderer.startColor = sparkleColor;
            lineRenderer.endColor = sparkleColor;
            lineRenderer.sortingOrder = visualSortingOrder + 1;

            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }

            float halfSize = 0.5f;

            lineRenderer.SetPosition(0, new Vector3(-halfSize, 0f, 0f));
            lineRenderer.SetPosition(1, new Vector3(halfSize, 0f, 0f));
            lineRenderer.SetPosition(2, Vector3.zero);
            lineRenderer.SetPosition(3, new Vector3(0f, -halfSize, 0f));
            lineRenderer.SetPosition(4, new Vector3(0f, halfSize, 0f));

            SparkleVisual sparkle = new SparkleVisual
            {
                RootObject = sparkleObject,
                LineRenderer = lineRenderer,
                AngleOffset = ((float)i / Mathf.Max(1, safeSparkleCount)) * 360f + Random.Range(-18f, 18f),
                Radius = sparkleRadius * Random.Range(0.78f, 1.12f),
                OrbitSpeed = sparkleOrbitSpeed * Random.Range(0.75f, 1.25f),
                PulseOffset = Random.Range(0f, 10f),
                BaseScale = sparkleSize * Random.Range(0.8f, 1.25f)
            };

            sparkles.Add(sparkle);
        }
    }

    private void SetVisualsActive(bool active)
    {
        if (glowRingRenderer != null)
        {
            glowRingRenderer.gameObject.SetActive(active && showGlowRing);
        }

        for (int i = 0; i < sparkles.Count; i++)
        {
            if (sparkles[i] != null && sparkles[i].RootObject != null)
            {
                sparkles[i].RootObject.SetActive(active && showSparkles);
            }
        }
    }
}
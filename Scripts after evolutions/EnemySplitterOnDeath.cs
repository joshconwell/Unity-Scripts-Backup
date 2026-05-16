using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemySplitterOnDeath : MonoBehaviour
{
    [Header("Split Prefab")]
    [SerializeField] private GameObject childEnemyPrefab;

    [Header("Split Count")]
    [SerializeField] private int minChildren = 2;
    [SerializeField] private int maxChildren = 4;

    [Header("Spawn Placement")]
    [SerializeField] private float spawnRadius = 0.75f;
    [SerializeField] private float spawnScatter = 0.35f;
    [SerializeField] private bool evenlySpaceChildren = true;

    [Header("Spawn Safety")]
    [SerializeField] private bool preventDuplicateSplit = true;
    [SerializeField] private bool requireChildPrefab = true;

    [Header("Split Visual")]
    [SerializeField] private bool showSplitBurst = true;
    [SerializeField] private Color splitBurstColor = new Color(0.8f, 0.35f, 1f, 0.85f);
    [SerializeField] private float splitBurstDuration = 0.22f;
    [SerializeField] private float splitBurstStartRadius = 0.35f;
    [SerializeField] private float splitBurstEndRadius = 1.6f;
    [SerializeField] private float splitBurstLineWidth = 0.1f;
    [SerializeField] private int splitBurstSegments = 48;
    [SerializeField] private int splitBurstSortingOrder = 35;

    [Header("Debug")]
    [SerializeField] private bool allowDebugSplitKey = false;
    [SerializeField] private KeyCode debugSplitKey = KeyCode.F2;

    private Health health;
    private bool hasSplit;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        hasSplit = false;

        if (health == null)
        {
            health = GetComponent<Health>();
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
    }

    private void Update()
    {
        if (allowDebugSplitKey && Input.GetKeyDown(debugSplitKey))
        {
            Split();
        }
    }

    private void HandleDied()
    {
        Split();
    }

    private void Split()
    {
        if (preventDuplicateSplit && hasSplit)
        {
            return;
        }

        hasSplit = true;

        if (childEnemyPrefab == null)
        {
            if (requireChildPrefab)
            {
                Debug.LogWarning($"{gameObject.name} tried to split, but no child enemy prefab is assigned.");
                return;
            }
        }

        int childrenToSpawn = Random.Range(
            Mathf.Min(minChildren, maxChildren),
            Mathf.Max(minChildren, maxChildren) + 1
        );

        for (int i = 0; i < childrenToSpawn; i++)
        {
            SpawnChild(i, childrenToSpawn);
        }

        if (showSplitBurst)
        {
            SpawnSplitBurstVisual();
        }
    }

    private void SpawnChild(int childIndex, int totalChildren)
    {
        if (childEnemyPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = GetChildSpawnPosition(childIndex, totalChildren);

        GameObject childObject = null;

        if (PoolManager.HasInstance)
        {
            childObject = PoolManager.Instance.Spawn(
                childEnemyPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (childObject == null)
        {
            childObject = Instantiate(
                childEnemyPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (childObject == null)
        {
            return;
        }

        childObject.transform.position = spawnPosition;
        childObject.transform.rotation = Quaternion.identity;
        childObject.SetActive(true);
    }

    private Vector3 GetChildSpawnPosition(int childIndex, int totalChildren)
    {
        Vector2 direction;

        if (evenlySpaceChildren && totalChildren > 0)
        {
            float angle = ((float)childIndex / totalChildren) * Mathf.PI * 2f;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        else
        {
            direction = Random.insideUnitCircle.normalized;

            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector2.right;
            }
        }

        Vector2 randomScatter = Random.insideUnitCircle * spawnScatter;
        Vector2 finalOffset = direction * spawnRadius + randomScatter;

        return transform.position + new Vector3(finalOffset.x, finalOffset.y, 0f);
    }

    private void SpawnSplitBurstVisual()
    {
        GameObject burstObject = new GameObject("Splitter Burst Visual");
        burstObject.transform.position = transform.position;

        SplitterBurstVisual burstVisual = burstObject.AddComponent<SplitterBurstVisual>();

        burstVisual.Play(
            transform.position,
            splitBurstColor,
            splitBurstDuration,
            splitBurstStartRadius,
            splitBurstEndRadius,
            splitBurstLineWidth,
            splitBurstSegments,
            splitBurstSortingOrder
        );
    }
}

public class SplitterBurstVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private Vector3 center;
    private Color burstColor;

    private float duration;
    private float startRadius;
    private float endRadius;
    private float lineWidth;

    private int segments;
    private float timer;

    private bool playing;

    public void Play(
        Vector3 newCenter,
        Color newBurstColor,
        float newDuration,
        float newStartRadius,
        float newEndRadius,
        float newLineWidth,
        int newSegments,
        int newSortingOrder)
    {
        center = newCenter;
        burstColor = newBurstColor;

        duration = Mathf.Max(0.01f, newDuration);
        startRadius = Mathf.Max(0.01f, newStartRadius);
        endRadius = Mathf.Max(startRadius, newEndRadius);
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        segments = Mathf.Max(8, newSegments);

        timer = 0f;
        playing = true;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = burstColor;
        lineRenderer.endColor = burstColor;
        lineRenderer.sortingOrder = newSortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            lineRenderer.material = new Material(spriteShader);
        }

        DrawCircle(startRadius, burstColor);
    }

    private void Update()
    {
        if (!playing)
        {
            return;
        }

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);
        float radius = Mathf.Lerp(startRadius, endRadius, t);

        Color finalColor = burstColor;
        finalColor.a = Mathf.Lerp(burstColor.a, 0f, t);

        DrawCircle(radius, finalColor);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void DrawCircle(float radius, Color color)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float percent = (float)i / segments;
            float angle = percent * Mathf.PI * 2f;

            Vector3 point = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            lineRenderer.SetPosition(i, point);
        }
    }
}
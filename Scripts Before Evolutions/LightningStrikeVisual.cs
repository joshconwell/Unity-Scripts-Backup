using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningStrikeVisual : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private float boltLength = 3.5f;
    [SerializeField] private int segmentCount = 6;
    [SerializeField] private float jaggedness = 0.35f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.16f;
    [SerializeField] private float width = 0.14f;

    [Header("Color")]
    [SerializeField] private Color startColor = new Color(0.4f, 0.9f, 1f, 1f);
    [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 0f);

    private LineRenderer lineRenderer;
    private PooledObject pooledObject;

    private float timer;
    private bool playing;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        pooledObject = GetComponent<PooledObject>();

        SetupLineRenderer();
    }

    private void OnEnable()
    {
        timer = 0f;
        playing = false;

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        SetupLineRenderer();
    }

    private void Update()
    {
        if (!playing)
        {
            return;
        }

        timer += Time.deltaTime;

        float t = 0f;

        if (duration > 0f)
        {
            t = Mathf.Clamp01(timer / duration);
        }
        else
        {
            t = 1f;
        }

        Color currentStartColor = Color.Lerp(startColor, endColor, t);
        Color currentEndColor = Color.Lerp(startColor, endColor, t);

        if (lineRenderer != null)
        {
            lineRenderer.startColor = currentStartColor;
            lineRenderer.endColor = currentEndColor;
            lineRenderer.widthMultiplier = Mathf.Lerp(width, 0f, t);
        }

        if (t >= 1f)
        {
            DeactivateVisual();
        }
    }

    public void Play(Vector3 strikePosition)
    {
        Play(
            strikePosition,
            boltLength,
            duration,
            startColor,
            endColor,
            width,
            segmentCount,
            jaggedness
        );
    }

    public void Play(
        Vector3 strikePosition,
        float newBoltLength,
        float newDuration,
        Color newStartColor,
        Color newEndColor,
        float newWidth,
        int newSegmentCount,
        float newJaggedness
    )
    {
        boltLength = Mathf.Max(0.5f, newBoltLength);
        duration = Mathf.Max(0.01f, newDuration);
        startColor = newStartColor;
        endColor = newEndColor;
        width = Mathf.Max(0.01f, newWidth);
        segmentCount = Mathf.Max(2, newSegmentCount);
        jaggedness = Mathf.Max(0f, newJaggedness);

        timer = 0f;
        playing = true;

        SetupLineRenderer();
        BuildLightningBolt(strikePosition);
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = width;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;
        lineRenderer.numCapVertices = 2;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.sortingOrder = 40;
    }

    private void BuildLightningBolt(Vector3 strikePosition)
    {
        if (lineRenderer == null)
        {
            return;
        }

        Vector3 startPoint = strikePosition + new Vector3(Random.Range(-0.75f, 0.75f), boltLength, 0f);
        Vector3 endPoint = strikePosition;

        Vector3 direction = endPoint - startPoint;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector3.down;
        }

        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;

        int pointCount = segmentCount + 1;
        lineRenderer.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);

            if (i != 0 && i != pointCount - 1)
            {
                float randomOffset = Random.Range(-jaggedness, jaggedness);
                point += perpendicular * randomOffset;
            }

            point.z = strikePosition.z;
            lineRenderer.SetPosition(i, point);
        }
    }

    private void DeactivateVisual()
    {
        playing = false;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
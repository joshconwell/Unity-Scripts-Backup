using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionVisual : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float startScaleMultiplier = 0.2f;
    [SerializeField] private float endScaleMultiplier = 1f;

    [Header("Color")]
    [SerializeField] private Color startColor = new Color(1f, 0.65f, 0.1f, 0.75f);
    [SerializeField] private Color endColor = new Color(1f, 0.1f, 0f, 0f);

    private SpriteRenderer spriteRenderer;
    private PooledObject pooledObject;

    private Vector3 baseScale;
    private float targetRadius = 2.25f;
    private float timer;
    private bool playing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pooledObject = GetComponent<PooledObject>();
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        timer = 0f;
        playing = false;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        transform.localScale = baseScale;
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

        float currentScaleMultiplier = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, t);
        float diameter = targetRadius * 2f;

        transform.localScale = baseScale * diameter * currentScaleMultiplier;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
        }

        if (t >= 1f)
        {
            DeactivateVisual();
        }
    }

    public void Play(float explosionRadius)
    {
        Play(explosionRadius, duration, startColor, endColor);
    }

    public void Play(float explosionRadius, float newDuration, Color newStartColor, Color newEndColor)
    {
        targetRadius = Mathf.Max(0.1f, explosionRadius);
        duration = Mathf.Max(0.01f, newDuration);

        startColor = newStartColor;
        endColor = newEndColor;

        timer = 0f;
        playing = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }

        float startDiameter = targetRadius * 2f * startScaleMultiplier;
        transform.localScale = baseScale * startDiameter;
    }

    private void DeactivateVisual()
    {
        playing = false;

        transform.localScale = baseScale;

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
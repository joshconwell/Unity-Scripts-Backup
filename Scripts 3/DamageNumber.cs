using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class DamageNumber : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float sideDrift = 0.35f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 0.75f;

    [Header("Scale")]
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float endScale = 0.45f;
    [SerializeField] private float criticalScaleMultiplier = 1.35f;

    private TextMesh textMesh;
    private PooledObject pooledObject;

    private Color startColor;
    private Vector3 driftDirection;
    private float timer;
    private bool initialized;
    private float currentStartScale;
    private float currentEndScale;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        timer = 0f;
        initialized = false;

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        timer += Time.deltaTime;

        float progress = timer / lifetime;
        progress = Mathf.Clamp01(progress);

        transform.position += driftDirection * Time.deltaTime;

        float currentScale = Mathf.Lerp(currentStartScale, currentEndScale, progress);
        transform.localScale = Vector3.one * currentScale;

        Color currentColor = startColor;
        currentColor.a = Mathf.Lerp(1f, 0f, progress);
        textMesh.color = currentColor;

        if (timer >= lifetime)
        {
            DeactivateDamageNumber();
        }
    }

    public void Initialize(float damageAmount, Color textColor, int fontSize, bool isCriticalHit)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMesh>();
        }

        timer = 0f;
        initialized = true;

        int roundedDamage = Mathf.RoundToInt(damageAmount);

        if (isCriticalHit)
        {
            textMesh.text = $"{roundedDamage}!";
        }
        else
        {
            textMesh.text = roundedDamage.ToString();
        }

        textMesh.color = textColor;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        startColor = textColor;

        float randomSideDirection = Random.Range(-1f, 1f);
        driftDirection = new Vector3(randomSideDirection * sideDrift, floatSpeed, 0f);

        currentStartScale = startScale;
        currentEndScale = endScale;

        if (isCriticalHit)
        {
            currentStartScale *= criticalScaleMultiplier;
            currentEndScale *= criticalScaleMultiplier;
        }

        transform.localScale = Vector3.one * currentStartScale;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 100;
        }
    }

    private void DeactivateDamageNumber()
    {
        initialized = false;

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
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public interface IDamageTakenModifier
{
    float ModifyIncomingDamage(float damageAmount, bool isCriticalHit);
}

public class DamageReductionStatus : MonoBehaviour, IDamageTakenModifier
{
    [Header("Runtime")]
    [SerializeField] private float damageReductionPercent = 0.35f;
    [SerializeField] private float remainingDuration = 2.5f;

    [Header("Visual")]
    [SerializeField] private bool showShieldRing = true;
    [SerializeField] private Color shieldColor = new Color(0.35f, 0.75f, 1f, 0.75f);
    [SerializeField] private float ringRadius = 0.75f;
    [SerializeField] private float ringWidth = 0.07f;
    [SerializeField] private int ringSegments = 48;
    [SerializeField] private int sortingOrder = 33;
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseAmount = 0.08f;

    private LineRenderer shieldRing;
    private float randomPulseOffset;

    private void Awake()
    {
        randomPulseOffset = Random.Range(0f, 10f);
        BuildShieldRing();
    }

    private void OnEnable()
    {
        randomPulseOffset = Random.Range(0f, 10f);
        BuildShieldRing();
        SetRingActive(showShieldRing);
    }

    private void OnDisable()
    {
        SetRingActive(false);
    }

    private void Update()
    {
        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
        {
            Destroy(this);
            return;
        }

        UpdateShieldRing();
    }

    public void ApplyShield(
        float newDamageReductionPercent,
        float newDuration,
        Color newShieldColor,
        float newRingRadius,
        float newRingWidth,
        int newSortingOrder)
    {
        damageReductionPercent = Mathf.Clamp01(newDamageReductionPercent);
        remainingDuration = Mathf.Max(remainingDuration, newDuration);

        shieldColor = newShieldColor;
        ringRadius = Mathf.Max(0.1f, newRingRadius);
        ringWidth = Mathf.Max(0.01f, newRingWidth);
        sortingOrder = newSortingOrder;

        BuildShieldRing();
        SetRingActive(showShieldRing);
        UpdateShieldRing();
    }

    public float ModifyIncomingDamage(float damageAmount, bool isCriticalHit)
    {
        if (damageAmount <= 0f)
        {
            return damageAmount;
        }

        float multiplier = 1f - Mathf.Clamp01(damageReductionPercent);
        return damageAmount * multiplier;
    }

    private void BuildShieldRing()
    {
        if (!showShieldRing)
        {
            return;
        }

        if (shieldRing != null)
        {
            return;
        }

        GameObject ringObject = new GameObject("Shield Reduction Ring");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;
        ringObject.transform.localRotation = Quaternion.identity;
        ringObject.transform.localScale = Vector3.one;

        shieldRing = ringObject.AddComponent<LineRenderer>();
        shieldRing.useWorldSpace = true;
        shieldRing.loop = true;
        shieldRing.positionCount = Mathf.Max(8, ringSegments);
        shieldRing.startWidth = ringWidth;
        shieldRing.endWidth = ringWidth;
        shieldRing.startColor = shieldColor;
        shieldRing.endColor = shieldColor;
        shieldRing.sortingOrder = sortingOrder;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
        {
            shieldRing.material = new Material(spriteShader);
        }
    }

    private void UpdateShieldRing()
    {
        if (!showShieldRing || shieldRing == null)
        {
            return;
        }

        shieldRing.sortingOrder = sortingOrder;
        shieldRing.startWidth = ringWidth;
        shieldRing.endWidth = ringWidth;

        float pulse = 1f + Mathf.Sin((Time.time + randomPulseOffset) * pulseSpeed) * pulseAmount;
        float finalRadius = ringRadius * pulse;

        Color finalColor = shieldColor;
        finalColor.a = Mathf.Lerp(0.35f, shieldColor.a, (Mathf.Sin((Time.time + randomPulseOffset) * pulseSpeed) + 1f) * 0.5f);

        shieldRing.startColor = finalColor;
        shieldRing.endColor = finalColor;

        int safeSegments = Mathf.Max(8, ringSegments);
        shieldRing.positionCount = safeSegments;

        for (int i = 0; i < safeSegments; i++)
        {
            float percent = (float)i / safeSegments;
            float angle = percent * Mathf.PI * 2f;

            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle) * finalRadius,
                Mathf.Sin(angle) * finalRadius,
                0f
            );

            shieldRing.SetPosition(i, point);
        }
    }

    private void SetRingActive(bool active)
    {
        if (shieldRing != null)
        {
            shieldRing.gameObject.SetActive(active);
        }
    }

    private void OnDestroy()
    {
        if (shieldRing != null)
        {
            Destroy(shieldRing.gameObject);
        }
    }
}
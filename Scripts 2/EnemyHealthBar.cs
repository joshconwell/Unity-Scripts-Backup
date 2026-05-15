using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Transform barRoot;
    [SerializeField] private Transform fillTransform;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private bool keepBarUpright = true;

    [Header("Visibility")]
    [SerializeField] private bool hideWhenFullHealth = true;
    [SerializeField] private bool hideWhenDead = true;

    private Vector3 originalFillScale;
    private Vector3 originalFillLocalPosition;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (fillTransform != null)
        {
            originalFillScale = fillTransform.localScale;
            originalFillLocalPosition = fillTransform.localPosition;
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthBar;
            health.OnDied += HandleDeath;

            UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthBar;
            health.OnDied -= HandleDeath;
        }
    }

    private void Start()
    {
        if (health != null)
        {
            UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void LateUpdate()
    {
        if (barRoot == null)
        {
            return;
        }

        barRoot.position = transform.position + worldOffset;

        if (keepBarUpright)
        {
            barRoot.rotation = Quaternion.identity;
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (barRoot == null || fillTransform == null)
        {
            return;
        }

        if (maxHealth <= 0f)
        {
            return;
        }

        float healthPercent = currentHealth / maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        Vector3 newScale = originalFillScale;
        newScale.x = originalFillScale.x * healthPercent;
        fillTransform.localScale = newScale;

        float missingWidth = originalFillScale.x - newScale.x;
        Vector3 newLocalPosition = originalFillLocalPosition;
        newLocalPosition.x = originalFillLocalPosition.x - missingWidth * 0.5f;
        fillTransform.localPosition = newLocalPosition;

        bool shouldHideBecauseFull = hideWhenFullHealth && currentHealth >= maxHealth;
        bool shouldHideBecauseDead = hideWhenDead && currentHealth <= 0f;

        barRoot.gameObject.SetActive(!shouldHideBecauseFull && !shouldHideBecauseDead);
    }

    private void HandleDeath()
    {
        if (barRoot != null && hideWhenDead)
        {
            barRoot.gameObject.SetActive(false);
        }
    }
}
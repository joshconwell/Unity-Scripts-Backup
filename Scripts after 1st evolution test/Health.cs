using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private bool resetHealthOnEnable = true;

    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private bool returnToPoolInsteadOfDestroy = false;

    [Header("Invincibility After Damage")]
    [SerializeField] private bool useInvincibilityAfterDamage = false;
    [SerializeField] private float invincibilityDuration = 0.75f;
    [SerializeField] private bool flashDuringInvincibility = true;
    [SerializeField] private float flashInterval = 0.08f;
    [SerializeField] private SpriteRenderer[] flashRenderers;

    private float currentHealth;
    private bool isDead;
    private bool isInvincible;
    private Coroutine invincibilityCoroutine;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;

    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnDamaged;
    public event Action<float, bool> OnDamagedDetailed;
    public event Action<float> OnHealed;
    public event Action OnDied;

    private void Awake()
    {
        AutoFindFlashRenderers();
        ResetHealth();
    }

    private void OnEnable()
    {
        if (resetHealthOnEnable)
        {
            ResetHealth();
        }

        StopInvincibility();
        SetFlashRenderersVisible(true);
    }

    private void OnDisable()
    {
        StopInvincibility();
        SetFlashRenderersVisible(true);
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damageAmount, bool isCriticalHit = false)
    {
        if (isDead)
        {
            return;
        }

        if (damageAmount <= 0f)
        {
            return;
        }

        if (useInvincibilityAfterDamage && isInvincible)
        {
            return;
        }

        damageAmount = ApplyIncomingDamageModifiers(damageAmount, isCriticalHit);

        if (damageAmount <= 0f)
        {
            return;
        }

        float previousHealth = currentHealth;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            isDead = true;
        }

        float actualDamageTaken = previousHealth - currentHealth;

        OnDamaged?.Invoke(actualDamageTaken);
        OnDamagedDetailed?.Invoke(actualDamageTaken, isCriticalHit);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        string critText = isCriticalHit ? " CRIT!" : "";
        Debug.Log($"{gameObject.name} took {actualDamageTaken:0.##} damage.{critText} Health: {currentHealth:0.##}/{maxHealth:0.##}");

        if (isDead)
        {
            Die();
            return;
        }

        if (useInvincibilityAfterDamage)
        {
            StartInvincibility();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
        {
            return;
        }

        if (healAmount <= 0f)
        {
            return;
        }

        float previousHealth = currentHealth;

        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        float actualHealAmount = currentHealth - previousHealth;

        OnHealed?.Invoke(actualHealAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(float amount, bool healByAddedAmount)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHealth += amount;

        if (healByAddedAmount)
        {
            currentHealth += amount;
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} max health increased to {maxHealth}");
    }

    public void RestoreFullHealth()
    {
        ResetHealth();
    }

    private float ApplyIncomingDamageModifiers(float damageAmount, bool isCriticalHit)
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            if (!behaviour.enabled)
            {
                continue;
            }

            IDamageTakenModifier modifier = behaviour as IDamageTakenModifier;

            if (modifier == null)
            {
                continue;
            }

            damageAmount = modifier.ModifyIncomingDamage(damageAmount, isCriticalHit);

            if (damageAmount <= 0f)
            {
                return 0f;
            }
        }

        return damageAmount;
    }

    private void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        StopInvincibility();
        SetFlashRenderersVisible(true);

        OnDied?.Invoke();

        if (!destroyOnDeath)
        {
            return;
        }

        if (returnToPoolInsteadOfDestroy)
        {
            PooledObject pooledObject = GetComponent<PooledObject>();

            if (pooledObject != null)
            {
                pooledObject.ReturnToPool();
                return;
            }
        }

        Destroy(gameObject);
    }

    private void StartInvincibility()
    {
        StopInvincibility();
        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private void StopInvincibility()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        isInvincible = false;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float elapsedTime = 0f;
        bool visible = true;

        while (elapsedTime < invincibilityDuration)
        {
            if (flashDuringInvincibility)
            {
                visible = !visible;
                SetFlashRenderersVisible(visible);
            }

            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        SetFlashRenderersVisible(true);

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    private void AutoFindFlashRenderers()
    {
        if (flashRenderers != null && flashRenderers.Length > 0)
        {
            return;
        }

        flashRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void SetFlashRenderersVisible(bool visible)
    {
        if (flashRenderers == null)
        {
            return;
        }

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] != null)
            {
                flashRenderers[i].enabled = visible;
            }
        }
    }
}
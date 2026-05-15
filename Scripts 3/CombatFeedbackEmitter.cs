using UnityEngine;

[RequireComponent(typeof(Health))]
public class CombatFeedbackEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;

    [Header("Damage Feedback")]
    [SerializeField] private bool feedbackOnDamage = true;
    [SerializeField] private bool shakeOnDamage = true;
    [SerializeField] private float damageShakeDuration = 0.05f;
    [SerializeField] private float damageShakeMagnitude = 0.025f;
    [SerializeField] private bool hitStopOnDamage = true;
    [SerializeField] private float damageHitStopDuration = 0.025f;
    [SerializeField] private float minimumTimeBetweenDamageFeedback = 0.06f;

    [Header("Death Feedback")]
    [SerializeField] private bool feedbackOnDeath = true;
    [SerializeField] private bool shakeOnDeath = true;
    [SerializeField] private float deathShakeDuration = 0.1f;
    [SerializeField] private float deathShakeMagnitude = 0.07f;
    [SerializeField] private bool hitStopOnDeath = true;
    [SerializeField] private float deathHitStopDuration = 0.035f;

    private float nextAllowedDamageFeedbackTime;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }
    }

    private void HandleDamaged(float damageAmount)
    {
        if (!feedbackOnDamage)
        {
            return;
        }

        if (damageAmount <= 0f)
        {
            return;
        }

        if (Time.unscaledTime < nextAllowedDamageFeedbackTime)
        {
            return;
        }

        nextAllowedDamageFeedbackTime = Time.unscaledTime + minimumTimeBetweenDamageFeedback;

        if (shakeOnDamage && CameraShake.HasInstance)
        {
            CameraShake.Instance.Shake(damageShakeDuration, damageShakeMagnitude);
        }

        if (hitStopOnDamage && HitStopManager.HasInstance)
        {
            HitStopManager.Instance.DoHitStop(damageHitStopDuration);
        }
    }

    private void HandleDied()
    {
        if (!feedbackOnDeath)
        {
            return;
        }

        if (shakeOnDeath && CameraShake.HasInstance)
        {
            CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);
        }

        if (hitStopOnDeath && HitStopManager.HasInstance)
        {
            HitStopManager.Instance.DoHitStop(deathHitStopDuration);
        }
    }
}
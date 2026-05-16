using UnityEngine;

public class HealthSFX : MonoBehaviour
{
    private enum HealthSFXOwnerType
    {
        Enemy,
        Player
    }

    [Header("Owner")]
    [SerializeField] private HealthSFXOwnerType ownerType = HealthSFXOwnerType.Enemy;

    [Header("Enemy SFX")]
    [SerializeField] private SFXType enemyHitSFX = SFXType.EnemyHit;
    [SerializeField] private SFXType enemyCritHitSFX = SFXType.EnemyCritHit;
    [SerializeField] private SFXType enemyDeathSFX = SFXType.EnemyDeath;

    [Header("Player SFX")]
    [SerializeField] private SFXType playerDamagedSFX = SFXType.PlayerDamaged;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float hitVolumeMultiplier = 0.4f;

    [Range(0f, 1f)]
    [SerializeField] private float critVolumeMultiplier = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolumeMultiplier = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float playerDamageVolumeMultiplier = 0.9f;

    [Header("Spam Control")]
    [SerializeField] private float minimumTimeBetweenHitSounds = 0.03f;

    private Health health;
    private float lastHitSoundTime = -999f;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (health == null)
            return;

        health.OnDamagedDetailed += HandleDamagedDetailed;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.OnDamagedDetailed -= HandleDamagedDetailed;
        health.OnDied -= HandleDied;
    }

    private void HandleDamagedDetailed(float actualDamage, bool isCriticalHit)
    {
        if (!AudioManager.HasInstance)
            return;

        if (Time.unscaledTime < lastHitSoundTime + minimumTimeBetweenHitSounds)
            return;

        lastHitSoundTime = Time.unscaledTime;

        if (ownerType == HealthSFXOwnerType.Player)
        {
            AudioManager.Instance.PlaySFX(playerDamagedSFX, transform.position, playerDamageVolumeMultiplier);
            return;
        }

        if (isCriticalHit)
        {
            AudioManager.Instance.PlaySFX(enemyCritHitSFX, transform.position, critVolumeMultiplier);
        }
        else
        {
            AudioManager.Instance.PlaySFX(enemyHitSFX, transform.position, hitVolumeMultiplier);
        }
    }

    private void HandleDied()
    {
        if (!AudioManager.HasInstance)
            return;

        if (ownerType == HealthSFXOwnerType.Player)
            return;

        AudioManager.Instance.PlaySFX(enemyDeathSFX, transform.position, deathVolumeMultiplier);
    }
}
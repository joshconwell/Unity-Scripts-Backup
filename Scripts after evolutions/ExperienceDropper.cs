using UnityEngine;

[RequireComponent(typeof(Health))]
public class ExperienceDropper : MonoBehaviour
{
    [Header("XP Drop Settings")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpAmount = 1;
    [SerializeField] private int orbCount = 1;
    [SerializeField] private float scatterRadius = 0.35f;

    [Header("Health Pickup Drop Settings")]
    [SerializeField] private GameObject healthPickupPrefab;

    [Tooltip("0.05 = 5% chance. This can be increased by player upgrades.")]
    [Range(0f, 1f)]
    [SerializeField] private float healthPickupDropChance = 0.05f;

    [SerializeField] private float healthPickupHealAmount = 15f;

    [Tooltip("If checked, enemies will not drop health pickups when the player is already full health.")]
    [SerializeField] private bool onlyDropHealthWhenPlayerNotFull = true;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += DropRewards;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= DropRewards;
        }
    }

    private void DropRewards()
    {
        DropExperience();
        TryDropHealthPickup();
    }

    private void DropExperience()
    {
        if (xpOrbPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing an XP Orb prefab.");
            return;
        }

        int safeOrbCount = Mathf.Max(1, orbCount);
        int xpPerOrb = Mathf.Max(1, xpAmount / safeOrbCount);

        for (int i = 0; i < safeOrbCount; i++)
        {
            Vector3 spawnPosition = GetRandomScatterPosition();

            GameObject orbObject = SpawnObject(xpOrbPrefab, spawnPosition);

            XPOrb xpOrb = orbObject.GetComponent<XPOrb>();

            if (xpOrb != null)
            {
                xpOrb.SetXPValue(xpPerOrb);
            }
        }
    }

    private void TryDropHealthPickup()
    {
        if (healthPickupPrefab == null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerStats playerStats = null;
        Health playerHealth = null;

        if (playerObject != null)
        {
            playerStats = playerObject.GetComponent<PlayerStats>();
            playerHealth = playerObject.GetComponent<Health>();
        }

        if (onlyDropHealthWhenPlayerNotFull && playerHealth != null)
        {
            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                return;
            }
        }

        float finalDropChance = healthPickupDropChance;

        if (playerStats != null)
        {
            finalDropChance = playerStats.GetFinalHealthPickupDropChance(healthPickupDropChance);
        }

        if (Random.value > finalDropChance)
        {
            return;
        }

        Vector3 spawnPosition = GetRandomScatterPosition();

        GameObject pickupObject = SpawnObject(healthPickupPrefab, spawnPosition);

        HealthPickup healthPickup = pickupObject.GetComponent<HealthPickup>();

        if (healthPickup != null)
        {
            healthPickup.SetHealAmount(healthPickupHealAmount);
        }
    }

    private Vector3 GetRandomScatterPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
        return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    private GameObject SpawnObject(GameObject prefab, Vector3 spawnPosition)
    {
        GameObject spawnedObject = null;

        if (PoolManager.HasInstance)
        {
            spawnedObject = PoolManager.Instance.Spawn(
                prefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (spawnedObject == null)
        {
            spawnedObject = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        return spawnedObject;
    }
}
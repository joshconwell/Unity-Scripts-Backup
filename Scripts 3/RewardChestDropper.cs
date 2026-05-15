using UnityEngine;

[RequireComponent(typeof(Health))]
public class RewardChestDropper : MonoBehaviour
{
    [Header("Chest Drop")]
    [SerializeField] private GameObject rewardChestPrefab;
    [SerializeField] private SpecialRewardTier rewardTier = SpecialRewardTier.Elite;

    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    [Header("Spawn Position")]
    [SerializeField] private float scatterRadius = 0.35f;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
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

    private void HandleDied()
    {
        TryDropChest();
    }

    private void TryDropChest()
    {
        if (rewardChestPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing a Reward Chest Prefab.");
            return;
        }

        if (Random.value > dropChance)
        {
            return;
        }

        Vector3 spawnPosition = GetChestSpawnPosition();

        GameObject chestObject = null;

        if (PoolManager.HasInstance)
        {
            chestObject = PoolManager.Instance.Spawn(
                rewardChestPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (chestObject == null)
        {
            chestObject = Instantiate(
                rewardChestPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        RewardChest rewardChest = chestObject.GetComponent<RewardChest>();

        if (rewardChest != null)
        {
            rewardChest.SetRewardTier(rewardTier);
        }
    }

    private Vector3 GetChestSpawnPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;

        return transform.position
            + spawnOffset
            + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }
}
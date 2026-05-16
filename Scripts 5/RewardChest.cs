using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RewardChest : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private SpecialRewardTier rewardTier = SpecialRewardTier.Elite;

    [Header("Collection")]
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;
    private Collider2D chestCollider;
    private PooledObject pooledObject;

    private bool opened;

    public SpecialRewardTier RewardTier => rewardTier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        chestCollider = GetComponent<Collider2D>();
        pooledObject = GetComponent<PooledObject>();

        if (chestCollider != null)
        {
            chestCollider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        opened = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        TryOpenChest();
    }

    public void SetRewardTier(SpecialRewardTier newRewardTier)
    {
        rewardTier = newRewardTier;
    }

    private void TryOpenChest()
    {
        if (opened)
        {
            return;
        }

        SpecialUpgradeManager manager = null;

        if (SpecialUpgradeManager.HasInstance)
        {
            manager = SpecialUpgradeManager.Instance;
        }
        else
        {
            manager = FindObjectOfType<SpecialUpgradeManager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("RewardChest could not find SpecialUpgradeManager.");
            return;
        }

        bool openedSuccessfully = manager.ShowRewardChoices(rewardTier);

        if (!openedSuccessfully)
        {
            return;
        }

        opened = true;
        DeactivateChest();
    }

    private void DeactivateChest()
    {
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
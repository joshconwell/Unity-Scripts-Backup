using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private float healAmount = 15f;

    [Header("Magnet Settings")]
    [SerializeField] private float attractionRange = 4f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float collectRadius = 0.65f;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;
    private Transform player;
    private Health playerHealth;
    private PlayerStats playerStats;
    private PooledObject pooledObject;

    private bool collected;

    public float HealAmount => healAmount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();

        Collider2D pickupCollider = GetComponent<Collider2D>();

        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        collected = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        if (player == null)
        {
            FindPlayer();
        }
    }

    private void Start()
    {
        if (player == null)
        {
            FindPlayer();
        }
    }

    private void FixedUpdate()
    {
        if (collected)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        MoveTowardPlayerIfClose();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;
        playerHealth = playerObject.GetComponent<Health>();
        playerStats = playerObject.GetComponent<PlayerStats>();
    }

    private void MoveTowardPlayerIfClose()
    {
        Vector2 pickupPosition = rb.position;
        Vector2 playerPosition = player.position;

        float currentAttractionRange = attractionRange;
        float currentCollectRadius = collectRadius;
        float currentMoveSpeed = moveSpeed;

        if (playerStats != null)
        {
            currentAttractionRange = playerStats.XPMagnetRange;
            currentCollectRadius = Mathf.Max(collectRadius, playerStats.XPCollectRadius);
            currentMoveSpeed = moveSpeed * playerStats.XPMagnetSpeedMultiplier;
        }

        if (currentAttractionRange < 0f)
        {
            currentAttractionRange = 0f;
        }

        if (currentCollectRadius < 0.05f)
        {
            currentCollectRadius = 0.05f;
        }

        float distanceToPlayer = Vector2.Distance(pickupPosition, playerPosition);

        if (distanceToPlayer <= currentCollectRadius)
        {
            CollectPickup();
            return;
        }

        if (distanceToPlayer > currentAttractionRange)
        {
            return;
        }

        Vector2 direction = (playerPosition - pickupPosition).normalized;
        Vector2 newPosition = pickupPosition + direction * currentMoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        player = other.transform;
        playerHealth = other.GetComponent<Health>();
        playerStats = other.GetComponent<PlayerStats>();

        CollectPickup();
    }

    private void CollectPickup()
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        if (playerStats == null && player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerHealth != null)
        {
            float finalHealAmount = healAmount;

            if (playerStats != null)
            {
                finalHealAmount = playerStats.GetFinalHealthPickupHealAmount(healAmount);
            }

            playerHealth.Heal(finalHealAmount);
        }

        DeactivatePickup();
    }

    public void SetHealAmount(float newHealAmount)
    {
        healAmount = Mathf.Max(1f, newHealAmount);
    }

    private void DeactivatePickup()
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
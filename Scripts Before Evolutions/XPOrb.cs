using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpValue = 1;

    [Header("Magnet Settings")]
    [Tooltip("Fallback range used if the player does not have PlayerStats.")]
    [SerializeField] private float attractionRange = 4f;

    [Tooltip("Base movement speed before PlayerStats magnet speed multiplier is applied.")]
    [SerializeField] private float moveSpeed = 8f;

    [Tooltip("Fallback collect radius used if the player does not have PlayerStats.")]
    [SerializeField] private float collectRadius = 0.65f;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerStats playerStats;
    private PlayerExperience playerExperience;
    private PooledObject pooledObject;

    private bool collected;

    public int XPValue => xpValue;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();

        Collider2D orbCollider = GetComponent<Collider2D>();
        orbCollider.isTrigger = true;
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
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
            playerExperience = playerObject.GetComponent<PlayerExperience>();
        }
    }

    private void MoveTowardPlayerIfClose()
    {
        Vector2 orbPosition = rb.position;
        Vector2 playerPosition = player.position;

        float currentAttractionRange = attractionRange;
        float currentCollectRadius = collectRadius;
        float currentMoveSpeed = moveSpeed;

        if (playerStats != null)
        {
            currentAttractionRange = playerStats.XPMagnetRange;
            currentCollectRadius = playerStats.XPCollectRadius;
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

        float distanceToPlayer = Vector2.Distance(orbPosition, playerPosition);

        if (distanceToPlayer <= currentCollectRadius)
        {
            CollectOrb();
            return;
        }

        if (distanceToPlayer > currentAttractionRange)
        {
            return;
        }

        Vector2 direction = (playerPosition - orbPosition).normalized;
        Vector2 newPosition = orbPosition + direction * currentMoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        player = other.transform;
        playerStats = other.GetComponent<PlayerStats>();
        playerExperience = other.GetComponent<PlayerExperience>();

        CollectOrb();
    }

    private void CollectOrb()
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (playerExperience == null && player != null)
        {
            playerExperience = player.GetComponent<PlayerExperience>();
        }

        if (playerStats == null && player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerExperience != null)
        {
            int finalXPValue = xpValue;

            if (playerStats != null)
            {
                finalXPValue = playerStats.GetFinalExperienceAmount(xpValue);
            }

            playerExperience.AddExperience(finalXPValue);
        }

        DeactivateOrb();
    }

    public void SetXPValue(int newXPValue)
    {
        xpValue = Mathf.Max(1, newXPValue);
    }

    private void DeactivateOrb()
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
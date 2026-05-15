using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpValue = 1;

    [Header("Magnet Settings")]
    [SerializeField] private float attractionRange = 4f;
    [SerializeField] private float moveSpeed = 8f;

    private Rigidbody2D rb;
    private Transform player;
    private PooledObject pooledObject;

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
        }
    }

    private void MoveTowardPlayerIfClose()
    {
        Vector2 orbPosition = rb.position;
        Vector2 playerPosition = player.position;

        float distanceToPlayer = Vector2.Distance(orbPosition, playerPosition);

        if (distanceToPlayer > attractionRange)
        {
            return;
        }

        Vector2 direction = (playerPosition - orbPosition).normalized;
        Vector2 newPosition = orbPosition + direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerExperience playerExperience = other.GetComponent<PlayerExperience>();

        if (playerExperience != null)
        {
            playerExperience.AddExperience(xpValue);
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
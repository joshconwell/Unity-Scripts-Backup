using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private Vector2 moveDirection = Vector2.right;
    private float lifeTimer;
    private bool hasLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();

        Collider2D projectileCollider = GetComponent<Collider2D>();
        projectileCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasLaunched = false;

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }
    }

    private void Update()
    {
        if (!hasLaunched)
        {
            return;
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifetime)
        {
            DeactivateProjectile();
        }
    }

    private void FixedUpdate()
    {
        if (!hasLaunched)
        {
            return;
        }

        Vector2 newPosition = rb.position + moveDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasLaunched)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Health playerHealth = other.GetComponent<Health>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        DeactivateProjectile();
    }

    public void Launch(Vector2 direction, float newSpeed, float newDamage, float newLifetime)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector2.right;
        }

        moveDirection = direction.normalized;
        speed = newSpeed;
        damage = newDamage;
        lifetime = newLifetime;
        lifeTimer = 0f;
        hasLaunched = true;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void DeactivateProjectile()
    {
        hasLaunched = false;

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
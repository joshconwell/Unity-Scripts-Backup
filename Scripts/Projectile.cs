using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 2.5f;

    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private Vector2 moveDirection = Vector2.right;
    private float lifeTimer;
    private bool hasLaunched;
    private bool isCriticalHit;

    public float Damage => damage;
    public bool IsCriticalHit => isCriticalHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasLaunched = false;
        isCriticalHit = false;

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

        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        Health enemyHealth = other.GetComponent<Health>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, isCriticalHit);
        }

        DeactivateProjectile();
    }

    public void Launch(Vector2 direction, float newSpeed, float newDamage, float newLifetime, bool criticalHit)
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
        isCriticalHit = criticalHit;

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
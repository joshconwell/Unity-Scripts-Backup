using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Pierce")]
    [Tooltip("How many extra enemies this projectile can pass through after the first hit.")]
    [SerializeField] private int pierceCount = 0;

    [Header("Size")]
    [Tooltip("1 = normal size. This is normally controlled by PlayerStats when launched.")]
    [SerializeField] private float sizeMultiplier = 1f;

    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private Vector2 baseLocalScale;
    private Vector2 moveDirection = Vector2.right;

    private float lifeTimer;
    private bool hasLaunched;
    private bool isCriticalHit;
    private int remainingPierce;

    private PlayerSpecialAbilities playerSpecialAbilities;

    private readonly HashSet<Health> hitEnemies = new HashSet<Health>();

    public float Damage => damage;
    public bool IsCriticalHit => isCriticalHit;
    public int PierceCount => pierceCount;
    public float SizeMultiplier => sizeMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
        baseLocalScale = transform.localScale;
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasLaunched = false;
        isCriticalHit = false;
        remainingPierce = 0;
        sizeMultiplier = 1f;
        playerSpecialAbilities = null;
        hitEnemies.Clear();

        transform.localScale = baseLocalScale;

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

        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<Health>();
        }

        if (enemyHealth == null)
        {
            return;
        }

        if (hitEnemies.Contains(enemyHealth))
        {
            return;
        }

        hitEnemies.Add(enemyHealth);

        enemyHealth.TakeDamage(damage, isCriticalHit);

        if (playerSpecialAbilities != null)
        {
            playerSpecialAbilities.TriggerExplosiveShot(
                transform.position,
                damage,
                enemyHealth,
                isCriticalHit
            );
        }

        if (remainingPierce > 0)
        {
            remainingPierce--;
            return;
        }

        DeactivateProjectile();
    }

    public void Launch(Vector2 direction, float newSpeed, float newDamage, float newLifetime, bool criticalHit)
    {
        Launch(direction, newSpeed, newDamage, newLifetime, criticalHit, 0, 1f, null);
    }

    public void Launch(Vector2 direction, float newSpeed, float newDamage, float newLifetime, bool criticalHit, int newPierceCount)
    {
        Launch(direction, newSpeed, newDamage, newLifetime, criticalHit, newPierceCount, 1f, null);
    }

    public void Launch(Vector2 direction, float newSpeed, float newDamage, float newLifetime, bool criticalHit, int newPierceCount, float newSizeMultiplier)
    {
        Launch(direction, newSpeed, newDamage, newLifetime, criticalHit, newPierceCount, newSizeMultiplier, null);
    }

    public void Launch(
        Vector2 direction,
        float newSpeed,
        float newDamage,
        float newLifetime,
        bool criticalHit,
        int newPierceCount,
        float newSizeMultiplier,
        PlayerSpecialAbilities newPlayerSpecialAbilities
    )
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

        pierceCount = Mathf.Max(0, newPierceCount);
        remainingPierce = pierceCount;

        sizeMultiplier = Mathf.Max(0.25f, newSizeMultiplier);
        transform.localScale = baseLocalScale * sizeMultiplier;

        playerSpecialAbilities = newPlayerSpecialAbilities;

        hitEnemies.Clear();

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void DeactivateProjectile()
    {
        hasLaunched = false;
        hitEnemies.Clear();

        transform.localScale = baseLocalScale;
        playerSpecialAbilities = null;

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
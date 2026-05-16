using UnityEngine;

public class EnemyDespawnByDistance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Despawn Settings")]
    [SerializeField] private float despawnDistance = 40f;
    [SerializeField] private float minimumLifetime = 3f;
    [SerializeField] private float checkInterval = 1f;

    [Header("Camera Safety")]
    [SerializeField] private bool requireOutsideCamera = true;
    [SerializeField] private float viewportPadding = 0.2f;

    private float lifeTimer;
    private float nextCheckTime;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (player == null)
        {
            FindPlayer();
        }
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        nextCheckTime = Time.time + checkInterval;

        if (player == null)
        {
            FindPlayer();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer < minimumLifetime)
        {
            return;
        }

        if (Time.time < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.time + checkInterval;

        CheckForDespawn();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void CheckForDespawn()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                return;
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < despawnDistance)
        {
            return;
        }

        if (requireOutsideCamera && IsInsideCameraView())
        {
            return;
        }

        DespawnEnemy();
    }

    private bool IsInsideCameraView()
    {
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        bool insideX = viewportPosition.x >= -viewportPadding && viewportPosition.x <= 1f + viewportPadding;
        bool insideY = viewportPosition.y >= -viewportPadding && viewportPosition.y <= 1f + viewportPadding;
        bool inFrontOfCamera = viewportPosition.z > 0f;

        return insideX && insideY && inFrontOfCamera;
    }

    private void DespawnEnemy()
    {
        PooledObject pooledObject = GetComponent<PooledObject>();

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, despawnDistance);
    }
}
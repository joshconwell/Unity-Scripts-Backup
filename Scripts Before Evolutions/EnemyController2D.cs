using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Target")]
    [SerializeField] private Transform target;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        MoveTowardTarget();
        RotateTowardTarget();
    }

    private void MoveTowardTarget()
    {
        Vector2 enemyPosition = rb.position;
        Vector2 targetPosition = target.position;

        Vector2 direction = targetPosition - enemyPosition;
        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            return;
        }

        Vector2 moveDirection = direction.normalized;
        Vector2 newPosition = enemyPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void RotateTowardTarget()
    {
        Vector2 enemyPosition = rb.position;
        Vector2 targetPosition = target.position;

        Vector2 direction = targetPosition - enemyPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }
}
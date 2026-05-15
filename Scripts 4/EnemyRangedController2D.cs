using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRangedController2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float distanceTolerance = 1.25f;

    [Header("Strafing")]
    [SerializeField] private bool useStrafing = true;
    [SerializeField] private float strafeSpeedMultiplier = 0.45f;
    [SerializeField] private float strafeDirectionChangeInterval = 2f;

    private Rigidbody2D rb;
    private float strafeDirection = 1f;
    private float nextStrafeDirectionChangeTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        FindTargetIfNeeded();
    }

    private void OnEnable()
    {
        FindTargetIfNeeded();
        PickNewStrafeDirection();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            FindTargetIfNeeded();

            if (target == null)
            {
                return;
            }
        }

        MoveRelativeToTarget();
        RotateTowardTarget();
    }

    private void FindTargetIfNeeded()
    {
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void MoveRelativeToTarget()
    {
        Vector2 enemyPosition = rb.position;
        Vector2 targetPosition = target.position;

        Vector2 toTarget = targetPosition - enemyPosition;
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= 0.001f)
        {
            return;
        }

        Vector2 directionToTarget = toTarget.normalized;
        Vector2 moveDirection = Vector2.zero;

        float tooCloseDistance = preferredDistance - distanceTolerance;
        float tooFarDistance = preferredDistance + distanceTolerance;

        if (distanceToTarget > tooFarDistance)
        {
            moveDirection += directionToTarget;
        }
        else if (distanceToTarget < tooCloseDistance)
        {
            moveDirection -= directionToTarget;
        }

        if (useStrafing)
        {
            if (Time.time >= nextStrafeDirectionChangeTime)
            {
                PickNewStrafeDirection();
            }

            Vector2 strafeDirectionVector = new Vector2(-directionToTarget.y, directionToTarget.x);
            moveDirection += strafeDirectionVector * strafeDirection * strafeSpeedMultiplier;
        }

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        moveDirection.Normalize();

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

    private void PickNewStrafeDirection()
    {
        strafeDirection = Random.value < 0.5f ? -1f : 1f;
        nextStrafeDirectionChangeTime = Time.time + strafeDirectionChangeInterval;
    }
}
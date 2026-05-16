using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyForcedRetreat : MonoBehaviour
{
    private Rigidbody2D rb;
    private PooledObject pooledObject;
    private Coroutine retreatRoutine;

    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();

    private bool controlsAreDisabled;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        // Safety reset for pooled enemies.
        // If an enemy was returned to the pool during a retreat, make sure it wakes up clean next time.
        ReEnableEnemyControlScripts();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        retreatRoutine = null;
    }

    public void BeginRetreat(Transform fleeFromTarget, float retreatSpeed, float retreatDuration)
    {
        // This is the important fix:
        // If BeginRetreat gets called twice on the same enemy, restore its scripts before restarting.
        if (retreatRoutine != null)
        {
            StopCoroutine(retreatRoutine);
            retreatRoutine = null;
        }

        ReEnableEnemyControlScripts();

        retreatRoutine = StartCoroutine(RetreatRoutine(fleeFromTarget, retreatSpeed, retreatDuration));
    }

    private IEnumerator RetreatRoutine(Transform fleeFromTarget, float retreatSpeed, float retreatDuration)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (pooledObject == null)
        {
            pooledObject = GetComponent<PooledObject>();
        }

        DisableEnemyControlScripts();

        Vector2 retreatDirection = GetRetreatDirection(fleeFromTarget);
        float elapsedTime = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        while (elapsedTime < retreatDuration)
        {
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            float deltaTime = Time.deltaTime;

            if (rb != null)
            {
                Vector2 newPosition = rb.position + retreatDirection * retreatSpeed * deltaTime;
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position += new Vector3(
                    retreatDirection.x,
                    retreatDirection.y,
                    0f
                ) * retreatSpeed * deltaTime;
            }

            elapsedTime += deltaTime;
            yield return null;
        }

        // Another important part:
        // Re-enable BEFORE returning to pool so the next spawn is not frozen.
        ReEnableEnemyControlScripts();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        retreatRoutine = null;

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private Vector2 GetRetreatDirection(Transform fleeFromTarget)
    {
        if (fleeFromTarget == null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            if (randomDirection.sqrMagnitude <= 0.001f)
            {
                randomDirection = Vector2.right;
            }

            return randomDirection;
        }

        Vector2 awayDirection = transform.position - fleeFromTarget.position;

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            awayDirection = Random.insideUnitCircle.normalized;
        }

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            awayDirection = Vector2.right;
        }

        return awayDirection.normalized;
    }

    private void DisableEnemyControlScripts()
    {
        if (controlsAreDisabled)
        {
            return;
        }

        disabledBehaviours.Clear();

        Behaviour[] behaviours = GetComponents<Behaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            if (behaviour == this)
            {
                continue;
            }

            if (!behaviour.enabled)
            {
                continue;
            }

            // Keep Health and pooling alive.
            if (behaviour is Health)
            {
                continue;
            }

            if (behaviour is PooledObject)
            {
                continue;
            }

            behaviour.enabled = false;
            disabledBehaviours.Add(behaviour);
        }

        controlsAreDisabled = true;
    }

    private void ReEnableEnemyControlScripts()
    {
        for (int i = 0; i < disabledBehaviours.Count; i++)
        {
            if (disabledBehaviours[i] != null)
            {
                disabledBehaviours[i].enabled = true;
            }
        }

        disabledBehaviours.Clear();
        controlsAreDisabled = false;
    }

    private void OnDisable()
    {
        if (retreatRoutine != null)
        {
            StopCoroutine(retreatRoutine);
            retreatRoutine = null;
        }

        ReEnableEnemyControlScripts();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
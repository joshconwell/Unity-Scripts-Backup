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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
    }

    public void BeginRetreat(Transform fleeFromTarget, float retreatSpeed, float retreatDuration)
    {
        if (retreatRoutine != null)
        {
            StopCoroutine(retreatRoutine);
        }

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

        while (elapsedTime < retreatDuration)
        {
            if (rb != null)
            {
                Vector2 newPosition = rb.position + retreatDirection * retreatSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position += new Vector3(retreatDirection.x, retreatDirection.y, 0f) * retreatSpeed * Time.deltaTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReEnableEnemyControlScripts();

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            gameObject.SetActive(false);
        }

        retreatRoutine = null;
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
    }

    private void OnDisable()
    {
        ReEnableEnemyControlScripts();

        if (retreatRoutine != null)
        {
            StopCoroutine(retreatRoutine);
            retreatRoutine = null;
        }
    }
}
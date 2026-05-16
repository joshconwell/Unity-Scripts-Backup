using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 0.75f;

    private float nextDamageTime;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        Health playerHealth = collision.gameObject.GetComponent<Health>();

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(damageAmount);

        nextDamageTime = Time.time + damageInterval;
    }
}
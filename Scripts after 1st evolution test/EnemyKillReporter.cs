using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyKillReporter : MonoBehaviour
{
    [Header("Kill Settings")]
    [SerializeField] private int killValue = 1;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += ReportKill;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= ReportKill;
        }
    }

    private void ReportKill()
    {
        if (GameRunStats.HasInstance)
        {
            GameRunStats.Instance.RegisterEnemyKill(killValue);
        }
    }
}
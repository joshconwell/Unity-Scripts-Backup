using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageNumberSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject damageNumberPrefab;

    [Header("Normal Damage Number")]
    [SerializeField] private Color damageNumberColor = Color.white;
    [SerializeField] private int fontSize = 48;

    [Header("Critical Damage Number")]
    [SerializeField] private Color criticalDamageNumberColor = Color.yellow;
    [SerializeField] private int criticalFontSize = 62;

    [Header("Position")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private float randomOffsetRadius = 0.25f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamagedDetailed += SpawnDamageNumber;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamagedDetailed -= SpawnDamageNumber;
        }
    }

    private void SpawnDamageNumber(float damageAmount, bool isCriticalHit)
    {
        if (damageAmount <= 0f)
        {
            return;
        }

        if (damageNumberPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing a Damage Number prefab.");
            return;
        }

        Vector2 randomOffset = Random.insideUnitCircle * randomOffsetRadius;

        Vector3 spawnPosition = transform.position
            + spawnOffset
            + new Vector3(randomOffset.x, randomOffset.y, 0f);

        GameObject damageNumberObject = null;

        if (PoolManager.HasInstance)
        {
            damageNumberObject = PoolManager.Instance.Spawn(
                damageNumberPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (damageNumberObject == null)
        {
            damageNumberObject = Instantiate(
                damageNumberPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        DamageNumber damageNumber = damageNumberObject.GetComponent<DamageNumber>();

        if (damageNumber != null)
        {
            Color numberColor = isCriticalHit ? criticalDamageNumberColor : damageNumberColor;
            int numberFontSize = isCriticalHit ? criticalFontSize : fontSize;

            damageNumber.Initialize(damageAmount, numberColor, numberFontSize, isCriticalHit);
        }
    }
}
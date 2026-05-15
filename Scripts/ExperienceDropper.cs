using UnityEngine;

[RequireComponent(typeof(Health))]
public class ExperienceDropper : MonoBehaviour
{
    [Header("XP Drop Settings")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpAmount = 1;
    [SerializeField] private int orbCount = 1;
    [SerializeField] private float scatterRadius = 0.35f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += DropExperience;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= DropExperience;
        }
    }

    private void DropExperience()
    {
        if (xpOrbPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing an XP Orb prefab.");
            return;
        }

        int safeOrbCount = Mathf.Max(1, orbCount);
        int xpPerOrb = Mathf.Max(1, xpAmount / safeOrbCount);

        for (int i = 0; i < safeOrbCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject orbObject = null;

            if (PoolManager.HasInstance)
            {
                orbObject = PoolManager.Instance.Spawn(
                    xpOrbPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
            }

            if (orbObject == null)
            {
                orbObject = Instantiate(
                    xpOrbPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
            }

            XPOrb xpOrb = orbObject.GetComponent<XPOrb>();

            if (xpOrb != null)
            {
                xpOrb.SetXPValue(xpPerOrb);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    private class EnemySpawnEntry
    {
        public string enemyName;
        public GameObject enemyPrefab;
        [Min(0f)] public float spawnWeight = 1f;
        [Min(0f)] public float unlockTime = 0f;
    }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;

    [Header("Enemy Spawn Pool")]
    [SerializeField] private EnemySpawnEntry[] enemySpawnEntries;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int enemiesPerSpawn = 1;
    [SerializeField] private int maxAliveEnemies = 35;
    [SerializeField] private float spawnPadding = 2f;

    [Header("Difficulty Over Time")]
    [SerializeField] private bool increaseSpawnRateOverTime = true;
    [SerializeField] private float spawnIntervalReductionPerMinute = 0.15f;
    [SerializeField] private float minimumSpawnInterval = 0.35f;
    [SerializeField] private bool increaseEnemiesPerSpawnOverTime = true;
    [SerializeField] private float enemiesPerSpawnIncreaseEverySeconds = 45f;
    [SerializeField] private int maxEnemiesPerSpawn = 5;

    [Header("Debug")]
    [SerializeField] private bool spawnOnStart = true;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private readonly List<EnemySpawnEntry> availableEntries = new List<EnemySpawnEntry>();

    private float spawnTimer;
    private float elapsedTime;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Start()
    {
        spawnTimer = GetCurrentSpawnInterval();

        if (spawnOnStart)
        {
            SpawnEnemyBurst();
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        CleanupEnemyList();

        if (mainCamera == null)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        if (aliveEnemies.Count >= maxAliveEnemies)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemyBurst();
            spawnTimer = GetCurrentSpawnInterval();
        }
    }

    private void SpawnEnemyBurst()
    {
        int currentEnemiesPerSpawn = GetCurrentEnemiesPerSpawn();

        for (int i = 0; i < currentEnemiesPerSpawn; i++)
        {
            if (aliveEnemies.Count >= maxAliveEnemies)
            {
                return;
            }

            GameObject enemyPrefab = GetRandomAvailableEnemyPrefab();

            if (enemyPrefab == null)
            {
                Debug.LogWarning("EnemySpawner has no available enemy prefab to spawn.");
                return;
            }

            SpawnEnemy(enemyPrefab);
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPosition = GetSpawnPositionOutsideCamera();

        GameObject enemyObject = null;

        if (PoolManager.HasInstance)
        {
            enemyObject = PoolManager.Instance.Spawn(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (enemyObject == null)
        {
            enemyObject = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (enemyObject != null)
        {
            aliveEnemies.Add(enemyObject);
        }
    }

    private GameObject GetRandomAvailableEnemyPrefab()
    {
        BuildAvailableEnemyList();

        if (availableEntries.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < availableEntries.Count; i++)
        {
            totalWeight += Mathf.Max(0f, availableEntries[i].spawnWeight);
        }

        if (totalWeight <= 0f)
        {
            return availableEntries[0].enemyPrefab;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < availableEntries.Count; i++)
        {
            currentWeight += Mathf.Max(0f, availableEntries[i].spawnWeight);

            if (randomValue <= currentWeight)
            {
                return availableEntries[i].enemyPrefab;
            }
        }

        return availableEntries[availableEntries.Count - 1].enemyPrefab;
    }

    private void BuildAvailableEnemyList()
    {
        availableEntries.Clear();

        if (enemySpawnEntries == null)
        {
            return;
        }

        for (int i = 0; i < enemySpawnEntries.Length; i++)
        {
            EnemySpawnEntry entry = enemySpawnEntries[i];

            if (entry == null)
            {
                continue;
            }

            if (entry.enemyPrefab == null)
            {
                continue;
            }

            if (elapsedTime < entry.unlockTime)
            {
                continue;
            }

            availableEntries.Add(entry);
        }
    }

    private float GetCurrentSpawnInterval()
    {
        if (!increaseSpawnRateOverTime)
        {
            return spawnInterval;
        }

        float minutesElapsed = elapsedTime / 60f;
        float reduction = minutesElapsed * spawnIntervalReductionPerMinute;

        float currentInterval = spawnInterval - reduction;

        if (currentInterval < minimumSpawnInterval)
        {
            currentInterval = minimumSpawnInterval;
        }

        return currentInterval;
    }

    private int GetCurrentEnemiesPerSpawn()
    {
        if (!increaseEnemiesPerSpawnOverTime)
        {
            return enemiesPerSpawn;
        }

        int bonusEnemies = Mathf.FloorToInt(elapsedTime / enemiesPerSpawnIncreaseEverySeconds);
        int currentEnemiesPerSpawn = enemiesPerSpawn + bonusEnemies;

        if (currentEnemiesPerSpawn > maxEnemiesPerSpawn)
        {
            currentEnemiesPerSpawn = maxEnemiesPerSpawn;
        }

        if (currentEnemiesPerSpawn < 1)
        {
            currentEnemiesPerSpawn = 1;
        }

        return currentEnemiesPerSpawn;
    }

    private Vector3 GetSpawnPositionOutsideCamera()
    {
        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;

        Vector3 cameraPosition = mainCamera.transform.position;

        float leftEdge = cameraPosition.x - cameraHalfWidth;
        float rightEdge = cameraPosition.x + cameraHalfWidth;
        float bottomEdge = cameraPosition.y - cameraHalfHeight;
        float topEdge = cameraPosition.y + cameraHalfHeight;

        int side = Random.Range(0, 4);

        float spawnX = 0f;
        float spawnY = 0f;

        switch (side)
        {
            case 0:
                spawnX = leftEdge - spawnPadding;
                spawnY = Random.Range(bottomEdge, topEdge);
                break;

            case 1:
                spawnX = rightEdge + spawnPadding;
                spawnY = Random.Range(bottomEdge, topEdge);
                break;

            case 2:
                spawnX = Random.Range(leftEdge, rightEdge);
                spawnY = bottomEdge - spawnPadding;
                break;

            case 3:
                spawnX = Random.Range(leftEdge, rightEdge);
                spawnY = topEdge + spawnPadding;
                break;
        }

        return new Vector3(spawnX, spawnY, 0f);
    }

    private void CleanupEnemyList()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
                continue;
            }

            if (!aliveEnemies[i].activeInHierarchy)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
}
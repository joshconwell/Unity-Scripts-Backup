using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBossEncounterDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BossArenaLock bossArenaLock;
    [SerializeField] private Transform player;

    [Header("Mini Boss")]
    [SerializeField] private GameObject miniBossPrefab;

    [Tooltip("Optional. Add things like EliteSpawnDirector here so they pause during the mini-boss fight.")]
    [SerializeField] private Behaviour[] behavioursToDisableDuringEncounter;

    [Header("Encounter Timing")]
    [SerializeField] private float firstBossEventTime = 180f;
    [SerializeField] private float bossEventInterval = 180f;
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Warning")]
    [SerializeField] private GameObject warningObject;
    [SerializeField] private float warningDuration = 2f;

    [Header("Enemy Evacuation")]
    [SerializeField] private bool retreatAllTaggedEnemiesInScene = true;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float enemyRetreatSpeed = 12f;
    [SerializeField] private float enemyRetreatDuration = 1.25f;
    [SerializeField] private float bossSpawnDelayAfterRetreat = 1f;

    [Header("Boss Spawn Position")]
    [SerializeField] private float arenaEdgePadding = 2f;
    [SerializeField] private float minimumBossSpawnDistanceFromPlayer = 6f;
    [SerializeField] private int bossSpawnPositionAttempts = 20;

    [Header("Encounter End")]
    [SerializeField] private float unlockDelayAfterBossDeath = 1f;

    [Header("Debug")]
    [SerializeField] private bool allowDebugTriggerKey = true;
    [SerializeField] private KeyCode debugTriggerKey = KeyCode.F8;
    [SerializeField] private bool autoTriggerByTime = true;

    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();

    private float elapsedTime;
    private float nextBossEventTime;

    private bool encounterActive;
    private bool encounterHasTriggered;
    private bool bossHasSpawned;
    private bool endingEncounter;

    private GameObject currentBossObject;
    private Health currentBossHealth;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (warningObject != null)
        {
            warningObject.SetActive(false);
        }
    }

    private void Start()
    {
        nextBossEventTime = firstBossEventTime;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (allowDebugTriggerKey && Input.GetKeyDown(debugTriggerKey))
        {
            TryStartMiniBossEvent();
        }

        if (!autoTriggerByTime)
        {
            return;
        }

        if (triggerOnlyOnce && encounterHasTriggered)
        {
            return;
        }

        if (!encounterActive && elapsedTime >= nextBossEventTime)
        {
            TryStartMiniBossEvent();
        }
    }

    public void TryStartMiniBossEvent()
    {
        if (encounterActive)
        {
            return;
        }

        if (triggerOnlyOnce && encounterHasTriggered)
        {
            return;
        }

        StartCoroutine(MiniBossEventRoutine());
    }

    public void EndMiniBossEvent()
    {
        if (endingEncounter)
        {
            return;
        }

        StartCoroutine(EndMiniBossEventRoutine());
    }

    private IEnumerator MiniBossEventRoutine()
    {
        FindReferencesIfNeeded();

        encounterActive = true;
        encounterHasTriggered = true;
        bossHasSpawned = false;
        endingEncounter = false;

        Debug.Log("Mini boss event started.");

        DisableEncounterBehaviours();

        if (warningObject != null)
        {
            warningObject.SetActive(true);
        }

        if (enemySpawner != null)
        {
            enemySpawner.PauseSpawning();
            enemySpawner.MakeAllEnemiesLeave(player, enemyRetreatSpeed, enemyRetreatDuration);
        }

        if (retreatAllTaggedEnemiesInScene)
        {
            MakeAllTaggedEnemiesInSceneLeave();
        }

        if (bossArenaLock != null)
        {
            bossArenaLock.LockArenaAroundPlayer();
        }

        float totalPreBossDelay = Mathf.Max(warningDuration, enemyRetreatDuration + bossSpawnDelayAfterRetreat);

        if (totalPreBossDelay > 0f)
        {
            yield return new WaitForSeconds(totalPreBossDelay);
        }

        if (warningObject != null)
        {
            warningObject.SetActive(false);
        }

        SpawnMiniBoss();
    }

    private void SpawnMiniBoss()
    {
        if (miniBossPrefab == null)
        {
            Debug.LogWarning("MiniBossEncounterDirector is missing a mini boss prefab.");
            EndMiniBossEvent();
            return;
        }

        Vector3 spawnPosition = GetMiniBossSpawnPosition();

        GameObject bossObject = null;

        if (PoolManager.HasInstance)
        {
            bossObject = PoolManager.Instance.Spawn(
                miniBossPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (bossObject == null)
        {
            bossObject = Instantiate(
                miniBossPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (bossObject == null)
        {
            Debug.LogWarning("MiniBossEncounterDirector failed to spawn mini boss.");
            EndMiniBossEvent();
            return;
        }

        currentBossObject = bossObject;
        currentBossHealth = currentBossObject.GetComponent<Health>();

        if (currentBossHealth == null)
        {
            currentBossHealth = currentBossObject.GetComponentInChildren<Health>();
        }

        if (currentBossHealth != null)
        {
            currentBossHealth.OnDied += HandleMiniBossDied;
        }
        else
        {
            Debug.LogWarning("Spawned mini boss has no Health component.");
        }

        bossHasSpawned = true;

        Debug.Log($"Mini boss spawned: {currentBossObject.name}");
    }

    private Vector3 GetMiniBossSpawnPosition()
    {
        FindReferencesIfNeeded();

        if (bossArenaLock == null || !bossArenaLock.ArenaLocked)
        {
            return player != null ? player.position : transform.position;
        }

        Vector3 bestPosition = bossArenaLock.GetRandomPointInsideArena(arenaEdgePadding);

        if (player == null)
        {
            return bestPosition;
        }

        float minimumDistanceSquared = minimumBossSpawnDistanceFromPlayer * minimumBossSpawnDistanceFromPlayer;

        for (int i = 0; i < bossSpawnPositionAttempts; i++)
        {
            Vector3 candidatePosition = bossArenaLock.GetRandomPointInsideArena(arenaEdgePadding);

            float distanceSquared = (candidatePosition - player.position).sqrMagnitude;

            if (distanceSquared >= minimumDistanceSquared)
            {
                return candidatePosition;
            }

            bestPosition = candidatePosition;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection.sqrMagnitude <= 0.001f)
        {
            randomDirection = Vector2.right;
        }

        Vector3 fallbackPosition = player.position + new Vector3(
            randomDirection.x,
            randomDirection.y,
            0f
        ) * minimumBossSpawnDistanceFromPlayer;

        return fallbackPosition;
    }

    private void HandleMiniBossDied()
    {
        if (currentBossHealth != null)
        {
            currentBossHealth.OnDied -= HandleMiniBossDied;
        }

        Debug.Log("Mini boss defeated.");

        EndMiniBossEvent();
    }

    private IEnumerator EndMiniBossEventRoutine()
    {
        endingEncounter = true;

        if (unlockDelayAfterBossDeath > 0f)
        {
            yield return new WaitForSeconds(unlockDelayAfterBossDeath);
        }

        if (currentBossHealth != null)
        {
            currentBossHealth.OnDied -= HandleMiniBossDied;
        }

        currentBossObject = null;
        currentBossHealth = null;
        bossHasSpawned = false;

        if (bossArenaLock != null)
        {
            bossArenaLock.UnlockArena();
        }

        if (enemySpawner != null)
        {
            enemySpawner.ResumeSpawning();
        }

        ReEnableEncounterBehaviours();

        encounterActive = false;
        endingEncounter = false;

        if (!triggerOnlyOnce)
        {
            nextBossEventTime = elapsedTime + bossEventInterval;
        }

        Debug.Log("Mini boss event ended.");
    }

    private void MakeAllTaggedEnemiesInSceneLeave()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemyObject = enemies[i];

            if (enemyObject == null)
            {
                continue;
            }

            if (!enemyObject.activeInHierarchy)
            {
                continue;
            }

            if (enemyObject == currentBossObject)
            {
                continue;
            }

            EnemyForcedRetreat forcedRetreat = enemyObject.GetComponent<EnemyForcedRetreat>();

            if (forcedRetreat == null)
            {
                forcedRetreat = enemyObject.AddComponent<EnemyForcedRetreat>();
            }

            forcedRetreat.BeginRetreat(player, enemyRetreatSpeed, enemyRetreatDuration);
        }
    }

    private void DisableEncounterBehaviours()
    {
        disabledBehaviours.Clear();

        if (behavioursToDisableDuringEncounter == null)
        {
            return;
        }

        for (int i = 0; i < behavioursToDisableDuringEncounter.Length; i++)
        {
            Behaviour behaviour = behavioursToDisableDuringEncounter[i];

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

            behaviour.enabled = false;
            disabledBehaviours.Add(behaviour);
        }
    }

    private void ReEnableEncounterBehaviours()
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

    private void FindReferencesIfNeeded()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        if (bossArenaLock == null)
        {
            bossArenaLock = FindObjectOfType<BossArenaLock>();
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

    private void OnDisable()
    {
        if (currentBossHealth != null)
        {
            currentBossHealth.OnDied -= HandleMiniBossDied;
        }

        ReEnableEncounterBehaviours();

        if (warningObject != null)
        {
            warningObject.SetActive(false);
        }
    }
}
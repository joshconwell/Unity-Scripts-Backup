using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPressureEventDirector : MonoBehaviour
{
    private enum PressureEventType
    {
        RunnerRush,
        ShooterSquad,
        BomberPanic,
        SupportCluster,
        SniperCrossfire,
        SplitterSwarm,
        ShieldWall
    }

    [System.Serializable]
    private class PressureEventSettings
    {
        public PressureEventType eventType;
        public string eventTitle = "PRESSURE EVENT";
        public string eventSubtitle = "Enemies are changing tactics.";
        public Color accentColor = new Color(1f, 0.55f, 0.12f, 1f);

        [Min(0f)] public float unlockTime = 60f;
        [Min(0f)] public float eventWeight = 1f;

        [Header("Spawn Amount")]
        [Min(1)] public int totalEnemiesToSpawn = 10;
        [Min(1)] public int enemiesPerBurst = 2;
        [Min(0.05f)] public float timeBetweenBursts = 0.45f;

        [Header("Event Behavior")]
        public bool pauseNormalSpawnerDuringEvent = false;
    }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PressureEventToastUI pressureEventToastUI;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicPrefab;
    [SerializeField] private GameObject runnerPrefab;
    [SerializeField] private GameObject brutePrefab;
    [SerializeField] private GameObject shooterPrefab;
    [SerializeField] private GameObject bomberPrefab;
    [SerializeField] private GameObject healerPrefab;
    [SerializeField] private GameObject splitterPrefab;
    [SerializeField] private GameObject sniperPrefab;
    [SerializeField] private GameObject shielderPrefab;

    [Header("Pressure Event Timing")]
    [SerializeField] private bool enablePressureEvents = true;
    [SerializeField] private float firstEventTime = 55f;
    [SerializeField] private float eventInterval = 45f;
    [SerializeField] private float eventIntervalRandomOffset = 12f;
    [SerializeField] private bool useRunStatsElapsedTime = true;

    [Header("Pressure Event Definitions")]
    [SerializeField] private PressureEventSettings[] pressureEvents;

    [Header("Spawn Position")]
    [SerializeField] private float spawnPadding = 2.75f;
    [SerializeField] private float gameplayPlaneZ = 0f;
    [SerializeField] private float fallbackSpawnDistanceFromPlayer = 18f;

    [Header("Warning Toast")]
    [SerializeField] private bool showWarningToast = true;
    [SerializeField] private float warningToastDuration = 1.45f;
    [SerializeField] private float delayAfterWarningBeforeSpawning = 0.2f;

    [Header("Mini-Boss Safety")]
    [SerializeField] private bool blockEventsWhilePaused = true;
    [SerializeField] private bool rescheduleEventAfterResume = true;
    [SerializeField] private float delayAfterResumeBeforeNextEvent = 15f;

    [Header("Debug")]
    [SerializeField] private bool allowDebugEventKey = true;
    [SerializeField] private KeyCode debugEventKey = KeyCode.F7;
    [SerializeField] private PressureEventType debugEventType = PressureEventType.RunnerRush;

    private readonly List<PressureEventSettings> availableEvents = new List<PressureEventSettings>();

    private Coroutine activeEventRoutine;

    private float elapsedTime;
    private float nextEventTime;

    private bool eventRunning;
    private bool eventsPaused;

    public bool EventRunning => eventRunning;
    public bool EventsPaused => eventsPaused;

    private void Reset()
    {
        BuildDefaultPressureEvents();
    }

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (pressureEvents == null || pressureEvents.Length == 0)
        {
            BuildDefaultPressureEvents();
        }
    }

    private void Start()
    {
        elapsedTime = GetCurrentElapsedTime();
        nextEventTime = firstEventTime;
    }

    private void Update()
    {
        FindReferencesIfNeeded();

        elapsedTime = GetCurrentElapsedTime();

        if (allowDebugEventKey && Input.GetKeyDown(debugEventKey))
        {
            StartDebugEvent();
        }

        if (!enablePressureEvents)
        {
            return;
        }

        if (eventsPaused && blockEventsWhilePaused)
        {
            return;
        }

        if (eventRunning)
        {
            return;
        }

        if (elapsedTime >= nextEventTime)
        {
            TryStartRandomPressureEvent();
        }
    }

    public void PausePressureEvents(bool cancelCurrentEvent)
    {
        eventsPaused = true;

        if (cancelCurrentEvent)
        {
            CancelActivePressureEvent();
        }
    }

    public void ResumePressureEvents()
    {
        eventsPaused = false;

        if (rescheduleEventAfterResume)
        {
            nextEventTime = GetCurrentElapsedTime() + delayAfterResumeBeforeNextEvent;
        }
    }

    public void CancelActivePressureEvent()
    {
        if (activeEventRoutine != null)
        {
            StopCoroutine(activeEventRoutine);
            activeEventRoutine = null;
        }

        if (eventRunning)
        {
            eventRunning = false;

            if (enemySpawner != null)
            {
                enemySpawner.ResumeSpawning();
            }
        }

        if (pressureEventToastUI != null)
        {
            pressureEventToastUI.HideInstant();
        }
    }

    public void TryStartRandomPressureEvent()
    {
        if (eventRunning)
        {
            return;
        }

        if (eventsPaused && blockEventsWhilePaused)
        {
            return;
        }

        PressureEventSettings selectedEvent = GetRandomAvailableEvent();

        if (selectedEvent == null)
        {
            ScheduleNextEvent();
            return;
        }

        activeEventRoutine = StartCoroutine(PressureEventRoutine(selectedEvent, false));
    }

    public void StartSpecificEventByName(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        if (pressureEvents == null)
        {
            return;
        }

        if (eventRunning)
        {
            return;
        }

        if (eventsPaused && blockEventsWhilePaused)
        {
            return;
        }

        for (int i = 0; i < pressureEvents.Length; i++)
        {
            PressureEventSettings pressureEvent = pressureEvents[i];

            if (pressureEvent == null)
            {
                continue;
            }

            if (pressureEvent.eventType.ToString().ToLower() == eventName.ToLower())
            {
                activeEventRoutine = StartCoroutine(PressureEventRoutine(pressureEvent, true));
                return;
            }

            if (!string.IsNullOrWhiteSpace(pressureEvent.eventTitle) &&
                pressureEvent.eventTitle.ToLower() == eventName.ToLower())
            {
                activeEventRoutine = StartCoroutine(PressureEventRoutine(pressureEvent, true));
                return;
            }
        }
    }

    private void StartDebugEvent()
    {
        if (eventsPaused && blockEventsWhilePaused)
        {
            Debug.Log("Pressure events are currently paused.");
            return;
        }

        PressureEventSettings selectedEvent = GetEventByType(debugEventType);

        if (selectedEvent == null)
        {
            Debug.LogWarning($"Pressure event not found: {debugEventType}");
            return;
        }

        if (eventRunning)
        {
            Debug.Log("Pressure event is already running.");
            return;
        }

        activeEventRoutine = StartCoroutine(PressureEventRoutine(selectedEvent, true));
    }

    private IEnumerator PressureEventRoutine(PressureEventSettings pressureEvent, bool ignoreUnlockTime)
    {
        if (pressureEvent == null)
        {
            yield break;
        }

        if (eventsPaused && blockEventsWhilePaused)
        {
            activeEventRoutine = null;
            yield break;
        }

        if (!ignoreUnlockTime && elapsedTime < pressureEvent.unlockTime)
        {
            ScheduleNextEvent();
            activeEventRoutine = null;
            yield break;
        }

        eventRunning = true;

        Debug.Log($"Pressure event started: {pressureEvent.eventTitle}");

        if (pressureEvent.pauseNormalSpawnerDuringEvent && enemySpawner != null)
        {
            enemySpawner.PauseSpawning();
        }

        if (showWarningToast)
        {
            FindPressureToastIfNeeded();

            if (pressureEventToastUI != null)
            {
                pressureEventToastUI.ShowEvent(
                    pressureEvent.eventTitle,
                    pressureEvent.eventSubtitle,
                    pressureEvent.accentColor,
                    warningToastDuration
                );
            }
        }

        float preSpawnDelay = warningToastDuration + delayAfterWarningBeforeSpawning;

        if (preSpawnDelay > 0f)
        {
            float timer = 0f;

            while (timer < preSpawnDelay)
            {
                if (eventsPaused && blockEventsWhilePaused)
                {
                    EndPressureEventEarly(pressureEvent);
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return SpawnPressureEventEnemies(pressureEvent);

        if (pressureEvent.pauseNormalSpawnerDuringEvent && enemySpawner != null)
        {
            enemySpawner.ResumeSpawning();
        }

        eventRunning = false;
        activeEventRoutine = null;

        ScheduleNextEvent();

        Debug.Log($"Pressure event ended: {pressureEvent.eventTitle}");
    }

    private void EndPressureEventEarly(PressureEventSettings pressureEvent)
    {
        if (pressureEvent != null && pressureEvent.pauseNormalSpawnerDuringEvent && enemySpawner != null)
        {
            enemySpawner.ResumeSpawning();
        }

        if (pressureEventToastUI != null)
        {
            pressureEventToastUI.HideInstant();
        }

        eventRunning = false;
        activeEventRoutine = null;

        Debug.Log("Pressure event cancelled because pressure events were paused.");
    }

    private IEnumerator SpawnPressureEventEnemies(PressureEventSettings pressureEvent)
    {
        int spawnedCount = 0;
        int totalToSpawn = Mathf.Max(1, pressureEvent.totalEnemiesToSpawn);
        int perBurst = Mathf.Max(1, pressureEvent.enemiesPerBurst);

        while (spawnedCount < totalToSpawn)
        {
            if (eventsPaused && blockEventsWhilePaused)
            {
                EndPressureEventEarly(pressureEvent);
                yield break;
            }

            int amountThisBurst = Mathf.Min(perBurst, totalToSpawn - spawnedCount);

            for (int i = 0; i < amountThisBurst; i++)
            {
                if (eventsPaused && blockEventsWhilePaused)
                {
                    EndPressureEventEarly(pressureEvent);
                    yield break;
                }

                GameObject prefabToSpawn = GetPrefabForPressureEvent(
                    pressureEvent.eventType,
                    spawnedCount,
                    totalToSpawn
                );

                if (prefabToSpawn != null)
                {
                    SpawnEnemy(prefabToSpawn, pressureEvent.eventType, spawnedCount, totalToSpawn);
                }

                spawnedCount++;
            }

            if (spawnedCount < totalToSpawn)
            {
                yield return new WaitForSeconds(pressureEvent.timeBetweenBursts);
            }
        }
    }

    private GameObject GetPrefabForPressureEvent(PressureEventType eventType, int spawnIndex, int totalToSpawn)
    {
        switch (eventType)
        {
            case PressureEventType.RunnerRush:
                return runnerPrefab != null ? runnerPrefab : basicPrefab;

            case PressureEventType.ShooterSquad:
                if (spawnIndex % 5 == 0 && brutePrefab != null)
                {
                    return brutePrefab;
                }

                return shooterPrefab != null ? shooterPrefab : basicPrefab;

            case PressureEventType.BomberPanic:
                if (spawnIndex % 4 == 0 && runnerPrefab != null)
                {
                    return runnerPrefab;
                }

                return bomberPrefab != null ? bomberPrefab : basicPrefab;

            case PressureEventType.SupportCluster:
                if (spawnIndex == 0 && healerPrefab != null)
                {
                    return healerPrefab;
                }

                if (spawnIndex == 1 && shielderPrefab != null)
                {
                    return shielderPrefab;
                }

                if (brutePrefab != null)
                {
                    return brutePrefab;
                }

                return basicPrefab;

            case PressureEventType.SniperCrossfire:
                if (spawnIndex % 3 == 0 && runnerPrefab != null)
                {
                    return runnerPrefab;
                }

                return sniperPrefab != null ? sniperPrefab : shooterPrefab;

            case PressureEventType.SplitterSwarm:
                if (spawnIndex % 3 == 0 && runnerPrefab != null)
                {
                    return runnerPrefab;
                }

                return splitterPrefab != null ? splitterPrefab : basicPrefab;

            case PressureEventType.ShieldWall:
                if (spawnIndex == 0 && shielderPrefab != null)
                {
                    return shielderPrefab;
                }

                if (spawnIndex == 1 && healerPrefab != null)
                {
                    return healerPrefab;
                }

                if (spawnIndex % 2 == 0 && brutePrefab != null)
                {
                    return brutePrefab;
                }

                return shooterPrefab != null ? shooterPrefab : basicPrefab;
        }

        return basicPrefab;
    }

    private void SpawnEnemy(GameObject enemyPrefab, PressureEventType eventType, int spawnIndex, int totalToSpawn)
    {
        if (enemyPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = GetPressureSpawnPosition(eventType, spawnIndex, totalToSpawn);

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

        if (enemyObject == null)
        {
            return;
        }

        enemyObject.transform.position = spawnPosition;
        enemyObject.transform.rotation = Quaternion.identity;
        enemyObject.SetActive(true);
    }

    private Vector3 GetPressureSpawnPosition(PressureEventType eventType, int spawnIndex, int totalToSpawn)
    {
        switch (eventType)
        {
            case PressureEventType.SniperCrossfire:
                return GetAlternatingSideSpawnPosition(spawnIndex);

            case PressureEventType.BomberPanic:
                return GetSpreadAroundCameraSpawnPosition(spawnIndex, totalToSpawn);

            case PressureEventType.SupportCluster:
            case PressureEventType.ShieldWall:
                return GetClusterSpawnPosition(spawnIndex);

            default:
                return GetSpawnPositionOutsideCamera();
        }
    }

    private Vector3 GetAlternatingSideSpawnPosition(int spawnIndex)
    {
        int side = spawnIndex % 4;
        return GetSpawnPositionOnSpecificSide(side);
    }

    private Vector3 GetSpreadAroundCameraSpawnPosition(int spawnIndex, int totalToSpawn)
    {
        int safeTotal = Mathf.Max(1, totalToSpawn);
        int side = Mathf.FloorToInt(((float)spawnIndex / safeTotal) * 4f);

        side = Mathf.Clamp(side, 0, 3);

        return GetSpawnPositionOnSpecificSide(side);
    }

    private Vector3 GetClusterSpawnPosition(int spawnIndex)
    {
        Vector3 center = GetSpawnPositionOutsideCamera();

        Vector2 offset = Random.insideUnitCircle * 1.75f;

        if (spawnIndex == 0)
        {
            offset = Vector2.zero;
        }

        return center + new Vector3(offset.x, offset.y, 0f);
    }

    private Vector3 GetSpawnPositionOutsideCamera()
    {
        int side = Random.Range(0, 4);
        return GetSpawnPositionOnSpecificSide(side);
    }

    private Vector3 GetSpawnPositionOnSpecificSide(int side)
    {
        if (mainCamera == null)
        {
            return GetFallbackSpawnPosition();
        }

        if (mainCamera.orthographic)
        {
            return GetOrthographicSpawnPosition(side);
        }

        return GetPerspectiveSpawnPosition(side);
    }

    private Vector3 GetOrthographicSpawnPosition(int side)
    {
        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;

        Vector3 cameraPosition = mainCamera.transform.position;

        float leftEdge = cameraPosition.x - cameraHalfWidth;
        float rightEdge = cameraPosition.x + cameraHalfWidth;
        float bottomEdge = cameraPosition.y - cameraHalfHeight;
        float topEdge = cameraPosition.y + cameraHalfHeight;

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

        return new Vector3(spawnX, spawnY, gameplayPlaneZ);
    }

    private Vector3 GetPerspectiveSpawnPosition(int side)
    {
        float viewportX = 0.5f;
        float viewportY = 0.5f;

        switch (side)
        {
            case 0:
                viewportX = -0.1f;
                viewportY = Random.Range(0f, 1f);
                break;

            case 1:
                viewportX = 1.1f;
                viewportY = Random.Range(0f, 1f);
                break;

            case 2:
                viewportX = Random.Range(0f, 1f);
                viewportY = -0.1f;
                break;

            case 3:
                viewportX = Random.Range(0f, 1f);
                viewportY = 1.1f;
                break;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f));
        Plane gameplayPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, gameplayPlaneZ));

        if (gameplayPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            worldPoint.z = gameplayPlaneZ;
            return worldPoint;
        }

        return GetFallbackSpawnPosition();
    }

    private Vector3 GetFallbackSpawnPosition()
    {
        if (player == null)
        {
            return Vector3.zero;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection.sqrMagnitude <= 0.001f)
        {
            randomDirection = Vector2.right;
        }

        Vector3 spawnPosition = player.position + new Vector3(
            randomDirection.x,
            randomDirection.y,
            0f
        ) * fallbackSpawnDistanceFromPlayer;

        spawnPosition.z = gameplayPlaneZ;

        return spawnPosition;
    }

    private PressureEventSettings GetRandomAvailableEvent()
    {
        availableEvents.Clear();

        if (pressureEvents == null)
        {
            return null;
        }

        for (int i = 0; i < pressureEvents.Length; i++)
        {
            PressureEventSettings pressureEvent = pressureEvents[i];

            if (pressureEvent == null)
            {
                continue;
            }

            if (elapsedTime < pressureEvent.unlockTime)
            {
                continue;
            }

            if (!HasRequiredPrefabForEvent(pressureEvent.eventType))
            {
                continue;
            }

            if (pressureEvent.eventWeight <= 0f)
            {
                continue;
            }

            availableEvents.Add(pressureEvent);
        }

        if (availableEvents.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < availableEvents.Count; i++)
        {
            totalWeight += Mathf.Max(0f, availableEvents[i].eventWeight);
        }

        if (totalWeight <= 0f)
        {
            return availableEvents[0];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < availableEvents.Count; i++)
        {
            currentWeight += Mathf.Max(0f, availableEvents[i].eventWeight);

            if (randomValue <= currentWeight)
            {
                return availableEvents[i];
            }
        }

        return availableEvents[availableEvents.Count - 1];
    }

    private PressureEventSettings GetEventByType(PressureEventType eventType)
    {
        if (pressureEvents == null)
        {
            return null;
        }

        for (int i = 0; i < pressureEvents.Length; i++)
        {
            if (pressureEvents[i] != null && pressureEvents[i].eventType == eventType)
            {
                return pressureEvents[i];
            }
        }

        return null;
    }

    private bool HasRequiredPrefabForEvent(PressureEventType eventType)
    {
        switch (eventType)
        {
            case PressureEventType.RunnerRush:
                return runnerPrefab != null || basicPrefab != null;

            case PressureEventType.ShooterSquad:
                return shooterPrefab != null || basicPrefab != null;

            case PressureEventType.BomberPanic:
                return bomberPrefab != null || basicPrefab != null;

            case PressureEventType.SupportCluster:
                return healerPrefab != null || shielderPrefab != null || brutePrefab != null || basicPrefab != null;

            case PressureEventType.SniperCrossfire:
                return sniperPrefab != null || shooterPrefab != null;

            case PressureEventType.SplitterSwarm:
                return splitterPrefab != null || basicPrefab != null;

            case PressureEventType.ShieldWall:
                return shielderPrefab != null || brutePrefab != null || shooterPrefab != null || basicPrefab != null;
        }

        return false;
    }

    private void ScheduleNextEvent()
    {
        float randomOffset = Random.Range(-eventIntervalRandomOffset, eventIntervalRandomOffset);
        nextEventTime = elapsedTime + Mathf.Max(5f, eventInterval + randomOffset);
    }

    private float GetCurrentElapsedTime()
    {
        if (useRunStatsElapsedTime && GameRunStats.HasInstance)
        {
            return GameRunStats.Instance.ElapsedTime;
        }

        return elapsedTime + Time.deltaTime;
    }

    private void FindReferencesIfNeeded()
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

        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        FindPressureToastIfNeeded();
    }

    private void FindPressureToastIfNeeded()
    {
        if (pressureEventToastUI != null)
        {
            return;
        }

        pressureEventToastUI = FindObjectOfType<PressureEventToastUI>();
    }

    private void BuildDefaultPressureEvents()
    {
        pressureEvents = new PressureEventSettings[]
        {
            new PressureEventSettings
            {
                eventType = PressureEventType.RunnerRush,
                eventTitle = "RUNNER RUSH",
                eventSubtitle = "Fast enemies are flooding the arena.",
                accentColor = new Color(0.75f, 1f, 0.2f, 1f),
                unlockTime = 45f,
                eventWeight = 2.2f,
                totalEnemiesToSpawn = 14,
                enemiesPerBurst = 3,
                timeBetweenBursts = 0.32f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.ShooterSquad,
                eventTitle = "SHOOTER SQUAD",
                eventSubtitle = "Ranged enemies are taking positions.",
                accentColor = new Color(0.35f, 0.65f, 1f, 1f),
                unlockTime = 75f,
                eventWeight = 1.3f,
                totalEnemiesToSpawn = 8,
                enemiesPerBurst = 2,
                timeBetweenBursts = 0.65f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.BomberPanic,
                eventTitle = "BOMBER PANIC",
                eventSubtitle = "Explosive enemies are rushing in.",
                accentColor = new Color(1f, 0.35f, 0.08f, 1f),
                unlockTime = 105f,
                eventWeight = 1.2f,
                totalEnemiesToSpawn = 7,
                enemiesPerBurst = 1,
                timeBetweenBursts = 0.55f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.SupportCluster,
                eventTitle = "SUPPORT CLUSTER",
                eventSubtitle = "Support units are reinforcing the horde.",
                accentColor = new Color(0.35f, 1f, 0.55f, 1f),
                unlockTime = 135f,
                eventWeight = 1f,
                totalEnemiesToSpawn = 6,
                enemiesPerBurst = 2,
                timeBetweenBursts = 0.55f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.SniperCrossfire,
                eventTitle = "SNIPER CROSSFIRE",
                eventSubtitle = "Long-range enemies are lining up shots.",
                accentColor = new Color(1f, 0.16f, 0.12f, 1f),
                unlockTime = 165f,
                eventWeight = 0.9f,
                totalEnemiesToSpawn = 5,
                enemiesPerBurst = 1,
                timeBetweenBursts = 0.85f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.SplitterSwarm,
                eventTitle = "SPLITTER SWARM",
                eventSubtitle = "Enemies will multiply if ignored.",
                accentColor = new Color(0.85f, 0.35f, 1f, 1f),
                unlockTime = 185f,
                eventWeight = 1f,
                totalEnemiesToSpawn = 7,
                enemiesPerBurst = 1,
                timeBetweenBursts = 0.55f,
                pauseNormalSpawnerDuringEvent = false
            },

            new PressureEventSettings
            {
                eventType = PressureEventType.ShieldWall,
                eventTitle = "SHIELD WALL",
                eventSubtitle = "Protected enemies are advancing.",
                accentColor = new Color(0.35f, 0.85f, 1f, 1f),
                unlockTime = 210f,
                eventWeight = 0.85f,
                totalEnemiesToSpawn = 7,
                enemiesPerBurst = 2,
                timeBetweenBursts = 0.65f,
                pauseNormalSpawnerDuringEvent = false
            }
        };
    }
}
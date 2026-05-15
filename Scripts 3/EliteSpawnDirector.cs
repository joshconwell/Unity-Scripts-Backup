using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteSpawnDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;

    [Header("Elite Prefab")]
    [SerializeField] private GameObject elitePrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float firstEliteSpawnDelay = 60f;
    [SerializeField] private float eliteSpawnInterval = 75f;
    [SerializeField] private int maxAliveElites = 1;

    [Header("Warning")]
    [SerializeField] private bool showWarningBeforeSpawn = true;
    [SerializeField] private GameObject warningObject;
    [SerializeField] private float warningDuration = 2f;

    [Header("Spawn Position")]
    [Tooltip("How far outside the camera viewport the elite can spawn. 0.1 means 10% outside the screen.")]
    [SerializeField] private float viewportSpawnPadding = 0.12f;

    [Tooltip("Used if the camera-to-world spawn calculation fails.")]
    [SerializeField] private float fallbackSpawnDistanceFromPlayer = 18f;

    [Tooltip("The Z plane where your 2D gameplay happens. Usually 0.")]
    [SerializeField] private float gameplayPlaneZ = 0f;

    [Header("Debug")]
    [SerializeField] private bool allowDebugSpawnKey = true;
    [SerializeField] private KeyCode debugSpawnKey = KeyCode.F6;

    private readonly List<GameObject> aliveElites = new List<GameObject>();

    private float elapsedTime;
    private float nextEliteSpawnTime;
    private bool spawnRoutineRunning;

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
        nextEliteSpawnTime = firstEliteSpawnDelay;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        FindReferencesIfNeeded();
        CleanupEliteList();

        if (allowDebugSpawnKey && Input.GetKeyDown(debugSpawnKey))
        {
            TryStartEliteSpawnRoutine(true);
        }

        if (elitePrefab == null)
        {
            return;
        }

        if (aliveElites.Count >= maxAliveElites)
        {
            return;
        }

        if (spawnRoutineRunning)
        {
            return;
        }

        if (elapsedTime >= nextEliteSpawnTime)
        {
            TryStartEliteSpawnRoutine(false);
        }
    }

    private void TryStartEliteSpawnRoutine(bool debugSpawn)
    {
        if (elitePrefab == null)
        {
            Debug.LogWarning("EliteSpawnDirector is missing an elite prefab.");
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("EliteSpawnDirector could not find the player.");
            return;
        }

        if (aliveElites.Count >= maxAliveElites && !debugSpawn)
        {
            return;
        }

        StartCoroutine(EliteSpawnRoutine(debugSpawn));
    }

    private IEnumerator EliteSpawnRoutine(bool debugSpawn)
    {
        spawnRoutineRunning = true;

        if (showWarningBeforeSpawn && warningObject != null && warningDuration > 0f)
        {
            warningObject.SetActive(true);
            yield return new WaitForSeconds(warningDuration);
            warningObject.SetActive(false);
        }

        SpawnElite();

        if (!debugSpawn)
        {
            nextEliteSpawnTime = elapsedTime + eliteSpawnInterval;
        }

        spawnRoutineRunning = false;
    }

    private void SpawnElite()
    {
        Vector3 spawnPosition = GetSpawnPositionOutsideCamera();

        GameObject eliteObject = null;

        if (PoolManager.HasInstance)
        {
            eliteObject = PoolManager.Instance.Spawn(
                elitePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (eliteObject == null)
        {
            eliteObject = Instantiate(
                elitePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        if (eliteObject != null)
        {
            aliveElites.Add(eliteObject);
            Debug.Log($"Elite spawned: {eliteObject.name}");
        }
    }

    private Vector3 GetSpawnPositionOutsideCamera()
    {
        if (mainCamera == null || player == null)
        {
            return GetFallbackSpawnPosition();
        }

        int side = Random.Range(0, 4);

        float viewportX = 0.5f;
        float viewportY = 0.5f;

        switch (side)
        {
            case 0:
                viewportX = -viewportSpawnPadding;
                viewportY = Random.Range(0f, 1f);
                break;

            case 1:
                viewportX = 1f + viewportSpawnPadding;
                viewportY = Random.Range(0f, 1f);
                break;

            case 2:
                viewportX = Random.Range(0f, 1f);
                viewportY = -viewportSpawnPadding;
                break;

            case 3:
                viewportX = Random.Range(0f, 1f);
                viewportY = 1f + viewportSpawnPadding;
                break;
        }

        if (mainCamera.orthographic)
        {
            float distanceFromCameraToPlane = Mathf.Abs(mainCamera.transform.position.z - gameplayPlaneZ);

            Vector3 viewportPoint = new Vector3(
                viewportX,
                viewportY,
                distanceFromCameraToPlane
            );

            Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoint);
            worldPoint.z = gameplayPlaneZ;

            return worldPoint;
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
    }

    private void CleanupEliteList()
    {
        for (int i = aliveElites.Count - 1; i >= 0; i--)
        {
            if (aliveElites[i] == null)
            {
                aliveElites.RemoveAt(i);
                continue;
            }

            if (!aliveElites[i].activeInHierarchy)
            {
                aliveElites.RemoveAt(i);
            }
        }
    }
}
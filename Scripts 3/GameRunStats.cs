using System;
using UnityEngine;

public class GameRunStats : MonoBehaviour
{
    public static GameRunStats Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("Run State")]
    [SerializeField] private bool runStartsAutomatically = true;

    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;

    private float elapsedTime;
    private int killCount;
    private bool runActive;

    public float ElapsedTime => elapsedTime;
    public int KillCount => killCount;
    public bool RunActive => runActive;

    public event Action<float> OnTimerChanged;
    public event Action<int> OnKillCountChanged;
    public event Action<float, int> OnRunEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        AutoFindPlayerHealth();
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDied += HandlePlayerDeath;
        }

        if (runStartsAutomatically)
        {
            StartRun();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDied -= HandlePlayerDeath;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!runActive)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        OnTimerChanged?.Invoke(elapsedTime);
    }

    public void StartRun()
    {
        elapsedTime = 0f;
        killCount = 0;
        runActive = true;

        OnTimerChanged?.Invoke(elapsedTime);
        OnKillCountChanged?.Invoke(killCount);
    }

    public void RegisterEnemyKill(int killValue = 1)
    {
        if (!runActive)
        {
            return;
        }

        killCount += Mathf.Max(1, killValue);
        OnKillCountChanged?.Invoke(killCount);
    }

    public void EndRun()
    {
        if (!runActive)
        {
            return;
        }

        runActive = false;
        OnRunEnded?.Invoke(elapsedTime, killCount);
    }

    private void HandlePlayerDeath()
    {
        EndRun();
    }

    private void AutoFindPlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<Health>();
        }
    }
}
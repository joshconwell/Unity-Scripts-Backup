using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("Hit Stop Settings")]
    [SerializeField] private float minimumTimeBetweenHitStops = 0.04f;

    private Coroutine hitStopCoroutine;
    private float nextAllowedHitStopTime;
    private float defaultFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.fixedDeltaTime = defaultFixedDeltaTime;

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    public void DoHitStop(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (Time.unscaledTime < nextAllowedHitStopTime)
        {
            return;
        }

        if (Time.timeScale <= 0f && hitStopCoroutine == null)
        {
            return;
        }

        if (hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
            hitStopCoroutine = null;
        }

        hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
        nextAllowedHitStopTime = Time.unscaledTime + minimumTimeBetweenHitStops;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float previousTimeScale = Time.timeScale;

        if (previousTimeScale <= 0f)
        {
            hitStopCoroutine = null;
            yield break;
        }

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * previousTimeScale;

        hitStopCoroutine = null;
    }
}
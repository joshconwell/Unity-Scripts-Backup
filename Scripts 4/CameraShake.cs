using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.12f;
    [SerializeField] private float defaultMagnitude = 0.08f;

    private Vector3 originalLocalPosition;

    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        originalLocalPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LateUpdate()
    {
        if (ScreenShakeSettings.HasInstance && !ScreenShakeSettings.Instance.ScreenShakeEnabled)
        {
            StopShakeImmediately();
            return;
        }

        if (shakeTimer > 0f)
        {
            float progress = shakeTimer / shakeDuration;
            float currentMagnitude = shakeMagnitude * progress;

            Vector2 randomOffset = Random.insideUnitCircle * currentMagnitude;

            transform.localPosition = originalLocalPosition + new Vector3(
                randomOffset.x,
                randomOffset.y,
                0f
            );

            shakeTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            StopShakeImmediately();
        }
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f)
        {
            return;
        }

        if (ScreenShakeSettings.HasInstance)
        {
            if (!ScreenShakeSettings.Instance.ScreenShakeEnabled)
            {
                return;
            }

            magnitude *= ScreenShakeSettings.Instance.IntensityMultiplier;
        }

        if (magnitude <= 0f)
        {
            return;
        }

        shakeDuration = duration;
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    private void StopShakeImmediately()
    {
        shakeTimer = 0f;
        shakeDuration = 0f;
        shakeMagnitude = 0f;
        transform.localPosition = originalLocalPosition;
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EliteEnemyUIBridge : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private string targetName = "ELITE BRUTE";
    [SerializeField] private EnemyTargetBarStyle targetBarStyle = EnemyTargetBarStyle.Elite;

    [Tooltip("If checked, this enemy automatically appears on the top target health bar when enabled.")]
    [SerializeField] private bool registerOnEnable = true;

    private Health health;
    private Coroutine registerRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (!registerOnEnable)
        {
            return;
        }

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        registerRoutine = StartCoroutine(RegisterAfterFrame());
    }

    private void OnDisable()
    {
        if (registerRoutine != null)
        {
            StopCoroutine(registerRoutine);
            registerRoutine = null;
        }

        if (EliteHealthBarUI.HasInstance)
        {
            EliteHealthBarUI.Instance.StopTrackingTarget(gameObject);
        }
    }

    private IEnumerator RegisterAfterFrame()
    {
        yield return null;

        if (health == null)
        {
            yield break;
        }

        if (!EliteHealthBarUI.HasInstance)
        {
            Debug.LogWarning("EliteEnemyUIBridge could not find EliteHealthBarUI in the scene.");
            yield break;
        }

        EliteHealthBarUI.Instance.TrackTarget(
            gameObject,
            health,
            targetName,
            targetBarStyle
        );
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EliteEnemyUIBridge : MonoBehaviour
{
    [Header("Elite UI")]
    [SerializeField] private string eliteName = "ELITE BRUTE";

    [Tooltip("If checked, this elite automatically appears on the top elite health bar when enabled.")]
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
            EliteHealthBarUI.Instance.StopTrackingElite(gameObject);
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

        EliteHealthBarUI.Instance.TrackElite(gameObject, health, eliteName);
    }
}
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private bool pauseGameOnDeath = true;

    private Health health;
    private PlayerController2D playerController;
    private PlayerShooter playerShooter;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerController = GetComponent<PlayerController2D>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        Debug.Log("GAME OVER - Player died.");

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerShooter != null)
        {
            playerShooter.enabled = false;
        }

        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
    }
}
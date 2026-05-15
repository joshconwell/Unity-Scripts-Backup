using UnityEngine;

public class RunSFX : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SFXType gameOverSFX = SFXType.GameOver;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMultiplier = 1f;

    private GameRunStats gameRunStats;

    private void Awake()
    {
        gameRunStats = GetComponent<GameRunStats>();
    }

    private void OnEnable()
    {
        if (gameRunStats == null)
            gameRunStats = GetComponent<GameRunStats>();

        if (gameRunStats == null)
            return;

        gameRunStats.OnRunEnded += HandleRunEnded;
    }

    private void OnDisable()
    {
        if (gameRunStats == null)
            return;

        gameRunStats.OnRunEnded -= HandleRunEnded;
    }

    private void HandleRunEnded(float elapsedTime, int killCount)
    {
        if (!AudioManager.HasInstance)
            return;

        AudioManager.Instance.PlaySFX(gameOverSFX, transform.position, volumeMultiplier);
    }
}
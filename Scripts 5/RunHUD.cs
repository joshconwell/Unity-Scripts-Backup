using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameRunStats gameRunStats;

    [Header("HUD Text")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text killCountText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalStatsText;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if (gameRunStats == null && GameRunStats.HasInstance)
        {
            gameRunStats = GameRunStats.Instance;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartCurrentScene);
        }
    }

    private void Start()
    {
        if (gameRunStats == null && GameRunStats.HasInstance)
        {
            gameRunStats = GameRunStats.Instance;
        }

        SubscribeToRunStats();
        RefreshHUD();
    }

    private void OnDestroy()
    {
        UnsubscribeFromRunStats();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartCurrentScene);
        }
    }

    private void SubscribeToRunStats()
    {
        if (gameRunStats == null)
        {
            Debug.LogWarning("RunHUD could not find GameRunStats.");
            return;
        }

        gameRunStats.OnTimerChanged += UpdateTimerText;
        gameRunStats.OnKillCountChanged += UpdateKillCountText;
        gameRunStats.OnRunEnded += ShowGameOverPanel;
    }

    private void UnsubscribeFromRunStats()
    {
        if (gameRunStats == null)
        {
            return;
        }

        gameRunStats.OnTimerChanged -= UpdateTimerText;
        gameRunStats.OnKillCountChanged -= UpdateKillCountText;
        gameRunStats.OnRunEnded -= ShowGameOverPanel;
    }

    private void RefreshHUD()
    {
        if (gameRunStats == null)
        {
            UpdateTimerText(0f);
            UpdateKillCountText(0);
            return;
        }

        UpdateTimerText(gameRunStats.ElapsedTime);
        UpdateKillCountText(gameRunStats.KillCount);
    }

    private void UpdateTimerText(float elapsedTime)
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = $"Time: {FormatTime(elapsedTime)}";
    }

    private void UpdateKillCountText(int killCount)
    {
        if (killCountText == null)
        {
            return;
        }

        killCountText.text = $"Kills: {killCount}";
    }

    private void ShowGameOverPanel(float finalTime, int finalKills)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalStatsText != null)
        {
            finalStatsText.text =
                $"Final Time: {FormatTime(finalTime)}\n" +
                $"Kills: {finalKills}";
        }
    }

    private void RestartCurrentScene()
    {
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private string FormatTime(float timeInSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}
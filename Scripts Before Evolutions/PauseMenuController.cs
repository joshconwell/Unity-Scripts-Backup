using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private enum PendingConfirmAction
    {
        None,
        RestartRun,
        MainMenu,
        QuitGame
    }

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool allowPauseKey = true;

    [Header("Scene Names")]
    [Tooltip("Name of your main menu scene. Do not include .unity")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Panels")]
    [Tooltip("The full pause menu root object. This should contain all pause menu panels.")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Tooltip("The main pause menu panel with Resume / Options / Restart / Main Menu / Quit.")]
    [SerializeField] private GameObject mainPausePanel;

    [Tooltip("Your options panel. Put your existing screen shake settings here.")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("Optional controls/how-to-play panel.")]
    [SerializeField] private GameObject controlsPanel;

    [Tooltip("Optional confirmation panel for restart/main menu/quit.")]
    [SerializeField] private GameObject confirmPanel;

    [Header("Confirmation Text - Legacy Optional")]
    [SerializeField] private Text confirmMessageText;

    [Header("Confirmation Text - TMP Optional")]
    [SerializeField] private TMP_Text confirmMessageTMPText;

    [Header("Pause Behavior")]
    [SerializeField] private bool pauseTimeScale = true;
    [SerializeField] private bool showCursorWhilePaused = true;
    [SerializeField] private bool hideCursorWhenResumed = false;

    [Header("Confirmation Settings")]
    [SerializeField] private bool confirmRestartRun = true;
    [SerializeField] private bool confirmMainMenu = true;
    [SerializeField] private bool confirmQuitGame = true;

    [Header("Do Not Open Pause Menu While These Are Active")]
    [Tooltip("Optional. Add Upgrade Panel, Special Reward Panel, Game Over Panel, etc. here so ESC does not open pause over them.")]
    [SerializeField] private GameObject[] blockingPanels;

    private PendingConfirmAction pendingConfirmAction = PendingConfirmAction.None;

    private bool isPaused;
    private float previousTimeScale = 1f;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        ForceClosePauseMenu();

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!allowPauseKey)
        {
            return;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            if (IsSubPanelOpen())
            {
                ShowMainPausePanel();
                return;
            }

            ResumeGame();
            return;
        }

        if (IsAnyBlockingPanelActive())
        {
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;

        if (pauseTimeScale)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (showCursorWhilePaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);
        }

        ShowMainPausePanel();
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        pendingConfirmAction = PendingConfirmAction.None;

        if (pauseTimeScale)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        if (hideCursorWhenResumed)
        {
            Cursor.visible = false;
        }

        ForceClosePauseMenu();
    }

    public void ShowMainPausePanel()
    {
        SetPanelActive(mainPausePanel, true);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(confirmPanel, false);

        pendingConfirmAction = PendingConfirmAction.None;
    }

    public void ShowOptionsPanel()
    {
        SetPanelActive(mainPausePanel, false);
        SetPanelActive(optionsPanel, true);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(confirmPanel, false);

        pendingConfirmAction = PendingConfirmAction.None;
    }

    public void ShowControlsPanel()
    {
        SetPanelActive(mainPausePanel, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(controlsPanel, true);
        SetPanelActive(confirmPanel, false);

        pendingConfirmAction = PendingConfirmAction.None;
    }

    public void BackToMainPausePanel()
    {
        ShowMainPausePanel();
    }

    public void RestartRun()
    {
        if (confirmRestartRun)
        {
            ShowConfirmation(
                PendingConfirmAction.RestartRun,
                "Restart the current run?\nYour current progress will be lost."
            );

            return;
        }

        ExecuteRestartRun();
    }

    public void ReturnToMainMenu()
    {
        if (confirmMainMenu)
        {
            ShowConfirmation(
                PendingConfirmAction.MainMenu,
                "Return to the main menu?\nYour current run progress will be lost."
            );

            return;
        }

        ExecuteReturnToMainMenu();
    }

    public void QuitGame()
    {
        if (confirmQuitGame)
        {
            ShowConfirmation(
                PendingConfirmAction.QuitGame,
                "Quit the game?"
            );

            return;
        }

        ExecuteQuitGame();
    }

    public void ConfirmPendingAction()
    {
        switch (pendingConfirmAction)
        {
            case PendingConfirmAction.RestartRun:
                ExecuteRestartRun();
                break;

            case PendingConfirmAction.MainMenu:
                ExecuteReturnToMainMenu();
                break;

            case PendingConfirmAction.QuitGame:
                ExecuteQuitGame();
                break;

            case PendingConfirmAction.None:
                ShowMainPausePanel();
                break;
        }
    }

    public void CancelConfirmation()
    {
        pendingConfirmAction = PendingConfirmAction.None;
        ShowMainPausePanel();
    }

    private void ShowConfirmation(PendingConfirmAction action, string message)
    {
        pendingConfirmAction = action;

        SetConfirmMessage(message);

        SetPanelActive(mainPausePanel, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(confirmPanel, true);
    }

    private void ExecuteRestartRun()
    {
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void ExecuteReturnToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("PauseMenuController: Main Menu Scene Name is empty.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ExecuteQuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Quit Game pressed.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ForceClosePauseMenu()
    {
        SetPanelActive(pauseMenuRoot, false);
        SetPanelActive(mainPausePanel, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(confirmPanel, false);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void SetConfirmMessage(string message)
    {
        if (confirmMessageText != null)
        {
            confirmMessageText.text = message;
        }

        if (confirmMessageTMPText != null)
        {
            confirmMessageTMPText.text = message;
        }
    }

    private bool IsSubPanelOpen()
    {
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            return true;
        }

        if (controlsPanel != null && controlsPanel.activeSelf)
        {
            return true;
        }

        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            return true;
        }

        return false;
    }

    private bool IsAnyBlockingPanelActive()
    {
        if (blockingPanels == null)
        {
            return false;
        }

        for (int i = 0; i < blockingPanels.Length; i++)
        {
            GameObject panel = blockingPanels[i];

            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }
}
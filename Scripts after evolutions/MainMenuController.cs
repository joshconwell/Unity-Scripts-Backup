using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("Name of your actual gameplay scene. Do not include .unity")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Tooltip("Only use this if you would rather load by Build Settings index instead of scene name.")]
    [SerializeField] private bool loadByBuildIndex = false;

    [Tooltip("Usually 1 if MainMenu is scene 0 and your game scene is scene 1.")]
    [SerializeField] private int gameSceneBuildIndex = 1;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Startup")]
    [SerializeField] private bool resetTimeScaleOnMenu = true;
    [SerializeField] private bool showCursorOnMenu = true;

    private void Awake()
    {
        if (resetTimeScaleOnMenu)
        {
            Time.timeScale = 1f;
        }

        if (showCursorOnMenu)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        ShowMainPanel();
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;

        if (loadByBuildIndex)
        {
            SceneManager.LoadScene(gameSceneBuildIndex);
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("MainMenuController: Game Scene Name is empty.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMainPanel()
    {
        SetPanelActive(mainPanel, true);
        SetPanelActive(howToPlayPanel, false);
        SetPanelActive(optionsPanel, false);
    }

    public void ShowHowToPlayPanel()
    {
        SetPanelActive(mainPanel, false);
        SetPanelActive(howToPlayPanel, true);
        SetPanelActive(optionsPanel, false);
    }

    public void ShowOptionsPanel()
    {
        SetPanelActive(mainPanel, false);
        SetPanelActive(howToPlayPanel, false);
        SetPanelActive(optionsPanel, true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
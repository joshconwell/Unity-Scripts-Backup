using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuSceneActions : MonoBehaviour
{
    [Header("Main Menu Scene")]
    [Tooltip("Name of your main menu scene. Do not include .unity")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Tooltip("Use this if you want to load by Build Settings index instead of scene name.")]
    [SerializeField] private bool loadByBuildIndex = false;

    [Tooltip("Usually 0 if MainMenu is first in your Build Settings scene list.")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    [Header("Before Loading Menu")]
    [SerializeField] private bool resetTimeScale = true;
    [SerializeField] private bool unlockAndShowCursor = true;

    public void ReturnToMainMenu()
    {
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
        }

        if (unlockAndShowCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (loadByBuildIndex)
        {
            SceneManager.LoadScene(mainMenuBuildIndex);
            return;
        }

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("InGameMenuSceneActions: Main Menu Scene Name is empty.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
        }

        Debug.Log("Quit Game pressed.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
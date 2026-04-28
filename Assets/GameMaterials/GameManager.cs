using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Sahne İsimleri")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "arenaoyunu";
    public string winSceneName = "WinScreen";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OpenWinScene()
    {
        SceneManager.LoadScene(winSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

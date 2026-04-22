using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    public void ExitToMenu()
    {
        // Make sure the game is no longer paused
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Clear SaveManager runtime state too
        if (SaveManager.Instance != null)
            SaveManager.Instance.PrepareForMainMenu();

        SceneManager.LoadScene("Main Menu Scene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
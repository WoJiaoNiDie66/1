using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitToMenu()
    {
        // Reset pause state so the next session starts clean.
        ActiveGameUIManager.isPaused = false;
        Time.timeScale = 1f;

        // Reset SaveManager's runtime state.
        if (SaveManager.Instance != null)
            SaveManager.Instance.PrepareForMainMenu();

        SceneManager.LoadScene("Main Menu Scene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitToMenu()
    {
        // Reset SaveManager's runtime state so the next slot load starts clean.
        if (SaveManager.Instance != null)
            SaveManager.Instance.PrepareForMainMenu();

        SceneManager.LoadScene("Main Menu Scene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
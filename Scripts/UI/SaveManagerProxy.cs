using UnityEngine;

public class SaveManagerProxy : MonoBehaviour
{
    // The button will call this method, which safely finds the surviving SaveManager
    public void CallLoadGame()
    {
        if (SaveManager.Instance != null)
        {
            Debug.Log("<color=cyan>[SaveManagerProxy]</color> UI Button clicked. Sending signal to SaveManager.Instance!");
            
            // We pass true so the player teleports to the checkpoint after loading
            SaveManager.Instance.LoadGame(true);
        }
        else
        {
            Debug.LogError("<color=red>[SaveManagerProxy]</color> SaveManager.Instance is null! Is it in the scene?");
        }
    }
}
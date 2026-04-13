// Assets/Scripts/Core/GameBootstrapper.cs
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    private void Start()
    {
        // We use Start() to ensure all Managers have finished their Awake() setup [web:18].
        if (SaveManager.Instance != null)
        {
            // The 'true' parameter teleports the player to the loaded checkpoint
            SaveManager.Instance.LoadGame(true);
        }
    }
}
// Assets/Scripts/Core/DemoGameBootstrapper.cs
using UnityEngine;

public class DemoGameBootstrapper : MonoBehaviour
{
    private void Start()
    {
        // We use Start() to ensure all Managers have finished their Awake() setup [web:18].
        if (DemoSaveManager.Instance != null)
        {
            // The 'true' parameter teleports the player to the loaded checkpoint
            DemoSaveManager.Instance.LoadGame(true);
        }
    }
}
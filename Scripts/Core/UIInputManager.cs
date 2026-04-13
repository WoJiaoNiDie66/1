// Assets/Scripts/Core/InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputManager : MonoBehaviour
{

    public static UIInputManager Instance;
    private PlayerInput _playerInput;





    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("UIInputManager requires a PlayerInput component.");
            enabled = false;
            return;
        }

    }



}

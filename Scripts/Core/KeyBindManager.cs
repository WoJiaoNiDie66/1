using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyBindManager : MonoBehaviour
{
    public static KeyBindManager Instance;

    public static bool IsActive = false;

    public static UnityAction<InputActionReference> OnKeybindSelected;

    public static UnityAction<string> OnKeybindChanged;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private GameObject chooseKeyPanel;
    [SerializeField]
    private TextMeshProUGUI debugText;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnKeybindSelected = StartBinding;
        ClosePanel();

        if(playerInput == null)
        {
            Debug.LogError("Player Input reference is missing in KeyBindManager");
        }

    }

    public void OpenPanel()
    {
        chooseKeyPanel.SetActive(true);
        debugText.text = "Press Any Key.";
    }

    public void ClosePanel()
    {
        chooseKeyPanel.SetActive(false);
    }

    public void StartBinding()
    {
        IsActive = true;
        playerInput.SwitchCurrentActionMap("Rebind");
        OpenPanel();
    }

    public void EndBinding()
    {
        IsActive = false;
        playerInput.SwitchCurrentActionMap("Player");
        ClosePanel();
    }

    public void StartBinding(InputActionReference actionReference)
    {
        OpenPanel();
        actionReference.action.Disable();
        playerInput.SwitchCurrentActionMap("Rebind");
        Debug.Log("Binding");
        rebindingOperation = actionReference.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindComplete(actionReference))
            .OnCancel(operation => RebindCancel(actionReference))
            .Start();
    }


    private void RebindComplete(InputActionReference actionReference)
    {
        actionReference.action.Enable();
        int bindingIndex = actionReference.action.GetBindingIndexForControl(actionReference.action.controls[0]);

        string newBindingPath = actionReference.action.bindings[bindingIndex].effectivePath;

        Debug.Log(newBindingPath);

        string text = InputControlPath.ToHumanReadableString(
            newBindingPath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
        OnKeybindChanged?.Invoke(text);
        rebindingOperation.Dispose();
        ClosePanel();
        playerInput.SwitchCurrentActionMap("Player");
        IsActive = false;
        OnKeybindChanged = null;
    }

    private void RebindCancel(InputActionReference actionReference)
    {
        ClosePanel();
        actionReference.action.Enable();
        rebindingOperation.Dispose();
        IsActive = false;
    }
}

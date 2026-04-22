using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
//This class no longer used. Please look at RebindActionUI.cs for the new implementation of keybind UI.
public class KeybindUI : MonoBehaviour,IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI keybindText;
    [SerializeField]
    private InputActionReference inputActionReference;
    [SerializeField]
    private UnityEvent onKeyBindSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        onKeyBindSelected.Invoke();
    }

    private void ChangeBindText(string text)
    {
        keybindText.text = text;
        int bindingIndex = inputActionReference.action.GetBindingIndexForControl(inputActionReference.action.controls[0]);

        string testText = InputControlPath.ToHumanReadableString(
            inputActionReference.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        Debug.Log( testText );
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance;


    [SerializeField]
    private TextMeshProUGUI openInteractUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (openInteractUI != null)
            openInteractUI.gameObject.SetActive(false);
        else Debug.Log("OpenInteractUI is not assigned in the inspector.");
    }

    public void OpenMessage(string text)
    {
        openInteractUI.text = text;
        openInteractUI.gameObject.SetActive(true);
    }

    public void CloseMessage()
    {
        openInteractUI.gameObject.SetActive(false);
    }
}

using cherrydev;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField]
    private DialogNodeGraph _graph;

    [SerializeField]
    private string characterName;

    public static bool IsInteracting = false;
    private bool _currentlyInteracting = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_currentlyInteracting || IsInteracting) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Interacting with NPC");
            DialogManager.Instance.StartDialog(_graph);
            IsInteracting = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsInteracting) return;

        if (other.CompareTag("Player"))
        {
            _currentlyInteracting = true;
            InteractableManager.Instance.OpenMessage($"Chat to {characterName} (F)");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(!_currentlyInteracting || IsInteracting);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_currentlyInteracting) return;

        if (other.CompareTag("Player"))
        {
            IsInteracting = false;
            _currentlyInteracting = false;
            InteractableManager.Instance.CloseMessage();
        }
    }

    public static void ResetNPCInteraction()
    {
        IsInteracting = false;
        Debug.Log("Reset NPC Interaction");
    }
}

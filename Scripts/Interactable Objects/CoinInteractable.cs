using cherrydev;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinInteractable : MonoBehaviour
{
    [SerializeField]
    private int coinAmount = 1;

    public static bool IsInteracting = false;
    private bool _currentlyInteracting = false;


    // Update is called once per frame
    void Update()
    {
        if (!_currentlyInteracting || IsInteracting) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Collected the coins");
            IsInteracting = true;
            ActiveGameUIManager.OnPlayerCoinColleceted.Invoke(coinAmount);
            InteractableManager.Instance.CloseMessage();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsInteracting) return;

        if (other.CompareTag("Player"))
        {
            _currentlyInteracting = true;
            InteractableManager.Instance.OpenMessage($"Collect {coinAmount} coin(s) (F)");
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

    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    public static void ResetCoinInteraction()
    {
        IsInteracting = false;
    }

}

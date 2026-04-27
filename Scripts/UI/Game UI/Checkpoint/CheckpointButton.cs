using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointButton : MonoBehaviour
{
    [SerializeField]
    private Checkpoint checkpoint;

    private Button button;
    
    private void Start()
    {
        button = GetComponent<Button>();
        //button.onClick.AddListener();
    }
}

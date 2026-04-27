using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewCheckpointPanel : MonoBehaviour
{
    [SerializeField]
    private List<Button> areaButtons;

    [SerializeField]
    private List<GameObject> checkpointHolders;

    private GameObject currentHolder;

    private void Start()
    {
        for (int i = 0; i < areaButtons.Count; i++)
        {
            int index = i;
            areaButtons[i].onClick.AddListener(() => LoadCheckpointHolder(index));
        }

        LoadCheckpointHolder(0);
    }

    private void LoadCheckpointHolder(int i)
    {
        if (currentHolder != null)
        {
            currentHolder.SetActive(false);
        }
        currentHolder = checkpointHolders[i];
        currentHolder.SetActive(true);
    }
   
    public void LoadPanel(Checkpoint from)
    {

    }

}

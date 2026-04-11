using UnityEngine;

public class Ptr2Door : MonoBehaviour
{
    public DoorController doorController; // 拖入 DoorController 的物件

    void Start()
    {
        if (doorController == null)
        {
            Debug.LogError("錯誤：請在 Inspector 中拖入 DoorController 物件！");
        }
    }

    public void TriggerDoor()
    {
        if (doorController != null)
        {
            doorController.TriggerFromLever();
        }
    }
}
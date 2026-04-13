using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2.0f; // 交互距离
    public LayerMask interactableLayer; // 设置为你的墙壁所在的Layer

    void Update()
    {
        // 假设按下“E”键进行交互
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 使用主相机的位置和前方作为射线起始点
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}
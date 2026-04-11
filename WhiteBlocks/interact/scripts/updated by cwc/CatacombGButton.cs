using UnityEngine;

public class CatacombGButton : MonoBehaviour
{
    // 狀態開關
    public bool isPressed = false;

    // --- 玩家踩上壓力板 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPressed = true;   // 按鈕凹下
        }
    }

    // --- 玩家離開壓力板 ---
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPressed = false;  // 按鈕彈回
        }
    }
}
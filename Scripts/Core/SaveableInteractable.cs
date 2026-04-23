using UnityEngine;
using UnityEngine.Events; // 引入 UnityEvent 命名空间

public class SaveableInteractable : MonoBehaviour
{
    [Header("Save Settings")]
    public bool requireSaveState = true;
    public string uniqueID;
    public bool hasBeenInteracted = false;

    [Header("交互表现 (无需写代码，在面板拖拽即可)")]
    [Tooltip("当玩家第一次按下交互键时触发（例如：播放开门动画、播放音效）")]
    public UnityEvent onInteract;

    [Tooltip("当读取存档发现已交互过时触发（例如：直接把门设为开启状态，不播动画）")]
    public UnityEvent onLoadAlreadyInteracted;

    [ContextMenu("Generate Unique ID")]
    private void GenerateUniqueID()
    {
        if (string.IsNullOrEmpty(uniqueID)) uniqueID = System.Guid.NewGuid().ToString();
    }

    // ==========================================
    // 【核心魔法：让玩家只调用这一个方法】
    // ==========================================
    public void TriggerInteraction()
    {
        if (hasBeenInteracted) return; // 如果机关只能触发一次，直接拦截

        // 1. 状态自动改变！（再也不用你手动写了）
        hasBeenInteracted = true;

        // 2. 自动触发你在 Inspector 里绑定的所有动画和音效
        onInteract?.Invoke();

        Debug.Log($"<color=green>[Interact]</color> {gameObject.name} 已触发，状态已自动锁定！");
    }

    // ==========================================
    // 【供 SaveManager 读档时调用】
    // ==========================================
    public void RestoreState()
    {
        hasBeenInteracted = true;
        onLoadAlreadyInteracted?.Invoke(); // 直接切换到已打开的状态
    }
}
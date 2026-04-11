using UnityEngine;
using UnityEngine.Events;

public class SB2EventNode : MonoBehaviour
{
    public UnityEvent OnHit;
    
    // 在你的 Hurtbox 中调用这个方法
    public void TriggerEvent()
    {
        // 触发 UnityEvent，调用所有绑定的方法
        OnHit?.Invoke();
    }
}
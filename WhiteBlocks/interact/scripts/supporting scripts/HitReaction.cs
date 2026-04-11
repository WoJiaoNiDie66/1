using UnityEngine;
using UnityEngine.Events;

public class HitReaction : MonoBehaviour
{
    [Header("Combat System")]
    [SerializeField] private Hurtbox[] hurtboxes;

    // 直接在 Inspector 中拖拽任何脚本的方法
    public UnityEvent OnHit;

    private void Start()
    {
        // 为每个 Hurtbox 添加事件监听
        foreach (var hurtbox in hurtboxes)
        {
            hurtbox.OnDamageReceived += HandleHit;
        }
    }
    
    // 在你的 Hurtbox 中调用这个方法
    private void HandleHit(DamageData damageData)
    {
        // 触发 UnityEvent，调用所有绑定的方法
        OnHit?.Invoke();
    }
}
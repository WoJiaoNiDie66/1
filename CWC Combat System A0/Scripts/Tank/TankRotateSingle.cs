using UnityEngine;

public class TankRotateSingle : StateMachineBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float targetAngle = 0f;      // 目标Y轴角度
    [SerializeField] private float duration = 0.5f;       // 旋转时间
    
    [Header("子物体")]
    [SerializeField] private string childPath = "";       // 子物体路径，如 "Arm/Hand/Weapon"
    
    private Transform target;
    private float startAngle;
    private float timer = 0f;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 获取子物体
        if (string.IsNullOrEmpty(childPath))
            target = animator.transform;
        else
            target = animator.transform.Find(childPath);
        
        if (target == null)
        {
            Debug.LogWarning($"找不到子物体: {childPath}");
            return;
        }
        
        startAngle = target.localEulerAngles.y;
        timer = 0f;
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (target == null) return;
        
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        
        // 使用简单的缓动
        float easedT = Mathf.SmoothStep(0, 1, t);
        float currentAngle = Mathf.Lerp(startAngle, targetAngle, easedT);
        
        Vector3 angles = target.localEulerAngles;
        angles.y = currentAngle;
        target.localEulerAngles = angles;
    }
}
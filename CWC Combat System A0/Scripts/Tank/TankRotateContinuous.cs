using UnityEngine;

public class TankRotateContinuous : StateMachineBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 90f;        // 旋转速度（度/秒）
    [SerializeField] private bool clockwise = true;           // 顺时针/逆时针
    
    [Header("子物体设置")]
    [SerializeField] private string childName = "";           // 子物体名称
    [SerializeField] private int childIndex = -1;             // 子物体索引
    
    private Transform targetTransform;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        targetTransform = GetTargetTransform(animator);
        
        if (targetTransform == null)
        {
            Debug.LogWarning($"找不到目标子物体: {childName}");
        }
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (targetTransform == null) return;
        
        float speed = clockwise ? rotationSpeed : -rotationSpeed;
        float delta = speed * Time.deltaTime;
        
        Vector3 angles = targetTransform.eulerAngles;
        angles.y += delta;
        targetTransform.eulerAngles = angles;
    }
    
    private Transform GetTargetTransform(Animator animator)
    {
        if (!string.IsNullOrEmpty(childName))
        {
            Transform child = animator.transform.Find(childName);
            if (child != null) return child;
        }
        
        if (childIndex >= 0 && childIndex < animator.transform.childCount)
        {
            return animator.transform.GetChild(childIndex);
        }
        
        return animator.transform;
    }
}
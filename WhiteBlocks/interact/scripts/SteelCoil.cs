using UnityEngine;

public class SteelCoil : MonoBehaviour
{
    public float maxSpeed = 10f;     // 最大滚动速度
    public float acceleration = 30f; // 加速度
    public float radius = 0.5f;      // 钢卷半径（用于计算旋转）

    private Rigidbody rb;
    private Transform meshChild;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 自动获取模型子物体用来旋转
        meshChild = transform.GetChild(0); 
    }

    void FixedUpdate()
    {
        // 1. 获取水平发射方向（基于出生时的 Y 轴，但抹掉高度差）
        Vector3 rawDir = transform.right; 
        Vector3 moveDir = new Vector3(rawDir.x, 0, rawDir.z).normalized;

        // 2. 只在水平面施加推力
        Vector3 currentHorizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (currentHorizontalVel.magnitude < maxSpeed)
        {
            rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);
        }

        // 3. 视觉滚动
        if (meshChild != null)
        {
            float distance = rb.velocity.magnitude * Time.fixedDeltaTime;
            float angle = (distance / radius) * (180f / Mathf.PI);
            // 绕 X 轴旋转，如果方向反了就改 -angle
            meshChild.Rotate(Vector3.up, angle, Space.Self);
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
// Assets/Scripts/Bullet/Behaviors/TrackingBehavior.cs
using UnityEngine;

public class TrackingBehavior : IBulletBehavior
{
    private Transform target;
    private float speed;
    private float turnSpeed;  // 轉向速度（度/秒）
    
    public TrackingBehavior(Transform targetObject, float moveSpeed, float trackingTurnSpeed)
    {
        target = targetObject;
        speed = moveSpeed;
        turnSpeed = trackingTurnSpeed;
    }
    
    public void Initialize(Bullet bullet) { }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        if (target == null)
            return true;
        
        // 計算指向目標的方向
        Vector3 directionToTarget = (target.position - bullet.position).normalized;
        
        // 從當前速度方向平滑轉向目標方向
        Vector3 currentDirection = bullet.velocity.normalized;
        Vector3 newDirection = Vector3.Lerp(
            currentDirection, 
            directionToTarget, 
            turnSpeed * deltaTime
        ).normalized;
        
        bullet.velocity = newDirection * speed;
        
        return true;
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

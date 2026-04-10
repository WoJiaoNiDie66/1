// Assets/Scripts/Bullet/Behaviors/AccelerationBehavior.cs
using UnityEngine;

public class AccelerationBehavior : IBulletBehavior
{
    private Vector3 acceleration;
    private float maxSpeed;
    
    public AccelerationBehavior(Vector3 accelerationVector, float maxVelocity = float.MaxValue)
    {
        acceleration = accelerationVector;
        maxSpeed = maxVelocity;
    }
    
    public void Initialize(Bullet bullet) { }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        bullet.velocity += acceleration * deltaTime;
        
        // 限制最大速度
        if (bullet.velocity.magnitude > maxSpeed)
        {
            bullet.velocity = bullet.velocity.normalized * maxSpeed;
        }
        
        return true;
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

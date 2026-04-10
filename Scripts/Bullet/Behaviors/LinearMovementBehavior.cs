// Assets/Scripts/Bullet/Behaviors/LinearMovementBehavior.cs
using UnityEngine;

public class LinearMovementBehavior : IBulletBehavior
{
    private Vector3 direction;
    private float speed;
    
    public LinearMovementBehavior(Vector3 moveDirection, float moveSpeed)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
    }
    
    public void Initialize(Bullet bullet)
    {
        bullet.velocity = direction * speed;
    }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        // 保持速度不變，持續返回 true（永不結束）
        return true;
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

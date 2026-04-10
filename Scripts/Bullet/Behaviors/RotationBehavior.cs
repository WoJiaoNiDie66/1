// Assets/Scripts/Bullet/Behaviors/RotationBehavior.cs
using UnityEngine;

public class RotationBehavior : IBulletBehavior
{
    private Vector3 rotationAxis;
    private float degreesPerSecond;
    
    public RotationBehavior(Vector3 axis, float rotationSpeed)
    {
        rotationAxis = axis.normalized;
        degreesPerSecond = rotationSpeed;
    }
    
    public void Initialize(Bullet bullet) { }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        float rotationAmount = degreesPerSecond * deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, rotationAxis);
        
        bullet.velocity = rotation * bullet.velocity;
        
        return true;
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

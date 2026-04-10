// Assets/Scripts/Bullet/Behaviors/SineOffsetBehavior.cs
using UnityEngine;

public class SineOffsetBehavior : IBulletBehavior
{
    private Vector3 baseVelocity;
    private Vector3 offsetAxis;      // 偏移的軸向
    private float frequency;          // 頻率（Hz）
    private float amplitude;          // 振幅
    private float elapsedTime = 0f;
    
    public SineOffsetBehavior(Vector3 baseVel, Vector3 axis, float freq, float amp)
    {
        baseVelocity = baseVel;
        offsetAxis = axis.normalized;
        frequency = freq;
        amplitude = amp;
    }
    
    public void Initialize(Bullet bullet) { }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        elapsedTime += deltaTime;
        
        // 計算正弦值作為偏移
        float sineValue = Mathf.Sin(elapsedTime * frequency * 2f * Mathf.PI);
        Vector3 offset = offsetAxis * sineValue * amplitude;
        
        // 基礎速度 + 正弦偏移
        bullet.velocity = baseVelocity + offset;
        
        return true;
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

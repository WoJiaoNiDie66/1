// Assets/Scripts/Bullet/Behaviors/ConditionalEndBehavior.cs
using UnityEngine;
using System;

public class ConditionalEndBehavior : IBulletBehavior
{
    private Func<Bullet, bool> condition;  // 返回 true 時結束行為
    
    public ConditionalEndBehavior(Func<Bullet, bool> endCondition)
    {
        condition = endCondition;
    }
    
    public void Initialize(Bullet bullet) { }
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        return !condition(bullet);
    }
    
    public void OnBehaviorEnd(Bullet bullet) { }
}

// Assets/Scripts/Bullet/Behaviors/TimedBehavior.cs
using UnityEngine;

public class TimedBehavior : IBulletBehavior
{
    private IBulletBehavior wrappedBehavior;
    private float duration;
    private float elapsedTime = 0f;

    public TimedBehavior(IBulletBehavior behavior, float timeLimit)
    {
        wrappedBehavior = behavior;
        duration = timeLimit;
    }

    public void Initialize(Bullet bullet)
    {
        wrappedBehavior.Initialize(bullet);
    }

    public bool Update(Bullet bullet, float deltaTime)
    {
        elapsedTime += deltaTime;

        // 如果時間未超時，繼續執行包裝的行為
        bool wrappedResult = wrappedBehavior.Update(bullet, deltaTime);

        if (elapsedTime >= duration)
        {
            return false;  // 時間到，結束此行為
        }

        return wrappedResult;
    }

    public void OnBehaviorEnd(Bullet bullet)
    {
        wrappedBehavior.OnBehaviorEnd(bullet);
    }
}

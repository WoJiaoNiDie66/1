using UnityEngine;

public class SimpleParticleControl : MonoBehaviour
{
    private ParticleSystem[] ps;

    void Start()
    {
        ps = GetComponentsInChildren<ParticleSystem>(); // 获取所有子对象中的 ParticleSystem 组件
    }

    public void Play()
    {
        if (ps == null) return;
        foreach (var system in ps)
            system?.Play();
    }

    public void Stop()
    {
        if (ps == null) return;
        foreach (var system in ps)
            system?.Stop();
    }

    public void Restart()
    {
        if (ps == null) return;
        foreach (var system in ps)
        {
            if (system == null) continue;
            system.Stop();
            system.Clear();
            system.Play();
        }
    }
}
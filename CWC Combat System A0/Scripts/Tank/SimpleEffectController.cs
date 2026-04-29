using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleEffectController : MonoBehaviour
{
    [System.Serializable]
    public class EffectItem
    {
        public string name;           // 特效名称
        public string path;           // 子物体路径
        public float lifeTime = 1f;   // 生命周期（秒），0表示手动停止
        public bool autoDisable = true; // 生命周期结束后自动禁用
    }
    
    [Header("特效列表")]
    [SerializeField] private List<EffectItem> effects = new List<EffectItem>();
    
    private Dictionary<string, GameObject> effectObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, ParticleSystem> effectParticles = new Dictionary<string, ParticleSystem>();
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    
    void Start()
    {
        // 缓存所有特效
        foreach (var effect in effects)
        {
            Transform t = transform.Find(effect.path);
            if (t != null)
            {
                effectObjects[effect.name] = t.gameObject;
                effectParticles[effect.name] = t.GetComponent<ParticleSystem>();
                
                // 初始时禁用
                t.gameObject.SetActive(false);
            }
        }
    }
    
    // 播放特效（通过名称）
    public void Play(string effectName)
    {
        if (!effectObjects.TryGetValue(effectName, out GameObject obj)) return;
        
        // 停止之前的协程
        if (activeCoroutines.TryGetValue(effectName, out Coroutine oldCoroutine))
        {
            StopCoroutine(oldCoroutine);
            activeCoroutines.Remove(effectName);
        }
        
        // 激活并播放
        obj.SetActive(true);
        if (effectParticles.TryGetValue(effectName, out ParticleSystem ps))
        {
            ps.Play();
        }
        
        // 获取生命周期
        EffectItem effect = effects.Find(e => e.name == effectName);
        if (effect != null && effect.lifeTime > 0)
        {
            // 启动自动停止协程
            Coroutine coroutine = StartCoroutine(AutoStop(effectName, effect.lifeTime, effect.autoDisable));
            activeCoroutines[effectName] = coroutine;
        }
    }
    
    // 停止特效
    public void Stop(string effectName)
    {
        if (!effectObjects.TryGetValue(effectName, out GameObject obj)) return;
        
        // 停止协程
        if (activeCoroutines.TryGetValue(effectName, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            activeCoroutines.Remove(effectName);
        }
        
        // 停止粒子并禁用
        if (effectParticles.TryGetValue(effectName, out ParticleSystem ps))
        {
            ps.Stop();
        }
        
        obj.SetActive(false);
    }
    
    // 播放并停止之前的特效（重启）
    public void Restart(string effectName)
    {
        Stop(effectName);
        Play(effectName);
    }
    
    // 通过索引播放
    public void PlayByIndex(int index)
    {
        if (index >= 0 && index < effects.Count)
        {
            Play(effects[index].name);
        }
    }
    
    // 通过索引停止
    public void StopByIndex(int index)
    {
        if (index >= 0 && index < effects.Count)
        {
            Stop(effects[index].name);
        }
    }
    
    // 停止所有特效
    public void StopAll()
    {
        foreach (var name in effectObjects.Keys)
        {
            Stop(name);
        }
    }
    
    private IEnumerator AutoStop(string effectName, float delay, bool disable)
    {
        yield return new WaitForSeconds(delay);
        
        if (effectObjects.TryGetValue(effectName, out GameObject obj))
        {
            if (effectParticles.TryGetValue(effectName, out ParticleSystem ps))
            {
                ps.Stop();
            }
            
            if (disable)
            {
                obj.SetActive(false);
            }
        }
        
        activeCoroutines.Remove(effectName);
    }
}
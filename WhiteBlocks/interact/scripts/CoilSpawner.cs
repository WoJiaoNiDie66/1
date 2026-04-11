using System.Collections;
using System.Collections.Generic; // 增加这个，为了使用 List
using UnityEngine;

public class CoilSpawner : MonoBehaviour
{
    [Header("预制体配置")]
    public GameObject coilPrefab;    // 钢卷预制体
    public Transform spawnPoint;     // 产生点

    [Header("特效物体配置")]
    // 拖入你场景里的特效物体 (coil_spawner_)
    public GameObject vfxObject;     

    [Header("时序配置")]
    // 两次发射之间的总间隔时间
    public float interval = 5f;
    // 特效出现后，等待多少秒再吐出钢卷 (预热时间)
    public float delayBeforeCoilSpawn = 1.0f; 

    private List<ParticleSystem> childParticleSystems = new List<ParticleSystem>();

    void Start()
    {
        // 1. 初始化特效物体：确保它是开启状态，但里面的粒子系统先停止
        if (vfxObject != null)
        {
            vfxObject.SetActive(true); 
            // 自动寻找下面所有的粒子系统子层
            var allPS = vfxObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in allPS)
            {
                ps.Stop(true); // 初始先停止喷发
                childParticleSystems.Add(ps);
            }
        }
        
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // --- 阶段1：特效预热 (特效亮起) ---
            if (childParticleSystems.Count > 0)
            {
                foreach (var ps in childParticleSystems)
                {
                    // 开启“唤醒播放”，确保它立刻重头喷发
                    ps.Play(true); 
                }
            }

            // 等待预热时间 (比如设为 1.0 秒)
            yield return new WaitForSeconds(delayBeforeCoilSpawn);

            // --- 阶段2：吐出钢卷 (产生钢卷) ---
            Instantiate(coilPrefab, spawnPoint.position, spawnPoint.rotation);

            // --- 阶段3：特效淡出 (停止产生新粒子) ---
            if (childParticleSystems.Count > 0)
            {
                foreach (var ps in childParticleSystems)
                {
                    // 这个很关键：停止“产生新粒子”，不打断已有的
                    // 这样已有的粒子会根据它们的生命周期慢慢淡出
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            // 计算剩下的等待时间 ( interval - 预热占用的时间 )
            yield return new WaitForSeconds(Mathf.Max(0, interval - delayBeforeCoilSpawn));
        }
    }
}
using UnityEngine;
using System.Collections;

public class FireShootingController : MonoBehaviour
{
    [Header("下降设置")]
    [Tooltip("下降的总距离")]
    public float dropDistance = 5f; 
    
    [Tooltip("下降的速度（数字越小降得越慢）")]
    public float dropSpeed = 1.5f;  

    [Header("喷火设置")]
    [Tooltip("每次喷火持续的时间（秒）")]
    public float fireOnTime = 2.0f; // 喷 2 秒

    [Tooltip("每次喷火停止的等待时间（秒）")]
    public float fireOffTime = 3.0f; // 停 3 秒

    // 防止被连续砍中触发多次
    private bool hasBeenHit = false; 
    
    // 储存所有的粒子系统，方便挨打时把它们关掉
    private ParticleSystem[] fireSystems;

    void Start()
    {
        // 游戏一开始，抓取所有子物件里的粒子系统
        fireSystems = GetComponentsInChildren<ParticleSystem>();
        
        if (fireSystems.Length > 0)
        {
            foreach (ParticleSystem ps in fireSystems)
            {
                var main = ps.main;
                main.loop = true; 
                main.playOnAwake = false; // 关闭自带启动，由脚本接管
                
                // 启动循环喷火
                StartCoroutine(FireCycleRoutine(ps));
            }
            Debug.Log("【机关就绪】开始间歇性喷火！");
        }
    }

    // 当被玩家武器砍中时
    private void OnTriggerEnter(Collider other)
    {
        // 如果碰到了武器，且还没被破坏过
        if (!hasBeenHit && other.CompareTag("Weapon"))
        {
            hasBeenHit = true; 
            Debug.Log("【机关被破坏】立刻停止喷火！");

            // ⚡ 核心逻辑 1：瞬间停止所有正在运行的协程（打断喷火的死循环）
            StopAllCoroutines(); 

            // ⚡ 核心逻辑 2：强制关掉所有的火
            foreach (ParticleSystem ps in fireSystems)
            {
                if (ps != null)
                {
                    // StopEmitting：不再喷新火，让空中的火花自然消散（看起来更真实）
                    // 如果你想让火瞬间消失，可以改成 ParticleSystemStopBehavior.StopEmittingAndClear
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); 
                }
            }

            // ⚡ 核心逻辑 3：启动机关下降的动画
            StartCoroutine(DescentRoutine()); 
        }
    }

    // 处理缓缓下降的协程
    IEnumerator DescentRoutine()
    {
        Vector3 targetPosition = transform.position - new Vector3(0, dropDistance, 0);

        // 只要还没沉到底部，就继续沉
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dropSpeed * Time.deltaTime);
            yield return null; 
        }

        transform.position = targetPosition;
        Debug.Log("机关已完全沉入地下。");
    }

    // 喷火循环逻辑
    IEnumerator FireCycleRoutine(ParticleSystem ps)
    {
        // 这个循环会一直跑，直到被 OnTriggerEnter 里的 StopAllCoroutines() 强行打断
        while (true)
        {
            // 喷火
            if (ps != null) ps.Play(true); 
            yield return new WaitForSeconds(fireOnTime);

            // 停火
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); 
            yield return new WaitForSeconds(fireOffTime);
        }
    }
}
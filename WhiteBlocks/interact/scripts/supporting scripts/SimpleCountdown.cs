using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SimpleCountdown : MonoBehaviour
{
    [System.Serializable]
    public class Keyframe
    {
        public float time;
        public UnityEvent onKeyframe;
    }

    [Header("Keyframe 设置")]
    [SerializeField] private bool startOnAwake = true;  // 是否自动开始
    public List<Keyframe> keyframes = new List<Keyframe>();
    
    [Header("事件")]
    public UnityEvent onComplete;  // 所有关键帧完成后触发
    public bool loop = false; // 是否循环播放
    
    private float currentTime;
    private bool isPlaying = false;
    private int currentIndex = 0;
    
    void Awake()
    {
        // 假设 keyframes 已按 time 排序
        if (startOnAwake)
        {
            StartPlayback();
        }
    }
    
    void Update()
    {
        if (!isPlaying || currentIndex >= keyframes.Count) return;
        
        currentTime += Time.deltaTime;
        
        while (currentIndex < keyframes.Count && currentTime >= keyframes[currentIndex].time)
        {
            keyframes[currentIndex].onKeyframe?.Invoke();
            currentIndex++;
        }
        
        if (currentIndex >= keyframes.Count)
        {
            CompletePlayback();
            StartPlayback(); // 如果循环，立即重新开始
        }
    }
    
    public void StartPlayback()
    {
        currentTime = 0f;
        currentIndex = 0;
        isPlaying = true;
    }
    
    public void StopPlayback()
    {
        isPlaying = false;
    }
    
    public void ResetPlayback()
    {
        currentTime = 0f;
        currentIndex = 0;
        isPlaying = false;
    }
    
    private void CompletePlayback()
    {
        isPlaying = false;
        onComplete?.Invoke();
    }
}
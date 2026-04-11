using UnityEngine;
using System.Collections.Generic;

public class SimpleSmoothScaler : MonoBehaviour
{
    [System.Serializable]
    public class Keyframe
    {
        public float time;
        public Vector3 scale;
    }
    
    public List<Keyframe> keyframes = new List<Keyframe>();
    public bool loop = false;
    
    private float timer = 0f;
    private bool isPlaying = true;
    
    void Update()
    {
        if (!isPlaying)
        {
            transform.localScale = keyframes.Count > 0 ? keyframes[keyframes.Count - 1].scale : Vector3.one;
            return;
        }
        if (keyframes.Count < 2) return;
        
        timer += Time.deltaTime;
        float duration = keyframes[keyframes.Count - 1].time;
        
        if (timer >= duration)
        {
            if (loop)
                timer = 0f;
            else
                timer = duration;
        }
        
        // 找到当前区间并插值
        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            if (timer >= keyframes[i].time && timer <= keyframes[i + 1].time)
            {
                float t = (timer - keyframes[i].time) / (keyframes[i + 1].time - keyframes[i].time);
                Vector3 scale = Vector3.Lerp(keyframes[i].scale, keyframes[i + 1].scale, t);
                transform.localScale = scale;
                break;
            }
        }
    }
    
    public void Play() => isPlaying = true;
    public void Stop() => isPlaying = false;
}
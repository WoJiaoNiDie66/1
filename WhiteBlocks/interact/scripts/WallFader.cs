using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个脚本挂载到墙壁的父级，它实现了交互接口
public class WallFader : MonoBehaviour, IInteractable
{
    [Header("淡入淡出设置")]
    public float fadeDuration = 0.5f; // 时间
    public float fadedAlpha = 0.1f;    // 淡出后的透明度 (0为完全看不见，建议留一点点)

    private Renderer[] renderers;
    private List<Material> materials = new List<Material>();
    public bool isFaded = false; // 当前是否处于淡出状态
    private Coroutine currentFadeCoroutine;
    private string colorPropertyName;

    void Awake()
    {
        // 自动找到所有子物体(LOD0, LOD1等)上的Renderer
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            materials.AddRange(rend.materials); // 使用实例化材质，防止全局影响
        }

        // 兼容 URP 的 _BaseColor 和 Standard 的 _Color
        if (materials.Count > 0)
        {
            bool isURP = materials[0].HasProperty("_BaseColor");
            colorPropertyName = isURP ? "_BaseColor" : "_Color";
        }
    }

    void Start()
    {
        // 初始化状态：如果一开始就要淡出，就直接设置透明度
        if (isFaded)
        {
            foreach (Material mat in materials)
            {
                Color color = mat.GetColor(colorPropertyName);
                color.a = fadedAlpha;
                mat.SetColor(colorPropertyName, color);
            }
            ToggleColliders(false); // 一开始就淡出的话，碰撞体也要关掉
        }
        else
        {
            ToggleColliders(true); // 确保一开始碰撞体是开启的
        }
    }

    // --- 实现 IInteractable 接口的核心方法 ---
    public void Interact()
    {
        InteractStyle(!isFaded);
    }

    public void InteractStyle(bool Fade) // 切换状态，传入目标状态
    {
        // 如果墙壁正在淡入/淡出中，不允许再次触发，防止逻辑错乱
        if (currentFadeCoroutine != null) return;

        // 根据当前状态切换
        isFaded = !Fade; // 直接根据传入的参数设置状态
        if (isFaded)
        {
            // 它是透明的，让它显示出来 (Fade In)
            currentFadeCoroutine = StartCoroutine(FadeRoutine(1.0f));
            isFaded = false;
        }
        else
        {
            // 它是显示的，让它变透明 (Fade Out)
            currentFadeCoroutine = StartCoroutine(FadeRoutine(fadedAlpha));
            isFaded = true;
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (materials.Count == 0)
        {
            currentFadeCoroutine = null;
            yield break;
        }

        float currentAlpha = materials[0].GetColor(colorPropertyName).a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsedTime / fadeDuration);

            foreach (Material mat in materials)
            {
                Color color = mat.GetColor(colorPropertyName);
                color.a = newAlpha;
                mat.SetColor(colorPropertyName, color);
            }
            yield return null;
        }

        // 确保最终透明度准确到达
        foreach (Material mat in materials)
        {
            Color finalColor = mat.GetColor(colorPropertyName);
            finalColor.a = targetAlpha;
            mat.SetColor(colorPropertyName, finalColor);
        }

        // 如果是要消失，完全隱形後才把碰撞體關掉，讓玩家掉下去
        if (targetAlpha < 0.5f) ToggleColliders(false);
        else ToggleColliders(true);

        currentFadeCoroutine = null; // 标记协程已结束，允许下次交互
    }

    private void ToggleColliders(bool enable)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = enable;
        }
    }
}
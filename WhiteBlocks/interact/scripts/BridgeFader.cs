using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BridgeFader : MonoBehaviour
{
    [Header("淡入淡出設定")]
    public float fadeDuration = 2f; // 淡入淡出花費的時間（秒）
    
    [Tooltip("打勾表示遊戲一開始橋是隱形的")]
    public bool startHidden = false; 

    // 儲存所有子物件的材質與碰撞體
    private List<Material> bridgeMaterials = new List<Material>();
    private Collider[] colliders;

    void Start()
    {
        // 1. 自動抓取底下所有子物件的 MeshRenderer 和 Collider
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        foreach (Renderer ren in renderers)
        {
            // 使用 .materials 會自動生成實例，這樣不會改壞原本的材質球檔案
            bridgeMaterials.AddRange(ren.materials);
        }

        // 2. 根據初始設定決定一開始的狀態
        if (startHidden)
        {
            SetAlphaInstant(0f);
            ToggleColliders(false);
        }
    }

    // ==========================================
    // 給外部呼叫的方法 (例如踩到機關、按按鈕時呼叫)
    // ==========================================

    [ContextMenu("測試：淡入 (出現)")]
    public void FadeInBridge()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f));
    }

    [ContextMenu("測試：淡出 (消失)")]
    public void FadeOutBridge()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f));
    }

    // ==========================================
    // 核心漸變邏輯
    // ==========================================

    IEnumerator FadeRoutine(float targetAlpha)
    {
        // 如果是要出現，一開始就把碰撞體打開，免得玩家踩空
        if (targetAlpha > 0.5f) ToggleColliders(true);

        if (bridgeMaterials.Count == 0) yield break;

        // 取得目前的透明度當作起點
        float startAlpha = GetCurrentAlpha(bridgeMaterials[0]);
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlphaInstant(currentAlpha);
            yield return null; // 等待下一幀
        }

        // 確保最終數值精準
        SetAlphaInstant(targetAlpha);

        // 如果是要消失，完全隱形後才把碰撞體關掉，讓玩家掉下去
        if (targetAlpha < 0.5f) ToggleColliders(false);
    }

    // --- 輔助方法 ---

    private void SetAlphaInstant(float alpha)
    {
        foreach (Material mat in bridgeMaterials)
        {
            // 兼容 Standard Shader (_Color) 與 URP Shader (_BaseColor)
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
            else if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
        }
    }

    private float GetCurrentAlpha(Material mat)
    {
        if (mat.HasProperty("_Color")) return mat.color.a;
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor").a;
        return 1f;
    }

    private void ToggleColliders(bool state)
    {
        foreach (Collider col in colliders)
        {
            col.enabled = state;
        }
    }
}
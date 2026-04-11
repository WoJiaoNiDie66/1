using UnityEngine;

public class ShatterForward : MonoBehaviour
{
    [Header("坍塌設定")]
    public float force = 250f;   
    public float radius = 5f;    
    public Vector3 globalExplosionOffset = new Vector3(0f, 1f, -2f); 

    [Header("要隱藏的完整牆壁")]
    public GameObject originalWall; 

    private FracturedObject fracturedObj;
    private bool hasShattered = false;

    void Start()
    {
        fracturedObj = GetComponent<FracturedObject>();
    }

    // 當觸發器碰到時
    void OnTriggerEnter(Collider other)
    {
        if (!hasShattered && other.CompareTag("Weapon"))
        {
            PrepareToCollapse();
        }
    }

    // 當實體碰撞碰到時
    void OnCollisionEnter(Collision collision)
    {
        if (!hasShattered && collision.gameObject.CompareTag("Weapon"))
        {
            PrepareToCollapse();
        }
    }

    // ==========================================
    // 緩衝處理：防止物理引擎因為穿模而暴走
    // ==========================================
    private void PrepareToCollapse()
    {
        hasShattered = true; // 1. 瞬間鎖上安全鎖，防止一刀觸發十次

        // 2. 瞬間關閉這面牆原本的碰撞體 (Mesh Collider / Box Collider)
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        // 3. 延遲 0.05 秒再引爆！讓你的刀先揮過去，避免碎塊生成在刀子裡面被光速彈飛
        Invoke("CollapseWall", 0.05f);
    }

    public void CollapseWall()
    {
        if (originalWall != null) originalWall.SetActive(false);

        MeshRenderer parentRenderer = GetComponent<MeshRenderer>();
        if (parentRenderer != null) parentRenderer.enabled = false;

        if (fracturedObj != null)
        {
            Vector3 trueCenter = transform.position; 
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                trueCenter = renderer.bounds.center;
            }

            Vector3 explosionCenter = trueCenter + globalExplosionOffset;
            fracturedObj.Explode(explosionCenter, force, radius, false, false, false, false);
        }
    }
}
using UnityEngine;
using DG.Tweening;

public class WallObject_SlingBoom : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private bool isIndestructible = false; // Tường không thể phá (dành cho tường biên)

    [Header("Visuals")]
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private MeshRenderer meshRenderer;

    private bool isDestroyed = false;

    // Bị đánh = biến mất luôn (không có HP)
    public void TakeDamage()
    {
        if (isDestroyed || isIndestructible) return;

        Debug.Log($"Wall {gameObject.name} bị phá hủy!");
        DestroyWall();
    }

    // Phá hủy tường
    private void DestroyWall()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Tạo hiệu ứng phá hủy
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        // Animation biến mất
        if (meshRenderer != null)
        {
            transform.DOScale(0f, 0.2f).SetEase(Ease.InBack);
            
            Material mat = meshRenderer.material;
            if (mat != null)
            {
                mat.DOFade(0f, 0.2f);
            }
        }

        // Hủy object sau 0.2s
        Destroy(gameObject, 0.2f);
    }

    // Getters
    public bool IsDestroyed() => isDestroyed;
    public bool IsIndestructible() => isIndestructible;

    // Vẽ trong Editor để phân biệt wall thường và wall bất tử
    private void OnDrawGizmos()
    {
        if (isIndestructible)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale * 1.05f);
        }
    }
}
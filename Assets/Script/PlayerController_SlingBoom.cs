using UnityEngine;
using DG.Tweening;
using System.Collections.Generic; // Cần thêm dòng này

public class PlayerController_SlingBoom : MonoBehaviour
{
    [Header("Bullet Prefabs")]
    [SerializeField] private GameObject normalBulletPrefab;
    [SerializeField] private GameObject bombBulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Settings")]
    [SerializeField] private float maxForceMultiplier = 15f;
    [SerializeField] private float spreadAngle = 10f;

    private BulletType currentBulletType = BulletType.Normal;
    private Collider playerCollider;
    public float MaxForce => maxForceMultiplier;

    private void Start()
    {
        playerCollider = GetComponent<Collider>();
    }

    public void SetBulletType(BulletType type)
    {
        currentBulletType = type;
        Debug.Log($"Đã chọn đạn: {type}");
    }

    public void Shoot(Vector3 finalVelocity)
    {
        switch (currentBulletType)
        {
            case BulletType.Normal:
                SpawnBullet(normalBulletPrefab, finalVelocity);
                break;

            case BulletType.Bomb:
                SpawnBullet(bombBulletPrefab, finalVelocity);
                break;

            case BulletType.Triple:
                HandleTripleShot(finalVelocity);
                break;
        }
    }

    // --- ĐÃ SỬA: Logic bắn 3 tia không va chạm nhau ---
    private void HandleTripleShot(Vector3 mainVelocity)
    {
        List<GameObject> bullets = new List<GameObject>();

        // 1. Viên Giữa
        bullets.Add(SpawnBullet(normalBulletPrefab, mainVelocity));

        // 2. Viên Trên
        Vector3 upVelocity = Quaternion.Euler(0, 0, spreadAngle) * mainVelocity;
        bullets.Add(SpawnBullet(normalBulletPrefab, upVelocity));

        // 3. Viên Dưới
        Vector3 downVelocity = Quaternion.Euler(0, 0, -spreadAngle) * mainVelocity;
        bullets.Add(SpawnBullet(normalBulletPrefab, downVelocity));

        // --- FIX: Tắt va chạm giữa các viên đạn ---
        for (int i = 0; i < bullets.Count; i++)
        {
            for (int j = i + 1; j < bullets.Count; j++)
            {
                if (bullets[i] != null && bullets[j] != null)
                {
                    Collider c1 = bullets[i].GetComponent<Collider>();
                    Collider c2 = bullets[j].GetComponent<Collider>();
                    if (c1 != null && c2 != null)
                    {
                        Physics.IgnoreCollision(c1, c2, true);
                    }
                }
            }
        }
    }

    // --- ĐÃ SỬA: Trả về GameObject ---
    private GameObject SpawnBullet(GameObject prefab, Vector3 velocity)
    {
        if (prefab == null || firePoint == null) return null;

        GameObject bullet = Instantiate(prefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Collider bulletCollider = bullet.GetComponent<Collider>();

        // Fix lỗi tự va chạm với người bắn
        if (playerCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, bulletCollider);
        }

        // Setup Owner (Nếu PlayerController điều khiển 1 GameUnit, ta cần gán Owner để tính điểm/sát thương)
        // Đoạn này lấy component GameUnit từ chính Player object để gán
        GameUnit_SlingBoom ownerUnit = GetComponent<GameUnit_SlingBoom>();
        BulletController_SlingBoom bulletCtrl = bullet.GetComponent<BulletController_SlingBoom>();
        if (bulletCtrl != null && ownerUnit != null)
        {
            bulletCtrl.SetOwner(ownerUnit);
        }

        if (rb != null)
        {
            rb.isKinematic = false;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = velocity;
#else
            rb.velocity = velocity;
#endif
        }

        return bullet;
    }
}
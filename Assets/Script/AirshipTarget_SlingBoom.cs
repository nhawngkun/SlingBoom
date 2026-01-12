using UnityEngine;
using DG.Tweening;

public class AirshipTarget_SlingBoom : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA; // Điểm bay đầu tiên
    [SerializeField] private Transform pointB; // Điểm bay thứ hai
    [SerializeField] private float moveDuration = 5f; // Thời gian bay từ A → B
    [SerializeField] private Ease moveEase = Ease.InOutSine;

    [Header("Model Settings")]
    [SerializeField] private Transform modelTransform; // Model con bên trong (nếu có)
    [SerializeField] private Vector3 baseRotation = new Vector3(-90, 0, 0); // Rotation gốc của model
    [SerializeField] private bool flipModelOnTurn = true; // Có lật model khi đổi hướng không

    [Header("Energy Buff Settings")]
    [SerializeField] private int energyBuffAmount = 2; // Tăng 2 energy khi bắn trúng

    [Header("Visual Effects")]
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private AudioClip destroySound;

    [Header("Hit Detection")]
    [SerializeField] private Collider airshipCollider;

    private bool isDestroyed = false;
    private Tween moveTween;
    private bool movingToB = true; // Đang bay về B hay A

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"[AirshipTarget] {gameObject.name} thiếu pointA hoặc pointB!");
            return;
        }

        if (airshipCollider == null)
        {
            airshipCollider = GetComponent<Collider>();
        }

        // Nếu không có modelTransform, dùng chính transform này
        if (modelTransform == null)
        {
            modelTransform = transform;
        }

        // Set rotation ban đầu
        modelTransform.rotation = Quaternion.Euler(baseRotation);

        // Bắt đầu bay từ pointA
        transform.position = pointA.position;
        StartFlying();
    }

    private void StartFlying()
    {
        if (isDestroyed) return;

        movingToB = true;

        // Xoay model về hướng B (nếu cần)
        if (flipModelOnTurn)
        {
            SetModelDirection(true); // Hướng về B
        }

        // Di chuyển từ A → B
        moveTween = transform.DOMove(pointB.position, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                if (!isDestroyed)
                {
                    FlyBackToA();
                }
            });
    }

    private void FlyBackToA()
    {
        if (isDestroyed) return;

        movingToB = false;

        // Xoay model về hướng A (nếu cần)
        if (flipModelOnTurn)
        {
            SetModelDirection(false); // Hướng về A
        }

        // Di chuyển từ B → A
        moveTween = transform.DOMove(pointA.position, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                if (!isDestroyed)
                {
                    StartFlying();
                }
            });
    }

    // ✅ HÀM MỚI: Set hướng của model (giữ nguyên base rotation X=-90)
    private void SetModelDirection(bool towardsB)
    {
        if (modelTransform == null) return;

        // Tính hướng bay
        Vector3 direction = towardsB ?
            (pointB.position - pointA.position) :
            (pointA.position - pointB.position);

        // Tính góc Y dựa trên hướng bay (giữ nguyên X=-90, Z=0)
        float targetYRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Áp dụng rotation: giữ nguyên baseRotation.x (-90), chỉ thay đổi Y
        Vector3 targetRotation = new Vector3(
            baseRotation.x,  // Giữ nguyên -90
            targetYRotation, // Xoay theo hướng bay
            baseRotation.z   // Giữ nguyên 0
        );

        modelTransform.rotation = Quaternion.Euler(targetRotation);

        Debug.Log($"[Airship] Direction: {(towardsB ? "A→B" : "B→A")}, Y Rotation: {targetYRotation:F1}°");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        // Kiểm tra xem có phải đạn không
        BulletController_SlingBoom bullet = collision.gameObject.GetComponent<BulletController_SlingBoom>();

        if (bullet != null)
        {
            Debug.Log($"[AirshipTarget] {gameObject.name} bị bắn trúng!");
            OnHit();
        }
    }

    private void OnHit()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Dừng tween di chuyển
        if (moveTween != null)
        {
            moveTween.Kill();
        }

        // ✅ BUFF NĂNG LƯỢNG CHO TẤT CẢ PLAYERS
        BuffAllPlayers();

        // Hiệu ứng phá hủy
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

       

        // Animation biến mất
        transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);

        // Hủy object
        Destroy(gameObject, 0.3f);
    }

    // ✅ HÀM BUFF NĂNG LƯỢNG CHO PLAYERS
    private void BuffAllPlayers()
    {
        if (TurnBasedGameManager.Instance == null)
        {
            Debug.LogError("[AirshipTarget] TurnBasedGameManager.Instance is null!");
            return;
        }

        // Tăng Max Energy của shared player state
        TurnBasedGameManager.Instance.BuffPlayerMaxEnergy(energyBuffAmount);

        Debug.Log($"[AirshipTarget] ✅ Buffed player energy by +{energyBuffAmount}!");

        // Hiển thị thông báo trên UI (optional)
        UIGameplay_SlingBoom uiGameplay = TurnBasedGameManager.Instance.GetUIGameplay();
        if (uiGameplay != null)
        {
            Debug.Log($"[AirshipTarget] 🚀 AIRSHIP DESTROYED! Energy +{energyBuffAmount}!");
        }
    }

    private void OnDestroy()
    {
        // Cleanup tweens
        if (moveTween != null)
        {
            moveTween.Kill();
        }

        transform.DOKill();
        if (modelTransform != null && modelTransform != transform)
        {
            modelTransform.DOKill();
        }
    }

    // ✅ DEBUG: Vẽ đường bay trong Scene View
    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;

        // Vẽ 2 điểm
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.position, 0.5f);

        // Vẽ đường bay
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);

        // Vẽ mũi tên chỉ hướng
        Vector3 direction = (pointB.position - pointA.position).normalized;
        Vector3 midPoint = (pointA.position + pointB.position) / 2f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(midPoint, direction * 2f);

        // Vẽ tên điểm
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pointA.position + Vector3.up, "Point A");
        UnityEditor.Handles.Label(pointB.position + Vector3.up, "Point B");
#endif
    }

    // ✅ DEBUG: Vẽ text trong Scene View
    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;

        // Label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"Airship: +{energyBuffAmount} Energy\n" +
            $"Move Time: {moveDuration}s\n" +
            $"Base Rotation: {baseRotation}\n" +
            $"Direction: {(movingToB ? "→ B" : "→ A")}");
#endif
    }
}
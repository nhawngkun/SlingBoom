using UnityEngine;
using DG.Tweening;

public class BulletController_SlingBoom : MonoBehaviour
{
    [Header("Explosion Stats")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private int damage = 20;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Visual Effects")]
    [SerializeField] private float rotateDuration = 0.5f;
    [SerializeField] private Vector3 rotationAxis = new Vector3(360, 0, 0);

    [Header("Spin Inaccuracy - ĐỘ LỆCH DO XOAY")]
    [SerializeField] private bool enableSpinInaccuracy = true; // ✅ Bật/tắt tính năng lệch
    [SerializeField] private float spinDeflectionForce = 0.5f; // ✅ Lực lệch do xoay (0.5 = lệch nhẹ)
    [SerializeField] private float deflectionInterval = 0.1f; // ✅ Cứ 0.1s lại lệch 1 lần

    private bool hasExploded = false;
    private GameUnit_SlingBoom owner;
    private Rigidbody rb;
    private Tween rotationTween;
    private float deflectionTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = 0f;
#else
            rb.drag = 0f;
#endif
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Xoay liên tục
        rotationTween = transform.DORotate(rotationAxis, rotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    private void FixedUpdate()
    {
        // ✅ TÍNH NĂNG MỚI: ĐẠN XOAY LÀM CHO BẮN LỆCH
        if (enableSpinInaccuracy && rb != null && !hasExploded)
        {
            deflectionTimer += Time.fixedDeltaTime;

            if (deflectionTimer >= deflectionInterval)
            {
                deflectionTimer = 0f;

                // Thêm lực lệch ngẫu nhiên theo trục X và Y
                Vector3 randomDeflection = new Vector3(
                    Random.Range(-spinDeflectionForce, spinDeflectionForce),
                    Random.Range(-spinDeflectionForce * 0.5f, spinDeflectionForce * 0.5f), // Y lệch ít hơn
                    0
                );

#if UNITY_6000_0_OR_NEWER
                rb.AddForce(randomDeflection, ForceMode.VelocityChange);
#else
                rb.AddForce(randomDeflection, ForceMode.VelocityChange);
#endif
            }
        }
    }

    public void SetOwner(GameUnit_SlingBoom ownerUnit)
    {
        owner = ownerUnit;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (rotationTween != null) rotationTween.Kill();
         if (SoundManager_SlingBoom.Instance != null)
    {
        SoundManager_SlingBoom.Instance.PlayVFXSound(3);
    }

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        if (explosionRadius > 0) Explode();
        else DirectHit(collision.gameObject);

        if (TurnBasedGameManager.Instance != null)
        {
            TurnBasedGameManager.Instance.ScheduleGameOverCheck();
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            ApplyEffect(nearby.gameObject);
        }
    }

    private void DirectHit(GameObject target)
    {
        ApplyEffect(target);
    }

    private void ApplyEffect(GameObject target)
    {
        // ✅ Áp lực nổ cho Rigidbody
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
            targetRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

        // ✅ Gây damage cho GameUnit
        GameUnit_SlingBoom unit = target.GetComponent<GameUnit_SlingBoom>();
        if (unit != null)
        {
            if (unit != owner)
            {
                unit.TakeDamage(damage);
            }
        }

        // ✅ PHÁ HỦY TƯỜNG
        WallObject_SlingBoom wall = target.GetComponent<WallObject_SlingBoom>();
        if (wall != null && !wall.IsDestroyed() && !wall.IsIndestructible())
        {
            wall.TakeDamage();
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
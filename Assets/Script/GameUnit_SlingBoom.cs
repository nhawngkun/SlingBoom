using UnityEngine;
using System.Collections.Generic;

public abstract class GameUnit_SlingBoom : MonoBehaviour
{
    [Header("Unit Info")]
    [SerializeField] protected string unitName = "Unit";
    [SerializeField] protected TeamType team;
    [SerializeField] protected int maxHealth = 100;

    [Header("Energy System")]
    protected int currentEnergy;
    protected int maxEnergy;
    [SerializeField] protected int baseMaxEnergy = 3;

    [Header("Shooting")]
    [SerializeField] protected GameObject normalBulletPrefab;
    [SerializeField] protected GameObject bombBulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float maxForceMultiplier = 15f;

    [Header("Turn Indicator - SPRITE MŨI TÊN")]
    [SerializeField] protected SpriteRenderer turnIndicatorSprite;

    // ✅ FIX: Lưu Z position ban đầu
    [Header("Position Lock")]
    [SerializeField] protected bool lockZPosition = true; // Khóa Z position
    protected float initialZPosition = 0f; // Z position ban đầu

    protected int currentHealth;
    protected bool isDead = false;
    protected bool isMyTurn = false;
    protected Collider unitCollider;

    public string UnitName => unitName;
    public TeamType Team => team;
    public bool IsDead => isDead;
    public bool IsMyTurn => isMyTurn;
    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        unitCollider = GetComponent<Collider>();

        // ✅ LƯU Z POSITION BAN ĐẦU
        initialZPosition = transform.position.z;

        SetArrowVisible(false);
    }

    // ✅ HÀM MỚI: LateUpdate để luôn giữ Z position cố định
    protected virtual void LateUpdate()
    {
        if (lockZPosition)
        {
            // Luôn giữ Z position = initialZPosition
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.z - initialZPosition) > 0.001f)
            {
                pos.z = initialZPosition;
                transform.position = pos;
            }
        }
    }

    public virtual void OnTurnStarted()
    {
        isMyTurn = true;
        SetArrowVisible(true);

        // ✅ ĐẢM BẢO Z POSITION ĐÚNG KHI BẮT ĐẦU TURN
        ForceResetZPosition();

        UIGameplay_SlingBoom uiGameplay = GetUIGameplay();
        if (uiGameplay != null)
        {
            uiGameplay.ShowHealthBarForUnit(this);
            uiGameplay.UpdateHealthBar(this);
        }
    }

    public virtual void OnTurnEnded()
    {
        isMyTurn = false;
        SetArrowVisible(false);

        // ✅ ĐẢM BẢO Z POSITION ĐÚNG KHI KẾT THÚC TURN
        ForceResetZPosition();
    }

    // ✅ HÀM MỚI: Force reset Z position về giá trị ban đầu
    protected void ForceResetZPosition()
    {
        Vector3 pos = transform.position;
        pos.z = initialZPosition;
        transform.position = pos;
    }

    protected void SetArrowVisible(bool visible)
    {
        if (turnIndicatorSprite != null)
        {
            turnIndicatorSprite.enabled = visible;
        }
    }

    public void ConsumeEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // ✅ ĐẢM BẢO Z POSITION SAU KHI BỊ DAMAGE
        ForceResetZPosition();

        UIGameplay_SlingBoom uiGameplay = GetUIGameplay();
        if (uiGameplay != null)
        {
            uiGameplay.UpdateHealthBar(this);
        }

        if (currentHealth <= 0) Die();
    }

    public void ForceDeath()
    {
        Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        currentHealth = 0;

        SetArrowVisible(false);

        UIGameplay_SlingBoom uiGameplay = GetUIGameplay();
        if (uiGameplay != null)
        {
            uiGameplay.UpdateHealthBar(this);
        }

        if (TurnBasedGameManager.Instance != null)
        {
            TurnBasedGameManager.Instance.OnUnitDied(this);
        }

        gameObject.SetActive(false);
    }

    protected UIGameplay_SlingBoom GetUIGameplay()
    {
        if (TurnBasedGameManager.Instance != null)
        {
            return TurnBasedGameManager.Instance.GetUIGameplay();
        }
        return null;
    }

    protected GameObject SpawnBullet(GameObject prefab, Vector3 velocity)
    {
        if (prefab == null || firePoint == null) return null;

        GameObject bullet = Instantiate(prefab, firePoint.position, Quaternion.identity);

        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (unitCollider != null && bulletCollider != null)
            Physics.IgnoreCollision(unitCollider, bulletCollider, true);

        BulletController_SlingBoom bulletCtrl = bullet.GetComponent<BulletController_SlingBoom>();
        if (bulletCtrl != null) bulletCtrl.SetOwner(this);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
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

    protected void HandleTripleShot(Vector3 mainVelocity)
    {
        List<GameObject> bullets = new List<GameObject>();

        bullets.Add(SpawnBullet(normalBulletPrefab, mainVelocity));
        bullets.Add(SpawnBullet(normalBulletPrefab, Quaternion.Euler(0, 0, 10f) * mainVelocity));
        bullets.Add(SpawnBullet(normalBulletPrefab, Quaternion.Euler(0, 0, -10f) * mainVelocity));

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

    public abstract void Shoot(Vector3 velocity, BulletType bulletType);

    // ✅ DEBUG: Hiển thị Z position trong Scene View
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector3.forward * 2f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"{unitName}\n" +
            $"Z Pos: {transform.position.z:F3}\n" +
            $"Initial Z: {initialZPosition:F3}\n" +
            $"Diff: {(transform.position.z - initialZPosition):F4}");
#endif
    }
}
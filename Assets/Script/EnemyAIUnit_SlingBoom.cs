using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class EnemyAIUnit_SlingBoom : GameUnit_SlingBoom
{
    [Header("AI Settings")]
    [SerializeField] private float thinkTime = 1.0f;
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float accuracyOffset = 1.0f;
    [SerializeField] private float endTurnDelay = 3f;

    [Header("AI Visuals")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private int trajectoryPoints = 40;
    [SerializeField] private float timeBetweenPoints = 0.05f;

    [Header("Enemy Flip Settings - XOAY ENEMY KHI BẮN")]
    [SerializeField] private float flipDuration = 0.3f; // ✅ Thời gian xoay mượt
    [SerializeField] private Ease flipEase = Ease.OutQuad; // ✅ Kiểu animation

    [Header("Smart AI - Tránh bắn đồng đội")]
    [SerializeField] private bool avoidFriendlyFire = true; // ✅ Bật/tắt tính năng
    [SerializeField] private float friendlyFireCheckRadius = 2f; // ✅ Bán kính kiểm tra xung quanh quỹ đạo
    [SerializeField] private int maxRetries = 5; // ✅ Số lần thử lại nếu bắn trúng đồng đội

    private int enemyTurnsPlayed = 0;

    protected override void Awake()
    {
        base.Awake();
        team = TeamType.Enemy;

        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    public override void OnTurnStarted()
    {
        base.OnTurnStarted();

        enemyTurnsPlayed++;
        maxEnergy = baseMaxEnergy + enemyTurnsPlayed;
        currentEnergy = maxEnergy;

        UIGameplay_SlingBoom uiGameplay = GetUIGameplay();
        if (uiGameplay != null)
        {
            uiGameplay.UpdateEnergy(currentEnergy, maxEnergy);
        }

        StartCoroutine(AITurnRoutine());
    }

    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(thinkTime);

        PlayerUnit_SlingBoom target = FindNearestAlivePlayer();

        if (target != null)
        {
            // ✅ LOGIC MỚI: Tìm shot an toàn (không bắn trúng đồng đội)
            ShootData bestShot = FindSafeShot(target);

            if (bestShot != null)
            {
               

                // ✅ XOAY ENEMY THEO HƯỚNG BẮN (NGƯỢC LẠI VỚI PLAYER)
                FlipEnemyBasedOnDirection(bestShot.velocity);

                if (aimLine != null)
                {
                    aimLine.enabled = true;
                    DrawTrajectory(bestShot.velocity);
                    yield return new WaitForSeconds(1.0f);
                    aimLine.enabled = false;
                }

                Shoot(bestShot.velocity, BulletType.Normal);
            }
            else
            {
               

                if (TurnBasedGameManager.Instance != null)
                {
                    TurnBasedGameManager.Instance.EndTurn();
                }
            }
        }
        else
        {
           

            if (TurnBasedGameManager.Instance != null)
            {
                TurnBasedGameManager.Instance.EndTurn();
            }
        }
    }

    // ✅ HÀM MỚI: Xoay Enemy theo hướng bắn (NGƯỢC LẠI VỚI PLAYER)
    private void FlipEnemyBasedOnDirection(Vector3 velocity)
    {
        // Enemy bắn theo velocity.x
        // velocity.x > 0 = bắn SANG PHẢI → Quay mặt sang PHẢI (180°)
        // velocity.x < 0 = bắn SANG TRÁI → Quay mặt sang TRÁI (0°)

        float targetYRotation = 0f;

        if (velocity.x > 0.1f)
        {
            // Bắn sang PHẢI → Quay mặt sang PHẢI (180°)
            targetYRotation = 180f;
        }
        else if (velocity.x < -0.1f)
        {
            // Bắn sang TRÁI → Quay mặt sang TRÁI (0°)
            targetYRotation = 0f;
        }
        else
        {
            // Không đổi hướng nếu bắn thẳng đứng
            return;
        }

        // Chỉ xoay nếu rotation hiện tại khác với target
        if (Mathf.Abs(transform.eulerAngles.y - targetYRotation) > 1f)
        {
            transform.DORotate(new Vector3(0, targetYRotation, 0), flipDuration)
                .SetEase(flipEase);
        }
    }

    // ✅ HÀM MỚI: Tìm shot an toàn (không trúng đồng đội)
    private ShootData FindSafeShot(PlayerUnit_SlingBoom target)
    {
        Vector3 targetPos = target.transform.position;

        // Thử nhiều lần với độ lệch khác nhau
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Thêm độ lệch ngẫu nhiên
            float randomX = Random.Range(-accuracyOffset, accuracyOffset);
            float randomY = Random.Range(-accuracyOffset * 0.5f, accuracyOffset * 0.5f);
            Vector3 aimPos = targetPos + new Vector3(randomX, randomY, 0);

            // Tính toán velocity
            Vector3 velocity = CalculateProjectileVelocity(firePoint.position, aimPos, 1.0f);

            // ✅ KIỂM TRA XEM CÓ TRÚNG ĐỒNG ĐỘI KHÔNG
            if (!avoidFriendlyFire || !WillHitFriendly(velocity))
            {
                return new ShootData { velocity = velocity, targetPosition = aimPos };
            }

            Debug.Log($"[AI {unitName}] Attempt {attempt + 1}: Will hit friendly, retrying...");
        }

        // Nếu không tìm được shot an toàn, thử bắn lên cao (lob shot)
        Debug.Log($"[AI {unitName}] No safe direct shot, trying lob shot...");
        Vector3 lobVelocity = CalculateProjectileVelocity(firePoint.position, targetPos, 1.5f);

        if (!WillHitFriendly(lobVelocity))
        {
            return new ShootData { velocity = lobVelocity, targetPosition = targetPos };
        }

        return null; // Không tìm được shot an toàn
    }

    // ✅ HÀM MỚI: Kiểm tra xem quỹ đạo có trúng đồng đội không
    private bool WillHitFriendly(Vector3 initialVelocity)
    {
        // Lấy danh sách tất cả enemy còn sống (trừ mình)
        var allEnemies = FindObjectsByType<EnemyAIUnit_SlingBoom>(FindObjectsSortMode.None)
            .Where(e => e != null && !e.IsDead && e != this)
            .ToList();

        if (allEnemies.Count == 0)
        {
            return false; // Không có đồng đội nào → không cần lo
        }

        // Tính toán quỹ đạo
        Vector3 startPos = firePoint.position;
        List<Vector3> trajectoryPoints = new List<Vector3>();

        for (int i = 0; i < this.trajectoryPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = startPos + initialVelocity * time + 0.5f * Physics.gravity * time * time;

            if (point.y < 0) break; // Chạm đất

            trajectoryPoints.Add(point);

            // ✅ KIỂM TRA XEM ĐIỂM NÀY CÓ GẦN ĐỒNG ĐỘI KHÔNG
            foreach (var enemy in allEnemies)
            {
                float distToEnemy = Vector3.Distance(point, enemy.transform.position);

                if (distToEnemy < friendlyFireCheckRadius)
                {
                    Debug.Log($"[AI {unitName}] ⚠️ Trajectory will hit {enemy.UnitName} (dist: {distToEnemy:F2}m)");
                    return true; // Quỹ đạo đi qua gần đồng đội
                }
            }
        }

        return false; // Quỹ đạo an toàn
    }

    private PlayerUnit_SlingBoom FindNearestAlivePlayer()
    {
        var allPlayers = FindObjectsByType<PlayerUnit_SlingBoom>(FindObjectsSortMode.None);

        PlayerUnit_SlingBoom nearest = null;
        float minDistance = float.MaxValue;

        foreach (var player in allPlayers)
        {
            if (player == null || player.IsDead || !player.gameObject.activeInHierarchy)
                continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    private Vector3 CalculateProjectileVelocity(Vector3 origin, Vector3 target, float timeToTarget)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float sY = distance.y;
        float sXZ = distanceXZ.magnitude;

        float Vxz = sXZ / timeToTarget;
        float Vy = (sY / timeToTarget) + (0.5f * Mathf.Abs(Physics.gravity.y) * timeToTarget);

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    public override void Shoot(Vector3 velocity, BulletType bulletType)
    {
        SpawnBullet(normalBulletPrefab, velocity);
        StartCoroutine(EndTurnDelay());
    }

    private IEnumerator EndTurnDelay()
    {
        yield return new WaitForSeconds(endTurnDelay);

        if (TurnBasedGameManager.Instance != null)
        {
            TurnBasedGameManager.Instance.EndTurn();
        }
    }

    private void DrawTrajectory(Vector3 initialVelocity)
    {
        if (aimLine == null) return;

        Vector3 startPosition = firePoint.position;
        int validPoints = 0;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = startPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;

            if (point.y < 0) break;

            validPoints++;
        }

        aimLine.positionCount = validPoints;

        for (int i = 0; i < validPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = startPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;
            aimLine.SetPosition(i, point);
        }
    }

    // ✅ CLASS HELPER: Lưu thông tin shot
    private class ShootData
    {
        public Vector3 velocity;
        public Vector3 targetPosition;
    }

    // ✅ DEBUG: Vẽ vùng kiểm tra trong Scene View
    private void OnDrawGizmosSelected()
    {
        if (!avoidFriendlyFire || firePoint == null) return;

        // Vẽ quỹ đạo test
        Gizmos.color = Color.yellow;

        var allEnemies = FindObjectsByType<EnemyAIUnit_SlingBoom>(FindObjectsSortMode.None)
            .Where(e => e != null && !e.IsDead && e != this)
            .ToList();

        foreach (var enemy in allEnemies)
        {
            // Vẽ sphere xung quanh enemy (vùng cấm bắn)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemy.transform.position, friendlyFireCheckRadius);
        }
    }
}
// File: DragToShootController.cs
using UnityEngine;
using DG.Tweening;

public class DragToShootController_SlingBoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUnit_SlingBoom playerUnit;
    [SerializeField] private Transform playerTransform;

    [Header("Drag Settings")]
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float forceMultiplier = 15f;

    [Header("Visuals")]
    [SerializeField] private LineRenderer dragLine;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 40;
    [SerializeField] private float timeBetweenPoints = 0.05f;

    [Header("Player Flip Settings - XOAY PLAYER KHI BẮN")]
    [SerializeField] private float flipDuration = 0.3f;
    [SerializeField] private Ease flipEase = Ease.OutQuad;

    // ✅ LƯU Z POSITION
    private float playerInitialZ = 0f;

    private bool isShootModeEnabled = false;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        if (playerUnit == null) playerUnit = GetComponent<PlayerUnit_SlingBoom>();
        if (playerTransform == null && playerUnit != null) playerTransform = playerUnit.transform;

        // ✅ LƯU Z POSITION BAN ĐẦU
        if (playerTransform != null)
        {
            playerInitialZ = playerTransform.position.z;
        }

        InitializeLineRenderers();
        DisableShootMode();
    }

    private void InitializeLineRenderers()
    {
        if (dragLine != null)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }

        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }

        Debug.Log("[DragToShootController] LineRenderers initialized");
    }

    public void EnableShootMode()
    {
        isShootModeEnabled = true;
    }

    public void DisableShootMode()
    {
        isShootModeEnabled = false;
        isDragging = false;

        if (dragLine)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }
        if (trajectoryLine)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }
    }

    private void Update()
    {
        if (!isShootModeEnabled || playerUnit == null || !playerUnit.IsMyTurn) return;

        // ✅ LUÔN ĐẢM BẢO Z POSITION CỐ ĐỊNH
        ForceZPosition();

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = GetMouseWorldPosition();

            if (dragLine)
            {
                dragLine.positionCount = 0;
                dragLine.enabled = true;
            }
            if (trajectoryLine)
            {
                trajectoryLine.positionCount = 0;
                trajectoryLine.enabled = true;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDragging();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            FinishDragging();
        }
    }

    // ✅ HÀM MỚI: Luôn giữ Z position cố định
    private void ForceZPosition()
    {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;
        if (Mathf.Abs(pos.z - playerInitialZ) > 0.001f)
        {
            pos.z = playerInitialZ;
            playerTransform.position = pos;
        }
    }

    private void UpdateDragging()
    {
        Vector3 currentMousePos = GetMouseWorldPosition();
        Vector3 dragVector = dragStartPos - currentMousePos;

        if (dragVector.magnitude > maxDragDistance)
            dragVector = dragVector.normalized * maxDragDistance;

        // ✅ XOAY PLAYER THEO HƯỚNG BẮN
        FlipPlayerBasedOnDirection(dragVector);

        // ✅ ĐẢM BẢO Z POSITION SAU KHI XOAY
        ForceZPosition();

        // ✅ Vẽ DÂY KÉO
        if (dragLine != null)
        {
            try
            {
                dragLine.positionCount = 2;
                dragLine.SetPosition(0, dragStartPos);
                dragLine.SetPosition(1, dragStartPos - dragVector);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DragToShootController] Error drawing drag line: {e.Message}");
                dragLine.positionCount = 0;
            }
        }

        // ✅ Vẽ QUỸ ĐẠO
        Vector3 calculatedVelocity = CalculateVelocity(dragVector);
        DrawTrajectory(calculatedVelocity);
    }

    private void FinishDragging()
    {
        isDragging = false;

        if (dragLine)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }
        if (trajectoryLine)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }

        Vector3 currentMousePos = GetMouseWorldPosition();
        Vector3 dragVector = dragStartPos - currentMousePos;

        if (dragVector.magnitude > maxDragDistance)
            dragVector = dragVector.normalized * maxDragDistance;

        if (dragVector.magnitude < 0.2f) return;

        // ✅ RESET ROTATION VÀ ĐẢM BẢO Z POSITION TRƯỚC KHI BẮN
        ResetPlayerRotationBeforeShoot();

        Vector3 finalVelocity = CalculateVelocity(dragVector);

        playerUnit.Shoot(finalVelocity, playerUnit.SelectedBulletType);

        DisableShootMode();
    }

    // ✅ SỬA LẠI: Reset rotation và đảm bảo Z position
    private void ResetPlayerRotationBeforeShoot()
    {
        if (playerTransform == null) return;

        // ✅ LƯU Z POSITION TRƯỚC KHI RESET
        float savedZ = playerTransform.position.z;

        // Reset rotation về 0 (hướng mặc định) trước khi bắn
        playerTransform.rotation = Quaternion.identity;

        // ✅ KHÔI PHỤC Z POSITION
        Vector3 pos = playerTransform.position;
        pos.z = savedZ;
        playerTransform.position = pos;

        Debug.Log($"[DragToShootController] ✅ Player rotation reset, Z locked at {savedZ:F3}");
    }

    // ✅ SỬA LẠI: Xoay Player và luôn giữ Z position
    private void FlipPlayerBasedOnDirection(Vector3 dragVector)
    {
        if (playerTransform == null) return;

        float targetYRotation = 0f;

        if (dragVector.x > 0.1f)
        {
            targetYRotation = 0f; // Bắn sang TRÁI → Quay mặt sang TRÁI
        }
        else if (dragVector.x < -0.1f)
        {
            targetYRotation = 180f; // Bắn sang PHẢI → Quay mặt sang PHẢI
        }
        else
        {
            return;
        }

        if (Mathf.Abs(playerTransform.eulerAngles.y - targetYRotation) > 1f)
        {
            // ✅ LƯU Z POSITION TRƯỚC KHI XOAY
            float savedZ = playerTransform.position.z;

            playerTransform.DORotate(new Vector3(0, targetYRotation, 0), flipDuration)
                .SetEase(flipEase)
                .OnUpdate(() =>
                {
                    // ✅ LUÔN GIỮ Z POSITION TRONG QUÁ TRÌNH XOAY
                    Vector3 pos = playerTransform.position;
                    pos.z = savedZ;
                    playerTransform.position = pos;
                })
                .OnComplete(() =>
                {
                    // ✅ ĐẢM BẢO Z POSITION SAU KHI XOAY XONG
                    Vector3 pos = playerTransform.position;
                    pos.z = savedZ;
                    playerTransform.position = pos;
                });
        }
    }

    private Vector3 CalculateVelocity(Vector3 dragVector)
    {
        return new Vector3(dragVector.x * forceMultiplier, dragVector.y * forceMultiplier, 0);
    }

    private void DrawTrajectory(Vector3 initialVelocity)
    {
        if (trajectoryLine == null) return;

        Vector3 startPosition = playerTransform.position;
        int validPoints = 0;

        trajectoryLine.positionCount = 0;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = startPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;

            if (point.y < 0)
            {
                break;
            }

            validPoints++;
        }

        if (validPoints > 0)
        {
            trajectoryLine.positionCount = validPoints;

            for (int i = 0; i < validPoints; i++)
            {
                float time = i * timeBetweenPoints;
                Vector3 point = startPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;

                try
                {
                    trajectoryLine.SetPosition(i, point);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DragToShootController] Error setting trajectory position {i}: {e.Message}");
                    break;
                }
            }
        }
        else
        {
            trajectoryLine.positionCount = 0;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(mousePos);
    }

    private void OnDestroy()
    {
        if (dragLine != null)
        {
            dragLine.positionCount = 0;
        }
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0;
        }
    }
}
// File: CameraController.cs
using UnityEngine;
using DG.Tweening;

public class CameraController_SlingBoom : MonoBehaviour
{
    public static CameraController_SlingBoom Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private Transform overviewPosition;
    [SerializeField] private float overviewDuration = 3f;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float unitFocusDistance = 10f;
    [SerializeField] private float unitFocusHeight = 5f;

    [Header("Unit Reset Settings")]
    [SerializeField] private float unitResetDuration = 0.5f;

    private Camera mainCamera;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isOverviewMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;

        if (overviewPosition == null)
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
        else
        {
            initialPosition = overviewPosition.position;
            initialRotation = overviewPosition.rotation;
        }
    }

    // ✅ SỬA LẠI: Reset rotation CHỈ TRÊN TRỤC Y (không động đến Z position)
    private void ResetAllUnitRotations()
    {
        GameUnit_SlingBoom[] allUnits = FindObjectsByType<GameUnit_SlingBoom>(FindObjectsSortMode.None);

        foreach (GameUnit_SlingBoom unit in allUnits)
        {
            if (unit != null && !unit.IsDead)
            {
                // ✅ LƯU Z POSITION TRƯỚC KHI RESET
                Vector3 currentPos = unit.transform.position;
                float savedZ = currentPos.z;

                // Reset rotation về 0 với DOTween để mượt mà
                unit.transform.DORotate(Vector3.zero, unitResetDuration)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() =>
                    {
                        // ✅ LUÔN GIỮ Z POSITION CỐ ĐỊNH TRONG QUÁ TRÌNH ANIMATION
                        Vector3 pos = unit.transform.position;
                        if (Mathf.Abs(pos.z - savedZ) > 0.001f)
                        {
                            pos.z = savedZ;
                            unit.transform.position = pos;
                        }
                    })
                    .OnComplete(() =>
                    {
                        // ✅ ĐẢM BẢO Z POSITION SAU KHI HOÀN THÀNH
                        Vector3 finalPos = unit.transform.position;
                        finalPos.z = savedZ;
                        unit.transform.position = finalPos;
                    });
            }
        }
    }

    public void ShowOverview(System.Action onComplete = null)
    {
        isOverviewMode = true;

        // ✅ RESET ROTATION CỦA TẤT CẢ UNITS KHI CHUYỂN SANG OVERVIEW
        ResetAllUnitRotations();

        transform.DOMove(initialPosition, transitionDuration).SetEase(Ease.InOutSine);
        transform.DORotateQuaternion(initialRotation, transitionDuration).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(overviewDuration, () =>
                {
                    isOverviewMode = false;
                    onComplete?.Invoke();
                });
            });
    }

    public void FocusOnUnit(GameUnit_SlingBoom unit, System.Action onComplete = null)
    {
        if (unit == null || unit.IsDead)
        {
            onComplete?.Invoke();
            return;
        }

        isOverviewMode = false;

        // ✅ RESET ROTATION CỦA TẤT CẢ UNITS KHI FOCUS VÀO 1 UNIT
        ResetAllUnitRotations();

        // Tính toán vị trí camera - XEM NGANG (side view)
        Vector3 targetPos = unit.transform.position;

        // Camera ở phía sau, nhìn ngang vào unit
        Vector3 cameraOffset = new Vector3(0, unitFocusHeight, -unitFocusDistance);
        Vector3 newCameraPos = targetPos + cameraOffset;

        // Di chuyển camera
        transform.DOMove(newCameraPos, transitionDuration).SetEase(Ease.InOutSine);

        // Xoay camera nhìn ngang về phía unit
        Vector3 lookDirection = targetPos - newCameraPos;
        lookDirection.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        transform.DORotateQuaternion(targetRotation, transitionDuration).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }

    public bool IsInOverviewMode()
    {
        return isOverviewMode;
    }
}
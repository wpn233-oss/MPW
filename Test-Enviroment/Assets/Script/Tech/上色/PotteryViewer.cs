using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PotteryViewer : MonoBehaviour
{
    [Header("旋转设置")]
    public float rotationSensitivity = 2f;    // 旋转灵敏度
    public float dragRotationTime = 0.1f;     // 拖拽旋转的过渡时间

    [Header("滑动设置")]
    public float friction = 0.95f;            // 摩擦系数，值越小停止越快
    public float minVelocity = 0.5f;          // 最小速度阈值，低于此值停止滑动

    [Header("上色阶段检测")]
    public Camera coloringCamera;

    private bool isDragging = false;
    private Vector3 lastMousePos;
    private Quaternion initialRotation;
    private float currentRotationY = 0f;
    private Tween dragTween;

    // 惯性滑动相关
    private float angularVelocity = 0f;
    private bool isSliding = false;
    private Vector3[] mousePosHistory = new Vector3[3]; // 记录鼠标位置历史
    private int historyIndex = 0;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (coloringCamera != null && !coloringCamera.gameObject.activeInHierarchy)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        HandleInput();

        // 如果没有拖拽但有速度，应用惯性滑动
        if (!isDragging && isSliding)
        {
            ApplyInertia();
        }
    }

    void HandleInput()
    {
        // 更准确地检测鼠标状态
        if (Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging) // 确保只有在拖拽状态时才停止
            {
                StopDragging();
            }
        }

        if (isDragging && Input.GetMouseButton(0)) // 双重确认鼠标左键按下
        {
            UpdateDragging();
        }
        else if (isDragging) // 如果处于拖拽状态但鼠标左键没有按下，停止拖拽
        {
            StopDragging();
        }
    }

    void StartDragging()
    {
        isDragging = true;
        lastMousePos = Input.mousePosition;

        // 停止任何正在进行的旋转动画
        if (dragTween != null && dragTween.IsActive())
        {
            dragTween.Kill();
        }

        // 重置滑动状态
        isSliding = false;
        angularVelocity = 0f;

        // 初始化鼠标位置历史
        for (int i = 0; i < mousePosHistory.Length; i++)
        {
            mousePosHistory[i] = Input.mousePosition;
        }
        historyIndex = 0;
    }

    void StopDragging()
    {
        isDragging = false;

        // 计算最终角速度
        CalculateAngularVelocity();

        // 如果速度足够大，开始惯性滑动
        if (Mathf.Abs(angularVelocity) > minVelocity)
        {
            isSliding = true;
        }
    }

    void UpdateDragging()
    {
        Vector3 currentMousePos = Input.mousePosition;

        // 记录鼠标位置历史
        mousePosHistory[historyIndex] = currentMousePos;
        historyIndex = (historyIndex + 1) % mousePosHistory.Length;

        Vector3 delta = currentMousePos - lastMousePos;
        lastMousePos = currentMousePos;

        // 计算旋转量：向右拖拽逆时针，向左拖拽顺时针
        float rotationDelta = -delta.x * rotationSensitivity;

        // 更新目标旋转角度
        float targetRotationY = currentRotationY + rotationDelta;

        // 使用 DOTween 平滑旋转到目标角度
        if (dragTween != null && dragTween.IsActive())
        {
            dragTween.Kill();
        }

        dragTween = DOTween.To(
            () => currentRotationY,
            y => {
                currentRotationY = y;
                ApplyRotation(y);
            },
            targetRotationY,
            dragRotationTime
        ).SetEase(Ease.OutQuad);
    }

    void CalculateAngularVelocity()
    {
        // 计算最近几帧的平均速度
        Vector3 totalDelta = Vector3.zero;
        int count = 0;

        for (int i = 0; i < mousePosHistory.Length - 1; i++)
        {
            int currentIndex = (historyIndex + i) % mousePosHistory.Length;
            int nextIndex = (historyIndex + i + 1) % mousePosHistory.Length;

            totalDelta += mousePosHistory[nextIndex] - mousePosHistory[currentIndex];
            count++;
        }

        if (count > 0)
        {
            Vector3 averageDelta = totalDelta / count;
            angularVelocity = -averageDelta.x * rotationSensitivity * 0.5f; // 乘以一个系数调整惯性强度
        }
    }

    void ApplyInertia()
    {
        // 应用角速度
        currentRotationY += angularVelocity * Time.deltaTime;
        ApplyRotation(currentRotationY);

        // 应用摩擦力减速
        angularVelocity *= friction;

        // 如果速度很小，停止滑动
        if (Mathf.Abs(angularVelocity) < minVelocity)
        {
            angularVelocity = 0f;
            isSliding = false;
        }
    }

    void ApplyRotation(float rotationY)
    {
        transform.rotation = initialRotation * Quaternion.Euler(0, rotationY, 0);
    }

    void OnDestroy()
    {
        // 清理 DOTween
        if (dragTween != null && dragTween.IsActive())
        {
            dragTween.Kill();
        }
    }

    // 可选：如果需要手动重置旋转，可以调用这个方法
    public void ResetRotation(float duration = 0.5f)
    {
        isDragging = false;
        isSliding = false;
        angularVelocity = 0f;

        if (dragTween != null && dragTween.IsActive())
        {
            dragTween.Kill();
        }

        dragTween = DOTween.To(
            () => currentRotationY,
            y => {
                currentRotationY = y;
                ApplyRotation(y);
            },
            0f,
            duration
        ).SetEase(Ease.OutCubic);
    }
}
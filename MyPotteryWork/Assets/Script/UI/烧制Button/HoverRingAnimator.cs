using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class HoverRingAnimator : MonoBehaviour
{
    [Header("按钮本体（用于检测区域）")]
    public RectTransform buttonRect; // 自动获取自身

    [Header("圆环图像（视觉特效）")]
    public Image ringImage;

    [Header("圆环动画参数")]
    public float minScale = 0.8f;
    public float maxScale = 2.2f;
    public float expandSpeed = 1.5f;
    public bool loopWhileHover = true;

    [Header("要缩放和旋转的按钮元素")]
    public List<RectTransform> hoverElements = new List<RectTransform>();

    [Header("要旋转的按钮元素（可选，为空则使用hoverElements）")]
    public List<RectTransform> rotateElements = new List<RectTransform>();

    [Header("缩放参数")]
    public float elementScaleMultiplier = 1.1f;
    public float elementScaleSpeed = 6f;

    [Header("缩放动画参数")]
    public float shrinkScale = 0.9f;      // 先收缩比例
    public float targetScale = 1.1f;      // 再放大比例
    public float shrinkDuration = 0.15f;  // 收缩时间
    public float expandDuration = 0.35f;  // 放大时间

    [Header("旋转动画参数")]
    public float rotateAngle = 720f;      // 初始旋转角度（默认2圈）
    public float rotateDuration = 0.8f;   // 初始旋转时间
    public float continuousRotateSpeed = 90f; // 持续旋转速度（度/秒）

    private Button button;
    private Canvas parentCanvas;
    private Camera uiCamera;

    private bool isHovering = false;
    private bool finishCurrentLoop = false;
    private float animProgress = 0f;

    private CanvasGroup ringCg;
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    private Dictionary<RectTransform, Quaternion> originalRotations = new Dictionary<RectTransform, Quaternion>();
    private Dictionary<RectTransform, Tween> activeTweens = new Dictionary<RectTransform, Tween>();
    private Dictionary<RectTransform, Tween> activeRotateTweens = new Dictionary<RectTransform, Tween>();
    private Dictionary<RectTransform, Tween> continuousRotateTweens = new Dictionary<RectTransform, Tween>();

    void Awake()
    {
        button = GetComponent<Button>();
        if (buttonRect == null) buttonRect = GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCamera = parentCanvas.worldCamera;
        else
            uiCamera = null;

        // 自动查找 ring
        if (ringImage == null)
        {
            Image[] imgs = GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                if (img == null || img.gameObject == gameObject) continue;
                if (img.name.ToLower().Contains("ring"))
                {
                    ringImage = img;
                    break;
                }
            }
        }

        if (ringImage != null)
        {
            ringCg = ringImage.GetComponent<CanvasGroup>();
            if (ringCg == null) ringCg = ringImage.gameObject.AddComponent<CanvasGroup>();
            ringImage.transform.localScale = Vector3.one * minScale;
            ringCg.alpha = 0f;
        }

        // 记录原始缩放值
        foreach (var r in hoverElements)
        {
            if (r != null && !originalScales.ContainsKey(r))
                originalScales[r] = r.localScale;
        }

        // 记录原始旋转值（为所有可能旋转的元素）
        var allRotateElements = rotateElements.Count > 0 ? rotateElements : hoverElements;
        foreach (var r in allRotateElements)
        {
            if (r != null && !originalRotations.ContainsKey(r))
                originalRotations[r] = r.localRotation;
        }
    }

    void Update()
    {
        if (buttonRect == null || button == null) return;

        // 按钮不可点击时重置
        if (!button.interactable)
        {
            if (isHovering || finishCurrentLoop)
            {
                isHovering = false;
                finishCurrentLoop = false;
                animProgress = 0f;
                ResetVisuals();
            }
            return;
        }

        // 判断鼠标是否在按钮上
        bool pointerOverButton = RectTransformUtility.RectangleContainsScreenPoint(buttonRect, Input.mousePosition, uiCamera);

        if (pointerOverButton && !isHovering && !finishCurrentLoop)
        {
            StartHover();
        }
        else if (!pointerOverButton && isHovering)
        {
            StopHoverDeferred();
        }

        if (!isHovering && !finishCurrentLoop) return;

        animProgress += Time.deltaTime * expandSpeed;
        float t = Mathf.Clamp01(animProgress % 1f);

        // 圆环扩散
        if (ringImage != null && ringCg != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, t);
            ringImage.transform.localScale = Vector3.one * scale;
            ringCg.alpha = Mathf.Lerp(1f, 0f, t);
        }

        // 循环控制
        if (animProgress >= 1f)
        {
            animProgress = 0f;

            if (isHovering && loopWhileHover)
                return;
            else if (finishCurrentLoop)
            {
                finishCurrentLoop = false;
                isHovering = false;
                ResetVisuals();
            }
        }
    }

    private void StartHover()
    {
        isHovering = true;
        finishCurrentLoop = false;
        animProgress = 0f;

        if (ringCg != null) ringCg.alpha = 1f;
        if (ringImage != null) ringImage.transform.localScale = Vector3.one * minScale;

        // 对每个元素执行缩放动画（保持原有逻辑）
        foreach (var rect in hoverElements)
        {
            if (rect == null) continue;

            if (activeTweens.ContainsKey(rect) && activeTweens[rect].IsActive())
                activeTweens[rect].Kill();

            Vector3 originalScale = originalScales.ContainsKey(rect)
                ? originalScales[rect]
                : rect.localScale;

            // Step 1: 收缩
            Tween scaleTween = rect.DOScale(originalScale * shrinkScale, shrinkDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                // Step 2: 放大
                rect.DOScale(originalScale * targetScale, expandDuration)
                        .SetEase(Ease.OutBack);
                });

            activeTweens[rect] = scaleTween;
        }

        // 对需要旋转的元素执行旋转动画
        var elementsToRotate = rotateElements.Count > 0 ? rotateElements : hoverElements;
        foreach (var rect in elementsToRotate)
        {
            if (rect == null) continue;

            // 停止之前的持续旋转动画
            if (continuousRotateTweens.ContainsKey(rect) && continuousRotateTweens[rect].IsActive())
                continuousRotateTweens[rect].Kill();

            // 停止之前的初始旋转动画
            if (activeRotateTweens.ContainsKey(rect) && activeRotateTweens[rect].IsActive())
                activeRotateTweens[rect].Kill();

            // 获取当前旋转角度
            float currentRotation = rect.localEulerAngles.z;

            // 计算剩余需要旋转的角度（从当前位置到目标角度）
            float remainingAngle = rotateAngle + currentRotation;

            // 根据剩余角度和旋转速度计算持续时间
            float remainingDuration = Mathf.Abs(remainingAngle) / (rotateAngle / rotateDuration);

            // 第一步：从当前位置继续旋转到目标角度
            Vector3 targetRotation = new Vector3(0, 0, currentRotation - remainingAngle);
            Tween initialRotateTween = rect.DOLocalRotate(targetRotation, remainingDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                // 第二步：开始持续缓慢旋转（从当前位置继续）
                if (isHovering && rect != null)
                    {
                        Tween continuousTween = rect.DOLocalRotate(new Vector3(0, 0, -360f), 360f / continuousRotateSpeed, RotateMode.LocalAxisAdd)
                            .SetEase(Ease.Linear)
                            .SetLoops(-1);

                        continuousRotateTweens[rect] = continuousTween;
                    }
                });

            activeRotateTweens[rect] = initialRotateTween;
        }
    }


    private void StopHoverDeferred()
    {
        if (isHovering)
        {
            isHovering = false;
            finishCurrentLoop = true;
        }
    }

    private void ResetVisuals()
    {
        if (ringImage != null && ringCg != null)
        {
            ringImage.transform.localScale = Vector3.one * minScale;
            ringCg.alpha = 0f;
        }

        // 重置缩放元素（保持原有逻辑）
        foreach (var rect in hoverElements)
        {
            if (rect == null) continue;

            if (activeTweens.ContainsKey(rect))
            {
                activeTweens[rect].Kill();
                activeTweens.Remove(rect);
            }

            if (originalScales.ContainsKey(rect))
                rect.localScale = originalScales[rect];
        }

        // 重置旋转元素
        var elementsToResetRotation = rotateElements.Count > 0 ? rotateElements : hoverElements;
        foreach (var rect in elementsToResetRotation)
        {
            if (rect == null) continue;

            if (activeRotateTweens.ContainsKey(rect))
            {
                activeRotateTweens[rect].Kill();
                activeRotateTweens.Remove(rect);
            }

            if (continuousRotateTweens.ContainsKey(rect))
            {
                continuousRotateTweens[rect].Kill();
                continuousRotateTweens.Remove(rect);
            }

            if (originalRotations.ContainsKey(rect))
                rect.localRotation = originalRotations[rect];
            else
                rect.localRotation = Quaternion.identity;
        }
    }

    private void OnDisable()
    {
        ResetVisuals();
    }

    private void OnDestroy()
    {
        // 清理所有动画
        foreach (var tween in activeTweens.Values)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }

        foreach (var tween in activeRotateTweens.Values)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }

        foreach (var tween in continuousRotateTweens.Values)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
    }
}
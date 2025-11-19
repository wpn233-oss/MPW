using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIRotator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Tween rotateTween;
    private Tween scaleTween;

    [Header("旋转设置")]
    public float rotateAngle = 720f;         // 旋转两圈
    public float rotateDuration = 1.5f;      // 动画时长

    [Header("缩放设置")]
    public float shrinkScale = 0.9f;         // 先缩小到多少倍
    public float targetScale = 1.1f;         // 最后放大到多少倍
    public float shrinkDuration = 0.15f;     // 缩小时长
    public float expandDuration = 0.4f;      // 放大时长

    private Vector3 originalScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 停止上一次动画
        rotateTween?.Kill();
        scaleTween?.Kill();

        // Step 1: 先缩小
        scaleTween = rectTransform.DOScale(originalScale * shrinkScale, shrinkDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                // Step 2: 再放大并旋转两圈（使用局部旋转）
                rectTransform.DOScale(originalScale * targetScale, expandDuration)
                    .SetEase(Ease.OutBack);
                rotateTween = rectTransform
                    .DOLocalRotate(new Vector3(0, 0, -rotateAngle), rotateDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic);
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标移出时恢复
        rotateTween?.Kill();
        scaleTween?.Kill();

        rectTransform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
        rectTransform.DOScale(originalScale, 0.4f).SetEase(Ease.OutQuad);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class UIButtonHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 组件")]
    public Image targetImage;       // 按钮背景图
    public TMP_Text tmpText;        // 按钮文字（TMP）

    [Header("颜色设置")]
    public Color normalImageColor = Color.white;
    public Color hoverImageColor = new Color(1f, 0.9f, 0.7f, 1f);
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = new Color(1f, 0.85f, 0.5f, 1f);

    [Header("动画时长")]
    public float tweenDuration = 0.25f;

    private Tween imageTween;
    private Tween textTween;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
        if (tmpText == null)
            tmpText = GetComponentInChildren<TMP_Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 停止旧动画
        imageTween?.Kill();
        textTween?.Kill();

        // 背景颜色变化
        if (targetImage != null)
            imageTween = targetImage.DOColor(hoverImageColor, tweenDuration).SetEase(Ease.OutQuad);

        // TMP文字颜色变化
        if (tmpText != null)
            textTween = tmpText.DOColor(hoverTextColor, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageTween?.Kill();
        textTween?.Kill();

        if (targetImage != null)
            imageTween = targetImage.DOColor(normalImageColor, tweenDuration).SetEase(Ease.OutQuad);

        if (tmpText != null)
            textTween = tmpText.DOColor(normalTextColor, tweenDuration).SetEase(Ease.OutQuad);
    }
}

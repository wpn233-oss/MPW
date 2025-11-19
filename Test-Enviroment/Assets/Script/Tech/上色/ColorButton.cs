using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ColorButton : MonoBehaviour
{
    public ColoringPainter painter;
    private Button btn;
    private Image img;
    private RectTransform rectTransform;

    [Header("选中状态设置")]
    public float selectedScale = 1.2f;
    public float animationDuration = 0.3f;
    public Ease scaleEase = Ease.OutBack;

    private Vector3 originalScale;
    private bool isSelected = false;
    private Tween scaleTween;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // 保存原始尺寸
        originalScale = rectTransform.localScale;

        btn.onClick.AddListener(OnClickColor);
    }

    void OnClickColor()
    {
        if (painter != null && img != null)
        {
            Debug.Log($"🎨 ColorButton 被点击: {img.color}");
            painter.SetBrushColor(img.color);

            // 方法1：尝试通过父对象找到管理器
            ColorButtonManager manager = GetComponentInParent<ColorButtonManager>();
            if (manager != null)
            {
                manager.OnColorButtonSelected(this);
            }
            else
            {
                // 方法2：尝试在场景中查找管理器
                manager = FindObjectOfType<ColorButtonManager>();
                if (manager != null)
                {
                    manager.OnColorButtonSelected(this);
                }
                else
                {
                    // 方法3：如果没有管理器，直接设置选中状态
                    Debug.LogWarning("未找到ColorButtonManager，使用直接设置方式");
                    SetSelectedState(true);
                }
            }
        }
        else
        {
            Debug.LogError($"❌ ColorButton 错误: painter={(painter == null ? "未设置" : "已设置")}, img={(img == null ? "未找到" : "已找到")}");
        }
    }

    // 设置选中状态
    public void SetSelectedState(bool selected)
    {
        if (isSelected == selected) return;

        isSelected = selected;

        // 杀死之前的动画
        scaleTween?.Kill();

        if (selected)
        {
            // 选中状态：放大
            scaleTween = rectTransform.DOScale(originalScale * selectedScale, animationDuration)
                .SetEase(scaleEase);
        }
        else
        {
            // 取消选中：恢复原始大小
            scaleTween = rectTransform.DOScale(originalScale, animationDuration)
                .SetEase(Ease.InBack);
        }
    }

    void OnDestroy()
    {
        // 清理 DOTween
        scaleTween?.Kill();
    }
}
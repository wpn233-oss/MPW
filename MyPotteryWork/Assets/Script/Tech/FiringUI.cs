using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FiringUI : MonoBehaviour
{
    [Header("核心UI")]
    public Slider 控制条;
    public RectTransform 有效区域;
    public Image 指针;

    [Header("文本显示")]
    public TMP_Text 剩余碰撞次数文本;
    public TMP_Text 倒计时文本;
    public TMP_Text 状态文本;

    [Header("样式设置")]
    public Color 有效区域颜色 = new Color(0, 1, 0, 0.3f);
    public Color 指针有效颜色 = Color.green;
    public Color 指针无效颜色 = Color.red;

    void Start()
    {
        // 初始化有效区域颜色
        if (有效区域 != null)
        {
            Image validZoneImage = 有效区域.GetComponent<Image>();
            if (validZoneImage != null)
            {
                validZoneImage.color = 有效区域颜色;
            }
        }

        // 初始化指针颜色
        if (指针 != null)
        {
            指针.color = 指针无效颜色;
        }

        // 初始化文本
        if (状态文本 != null)
        {
            状态文本.text = "";
        }
    }

    public void UpdatePointerPosition(float normalizedPosition)
    {
        if (控制条 == null || 指针 == null) return;

        RectTransform sliderRect = 控制条.GetComponent<RectTransform>();
        float sliderWidth = sliderRect.rect.width;
        float pointerPos = normalizedPosition * sliderWidth;
        指针.rectTransform.anchoredPosition = new Vector2(pointerPos, 0);
    }

    public void UpdatePointerColor(bool isValid)
    {
        if (指针 == null) return;
        指针.color = isValid ? 指针有效颜色 : 指针无效颜色;
    }
}
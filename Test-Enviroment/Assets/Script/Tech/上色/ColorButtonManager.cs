using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class ColorButtonManager : MonoBehaviour
{
    [Header("动画设置")]
    public float switchDelay = 0.1f;

    [Header("笔刷控制器")]
    public ColoringPainter painter;

    [Header("音效设置")]
    public AudioClip colorSelectClip; // 🎵 点击颜色时的音效

    private List<ColorButton> colorButtons = new List<ColorButton>();
    private ColorButton currentlySelectedButton;
    private Sequence switchSequence;

    void Start()
    {
        // 获取所有子对象的 ColorButton 组件
        colorButtons = new List<ColorButton>(GetComponentsInChildren<ColorButton>(true));
        Debug.Log($"找到 {colorButtons.Count} 个颜色按钮");

        // 为每个按钮注册事件
        foreach (var button in colorButtons)
        {
            Button uiButton = button.GetComponent<Button>();
            Image buttonImage = button.GetComponent<Image>();

            if (uiButton != null && buttonImage != null)
            {
                uiButton.onClick.AddListener(() =>
                {
                    // 切换选中状态
                    OnColorButtonSelected(button);

                    // 设置画笔颜色
                    if (painter != null)
                        painter.SetBrushColor(buttonImage.color);

                    // 🎵 播放颜色点击音效
                    PlayColorSelectSound();
                });
            }
        }
    }

    private void PlayColorSelectSound()
    {
        if (colorSelectClip == null) return;

        // ✅ 优先走 ColoringAudioManager 的全局音源（受 AudioManager 控制）
        if (ColoringAudioManager.Instance != null && ColoringAudioManager.Instance.sfxSource != null)
        {
            var src = ColoringAudioManager.Instance.sfxSource;
            src.PlayOneShot(colorSelectClip, src.volume);
        }
        else
        {
            // ⚠️ 如果上色音频系统还没初始化，做一个兜底
            AudioSource.PlayClipAtPoint(colorSelectClip, Camera.main.transform.position, 0.5f);
        }
    }

    public void OnColorButtonSelected(ColorButton selectedButton)
    {
        if (switchSequence != null && switchSequence.IsActive())
            switchSequence.Kill();

        switchSequence = DOTween.Sequence();

        if (currentlySelectedButton != null && currentlySelectedButton != selectedButton)
        {
            switchSequence.AppendCallback(() => currentlySelectedButton.SetSelectedState(false));
            switchSequence.AppendInterval(switchDelay);
        }

        switchSequence.AppendCallback(() =>
        {
            selectedButton.SetSelectedState(true);
            currentlySelectedButton = selectedButton;
        });
    }

    public void SelectButtonByColor(Color color)
    {
        foreach (ColorButton button in colorButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && ApproximatelyEqualColor(buttonImage.color, color))
            {
                OnColorButtonSelected(button);
                break;
            }
        }
    }

    private bool ApproximatelyEqualColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }

    void OnDestroy()
    {
        switchSequence?.Kill();
    }
}

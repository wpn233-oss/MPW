using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("动画设置")]
    public Animator buttonAnimator;
    public string hoverTrigger = "Hover";
    public string normalTrigger = "Normal";

    [Header("音效设置")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("基础设置")]
    public bool enableHover = true;
    public bool enableClickSound = true;

    private void Start()
    {
        // 自动获取组件
        if (buttonAnimator == null)
            buttonAnimator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // 鼠标悬停
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHover) return;

        // 播放悬停动画
        if (buttonAnimator != null)
            buttonAnimator.SetTrigger(hoverTrigger);

        // 播放悬停音效
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);
    }

    // 鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enableHover) return;

        // 恢复正常状态
        if (buttonAnimator != null)
            buttonAnimator.SetTrigger(normalTrigger);
    }

    // 鼠标点击
    public void OnPointerClick(PointerEventData eventData)
    {
        // 播放点击音效
        if (enableClickSound && clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);
    }
}
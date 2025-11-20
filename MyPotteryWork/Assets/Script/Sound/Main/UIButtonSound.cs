using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class UIButtonSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 音效设置")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    [Tooltip("悬停音效延迟时间（秒）")]
    public float hoverDelay = 0.25f;

    private bool isInteractable = true;
    private bool isHovering = false;
    private float hoverTimer = 0f;
    private Selectable selectable;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    void Update()
    {
        // 按钮是否可交互
        if (selectable != null)
            isInteractable = selectable.interactable;

        // 如果鼠标正在停留在按钮上
        if (isHovering && isInteractable && hoverSound != null)
        {
            hoverTimer += Time.unscaledDeltaTime; // 用 unscaled 防止暂停菜单影响
            if (hoverTimer >= hoverDelay)
            {
                AudioManager.Instance.PlayUI(hoverSound);
                isHovering = false; // 播放一次后就停止计时
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标进入，开始计时
        if (!isInteractable) return;
        hoverTimer = 0f;
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开，取消悬停播放
        isHovering = false;
        hoverTimer = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || clickSound == null) return;
        AudioManager.Instance.PlayUI(clickSound);
    }
}

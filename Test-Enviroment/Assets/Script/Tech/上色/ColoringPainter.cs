using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ColoringPainter : MonoBehaviour
{
    [Header("笔刷参数")]
    public float brushSize = 8f;
    public float minBrushSize = 1f;
    public float maxBrushSize = 50f;
    public float brushSizeSensitivity = 0.1f;
    public Color brushColor = Color.white;

    [Header("工具按钮")]
    public Button brushButton;
    public Button sprayButton;
    public Button paintBucketButton;

    [Header("工具动画参数")]
    public float hoverScale = 1.1f;
    public float selectedScale = 1.2f;
    public float animationDuration = 0.2f;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.85f);
    public Color selectedColor = new Color(1f, 0.9f, 0.5f, 1f);

    [Header("当前颜色显示")]
    public Image currentColorDisplay;

    [Header("音效设置")]
    public AudioClip brushAdjustClip;          // 调整笔刷大小音效
    public float adjustSoundInterval = 0.15f;  // 限制播放频率
    private float lastAdjustSoundTime;

    private AudioSource sfxSource;             // 全局音源
    private Vector3 originalScale = Vector3.one;
    private bool isAdjustingBrushSize = false;
    private Vector3 lastMousePosition;

    public enum ToolType { Brush, Spray, PaintBucket }
    public ToolType currentTool = ToolType.Brush;

    void Start()
    {
        InitializeToolButtons();
        UpdateToolButtonStates();

        // 自动获取全局上色音源
        if (ColoringAudioManager.Instance != null)
            sfxSource = ColoringAudioManager.Instance.sfxSource;
    }

    void Update()
    {
        HandleBrushSizeAdjustment();
    }

    // ======================== 🎨 右键调整笔刷大小 ========================
    void HandleBrushSizeAdjustment()
    {
        // 按下右键开始调整
        if (Input.GetMouseButtonDown(1))
        {
            isAdjustingBrushSize = true;
            lastMousePosition = Input.mousePosition;
            Cursor.visible = false;
        }

        // 按住右键拖拽调整
        if (isAdjustingBrushSize && Input.GetMouseButton(1))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            float deltaX = currentMousePosition.x - lastMousePosition.x;

            if (Mathf.Abs(deltaX) > 0.1f)
            {
                float sizeDelta = deltaX * brushSizeSensitivity;
                brushSize = Mathf.Clamp(brushSize + sizeDelta, minBrushSize, maxBrushSize);

                // 播放调整音效
                PlayBrushAdjustSound();
            }

            lastMousePosition = currentMousePosition;
        }

        // 松开右键结束调整
        if (Input.GetMouseButtonUp(1))
        {
            isAdjustingBrushSize = false;
            Cursor.visible = true;
        }
    }

    void PlayBrushAdjustSound()
    {
        if (brushAdjustClip == null || sfxSource == null) return;

        if (Time.time - lastAdjustSoundTime > adjustSoundInterval)
        {
            sfxSource.PlayOneShot(brushAdjustClip, sfxSource.volume);
            lastAdjustSoundTime = Time.time;
        }
    }

    // ======================== 🧰 工具按钮逻辑 ========================
    void InitializeToolButtons()
    {
        if (brushButton != null)
        {
            AddButtonHoverEffects(brushButton, ToolType.Brush);
            brushButton.onClick.AddListener(() => SwitchTool(ToolType.Brush));
        }

        if (sprayButton != null)
        {
            AddButtonHoverEffects(sprayButton, ToolType.Spray);
            sprayButton.onClick.AddListener(() => SwitchTool(ToolType.Spray));
        }

        if (paintBucketButton != null)
        {
            AddButtonHoverEffects(paintBucketButton, ToolType.PaintBucket);
            paintBucketButton.onClick.AddListener(() => SwitchTool(ToolType.PaintBucket));
        }
    }

    void AddButtonHoverEffects(Button button, ToolType toolType)
    {
        button.transform.localScale = originalScale;
        button.image.color = (currentTool == toolType) ? selectedColor : normalColor;

        var trigger = button.gameObject.AddComponent<EventTrigger>();

        var enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            if (currentTool != toolType)
            {
                button.transform.DOScale(originalScale * hoverScale, animationDuration);
                button.image.DOColor(hoverColor, animationDuration);
            }
        });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            if (currentTool != toolType)
            {
                button.transform.DOScale(originalScale, animationDuration);
                button.image.DOColor(normalColor, animationDuration);
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    public void SwitchTool(ToolType newTool)
    {
        if (currentTool == newTool) return;
        currentTool = newTool;
        UpdateToolButtonStates();
    }

    void UpdateToolButtonStates()
    {
        UpdateButtonState(brushButton, ToolType.Brush);
        UpdateButtonState(sprayButton, ToolType.Spray);
        UpdateButtonState(paintBucketButton, ToolType.PaintBucket);
    }

    void UpdateButtonState(Button button, ToolType toolType)
    {
        if (button == null) return;

        button.transform.DOKill();
        button.image.DOKill();

        if (currentTool == toolType)
        {
            button.transform.DOScale(originalScale * selectedScale, animationDuration);
            button.image.DOColor(selectedColor, animationDuration);
        }
        else
        {
            button.transform.DOScale(originalScale, animationDuration);
            button.image.DOColor(normalColor, animationDuration);
        }
    }

    // ======================== 🎨 颜色显示 ========================
    public void SetBrushColor(Color newColor)
    {
        brushColor = newColor;
        UpdateCurrentColorDisplay();
    }

    void UpdateCurrentColorDisplay()
    {
        if (currentColorDisplay != null)
        {
            currentColorDisplay.color = brushColor;

            currentColorDisplay.transform.DOKill();
            currentColorDisplay.transform.DOScale(1.1f, 0.1f)
                .OnComplete(() => currentColorDisplay.transform.DOScale(1f, 0.1f));
        }
    }

    void OnDestroy()
    {
        if (brushButton) brushButton.transform.DOKill();
        if (sprayButton) sprayButton.transform.DOKill();
        if (paintBucketButton) paintBucketButton.transform.DOKill();
    }
}

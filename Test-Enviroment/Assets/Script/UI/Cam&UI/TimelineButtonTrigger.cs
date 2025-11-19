using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class TimelineButtonTrigger : MonoBehaviour
{
    [Header("UI & Timeline")]
    public GameObject originalUI;
    public GameObject timelineUI;
    public PlayableDirector timelinePrefab;

    [Header("返回按钮")]
    public Button backButton;
    public bool reverseAnimation = true;
    public float reverseSpeed = 1.0f;

    private PlayableDirector instantiatedTimeline;
    private bool isPlaying = false;
    private bool isReversing = false;
    private Coroutine reverseCoroutine;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
            backButton.gameObject.SetActive(false);
        }
    }

    public void OnButtonClick()
    {
        if (isPlaying) return;

        StartCoroutine(PlayTimelineForward());
    }

    private IEnumerator PlayTimelineForward()
    {
        isPlaying = true;
        isReversing = false;

        // 隐藏原始UI
        if (originalUI != null)
            originalUI.SetActive(false);

        // 显示Timeline UI
        if (timelineUI != null)
            timelineUI.SetActive(true);

        // 实例化并播放Timeline
        if (timelinePrefab != null)
        {
            instantiatedTimeline = Instantiate(timelinePrefab);

            // 注册完成事件
            instantiatedTimeline.stopped += OnTimelineEnd;

            // 播放Timeline
            instantiatedTimeline.Play();

            // 显示返回按钮
            yield return new WaitForSeconds(0.5f);
            if (backButton != null)
            {
                backButton.gameObject.SetActive(true);
            }

            Debug.Log("开始正向播放Timeline");
        }
        else
        {
            Debug.LogError("Timeline预制体未设置!");
            isPlaying = false;
        }
    }

    public void OnBackButtonClick()
    {
        if (!isPlaying || isReversing) return;

        if (reverseAnimation)
        {
            StartReverseAnimation();
        }
        else
        {
            // 如果不倒放，直接返回
            StartCoroutine(ReturnImmediately());
        }
    }

    private void StartReverseAnimation()
    {
        if (reverseCoroutine != null)
            StopCoroutine(reverseCoroutine);

        reverseCoroutine = StartCoroutine(ReverseTimelineSmooth());
    }

    // 平滑倒放方法 - 修复时间计算问题
    private IEnumerator ReverseTimelineSmooth()
    {
        isReversing = true;

        // 隐藏返回按钮
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (instantiatedTimeline != null)
        {
            Debug.Log("开始倒放Timeline");

            // 获取当前时间和总时长
            double currentTime = instantiatedTimeline.time;
            double duration = instantiatedTimeline.duration;

            // 使用安全的时间计算
            while (currentTime > 0)
            {
                // 确保deltaTime为正
                float delta = Time.deltaTime;
                if (delta < 0)
                {
                    Debug.LogWarning("检测到负deltaTime，跳过该帧");
                    yield return null;
                    continue;
                }

                // 计算新的时间位置
                currentTime -= delta * reverseSpeed;
                if (currentTime < 0) currentTime = 0;

                // 安全地设置时间
                instantiatedTimeline.time = currentTime;
                instantiatedTimeline.Evaluate();

                yield return null;
            }

            // 确保回到起点
            instantiatedTimeline.time = 0;
            instantiatedTimeline.Evaluate();
        }

        CleanupAndReturn();
    }

    // 替代方案：使用时间缩放进行倒放
    private IEnumerator ReverseTimelineWithTimeScale()
    {
        isReversing = true;

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (instantiatedTimeline != null)
        {
            // 暂停Timeline
            instantiatedTimeline.playableGraph.GetRootPlayable(0).SetSpeed(0);

            double currentTime = instantiatedTimeline.time;
            double startTime = currentTime;

            // 逐步倒放
            while (currentTime > 0)
            {
                // 计算进度 (1 -> 0)
                float progress = (float)(currentTime / startTime);

                // 使用安全的时间计算
                float delta = Mathf.Max(0, Time.deltaTime);
                currentTime -= delta * reverseSpeed;
                if (currentTime < 0) currentTime = 0;

                // 设置时间
                instantiatedTimeline.time = currentTime;
                instantiatedTimeline.Evaluate();

                yield return null;
            }

            // 恢复播放速度
            instantiatedTimeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }

        CleanupAndReturn();
    }

    // 直接返回，不播放倒放动画
    private IEnumerator ReturnImmediately()
    {
        isReversing = true;

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        // 短暂延迟，让过渡更自然
        yield return new WaitForSeconds(0.3f);

        CleanupAndReturn();
    }

    private void OnTimelineEnd(PlayableDirector director)
    {
        // 正向播放完成时的处理
        if (!isReversing)
        {
            Debug.Log("Timeline正向播放完成");
        }
    }

    private void CleanupAndReturn()
    {
        // 停止所有协程
        if (reverseCoroutine != null)
        {
            StopCoroutine(reverseCoroutine);
            reverseCoroutine = null;
        }

        // 销毁Timeline实例
        if (instantiatedTimeline != null)
        {
            instantiatedTimeline.stopped -= OnTimelineEnd;
            instantiatedTimeline.Stop();
            Destroy(instantiatedTimeline.gameObject);
            instantiatedTimeline = null;
        }

        // 隐藏Timeline UI
        if (timelineUI != null)
            timelineUI.SetActive(false);

        // 显示原始UI
        if (originalUI != null)
            originalUI.SetActive(true);

        // 重置状态
        isPlaying = false;
        isReversing = false;

        Debug.Log("已返回原始状态");
    }

    void Update()
    {
        // 添加ESC键返回功能
        if (Input.GetKeyDown(KeyCode.Escape) && isPlaying && !isReversing)
        {
            OnBackButtonClick();
        }

        // 调试信息
        if (isPlaying && instantiatedTimeline != null)
        {
            Debug.Log($"Timeline时间: {instantiatedTimeline.time:F2}, 状态: {(isReversing ? "倒放" : "正放")}");
        }
    }

    void OnDestroy()
    {
        // 清理事件注册
        if (instantiatedTimeline != null)
        {
            instantiatedTimeline.stopped -= OnTimelineEnd;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClick);
        }

        // 停止所有协程
        if (reverseCoroutine != null)
        {
            StopCoroutine(reverseCoroutine);
        }
    }

    // 公共方法
    public void ForceReturn()
    {
        if (isPlaying)
        {
            OnBackButtonClick();
        }
    }

    // 编辑器调试方法
    [ContextMenu("测试正向播放")]
    private void TestForward()
    {
        OnButtonClick();
    }

    [ContextMenu("测试返回")]
    private void TestBack()
    {
        OnBackButtonClick();
    }
}
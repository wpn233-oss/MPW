using UnityEngine;

public class DebugHotkeyStageSwitcher : MonoBehaviour
{
    [Header("启用调试快捷键（发布时可关闭或删除）")]
    public bool enableHotkeys = true;

    [Header("快捷键设置")]
    public KeyCode keyToThrowing = KeyCode.F3;   // 拉坯
    public KeyCode keyToFiring = KeyCode.F4;     // 烧制
    public KeyCode keyToColoring = KeyCode.F5;   // 上色

    private GameFlowManager flow;

    void Start()
    {
        flow = FindObjectOfType<GameFlowManager>();
        if (flow == null)
        {
            Debug.LogWarning("⚠️ 未找到 GameFlowManager，快捷键切换将无法使用。");
            enabled = false;
        }
    }

    void Update()
    {
        if (!enableHotkeys || flow == null) return;

        if (Input.GetKeyDown(keyToThrowing))
        {
            flow.SwitchToThrowing();
            Debug.Log("🎯 快捷键：已切换到【拉坯】阶段");
        }

        if (Input.GetKeyDown(keyToFiring))
        {
            flow.SwitchToFiring();
            Debug.Log("🔥 快捷键：已切换到【烧制】阶段");
        }

        if (Input.GetKeyDown(keyToColoring))
        {
            flow.SwitchToColoring();
            Debug.Log("🎨 快捷键：已切换到【上色】阶段");
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    [Header("目标场景名称")]
    [Tooltip("填写要跳转的场景名（需在Build Settings中添加）")]
    public string targetSceneName;

    [Header("可选：按钮绑定")]
    [Tooltip("可直接在Inspector中拖入按钮")]
    public Button switchButton;

    [Header("调试输出")]
    public bool enableDebugLogs = true;

    void Start()
    {
        // 如果在 Inspector 里绑定了按钮，则自动监听
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(SwitchToTargetScene);
        }
    }

    /// <summary>
    /// 手动或按钮点击时调用
    /// </summary>
    public void SwitchToTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneSwitcher] 未设置目标场景名称！");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("[SceneSwitcher] 正在切换场景 → " + targetSceneName);

        // 直接加载目标场景
        SceneManager.LoadScene(targetSceneName);
    }
}

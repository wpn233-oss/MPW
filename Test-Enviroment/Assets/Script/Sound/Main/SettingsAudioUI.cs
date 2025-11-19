using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioUI : MonoBehaviour
{
    [Header("音量滑块")]
    public Slider bgmSlider;
    public Slider uiSlider;

    void Start()
    {
        // 初始化滑块数值（读取 AudioManager 中的音量）
        if (AudioManager.Instance != null)
        {
            bgmSlider.value = AudioManager.Instance.GetVolume(AudioType.BGM);
            uiSlider.value = AudioManager.Instance.GetVolume(AudioType.UI);
        }

        // 添加监听事件
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        uiSlider.onValueChanged.AddListener(OnUIChanged);
    }

    public void OnBGMChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(AudioType.BGM, value);
    }

    public void OnUIChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(AudioType.UI, value);
    }
}

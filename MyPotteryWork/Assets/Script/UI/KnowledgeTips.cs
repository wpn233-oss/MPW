using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class KnowledgeTips : MonoBehaviour
{
    [Header("UI组件")]
    public CanvasGroup tipPanel;
    public TMP_Text tipText;
    public Image tipImage;

    [Header("默认切换参数")]
    [Tooltip("如果当前场景没有特殊设定，就用这个默认值")]
    public float defaultDuration = 10f;
    public float fadeDuration = 1f;

    [Header("场景专属显示时间")]
    public List<SceneTipSetting> sceneSettings = new List<SceneTipSetting>();

    [TextArea(3, 8)]
    public string[] tips;

    private int currentTipIndex = 0;
    private Coroutine tipRoutine;
    private float displayDuration;

    [System.Serializable]
    public class SceneTipSetting
    {
        public string sceneName;
        public float displayDuration = 10f;
    }

    void Start()
    {
        // 根据场景自动调整显示间隔
        string scene = SceneManager.GetActiveScene().name;
        displayDuration = GetDurationForScene(scene);

        if (tips.Length > 0)
            StartTips();
    }

    float GetDurationForScene(string sceneName)
    {
        foreach (var setting in sceneSettings)
        {
            if (setting.sceneName == sceneName)
                return setting.displayDuration;
        }
        return defaultDuration;
    }

    public void StartTips()
    {
        if (tipRoutine != null)
            StopCoroutine(tipRoutine);
        tipRoutine = StartCoroutine(ShowTipsLoop());
    }

    public void StopTips()
    {
        if (tipRoutine != null)
            StopCoroutine(tipRoutine);
        tipPanel.alpha = 0;
    }

    IEnumerator ShowTipsLoop()
    {
        while (true)
        {
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeOut());
            currentTipIndex = (currentTipIndex + 1) % tips.Length;
        }
    }

    IEnumerator FadeIn()
    {
        tipText.text = tips[currentTipIndex];
        for (float t = 0; t < 1; t += Time.deltaTime / fadeDuration)
        {
            tipPanel.alpha = t;
            yield return null;
        }
        tipPanel.alpha = 1;
    }

    IEnumerator FadeOut()
    {
        for (float t = 1; t > 0; t -= Time.deltaTime / fadeDuration)
        {
            tipPanel.alpha = t;
            yield return null;
        }
        tipPanel.alpha = 0;
    }
}

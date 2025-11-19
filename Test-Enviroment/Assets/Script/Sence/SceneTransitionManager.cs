using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("过渡设置")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("加载界面")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public Text progressText;
    public Text loadingTipText;

    [Header("加载提示")]
    public string[] loadingTips;

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化时确保加载界面隐藏
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (fadeImage != null) fadeImage.gameObject.SetActive(false);
    }

    // 通用场景跳转方法
    public void LoadScene(string sceneName, bool useLoadingScreen = true)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionToScene(sceneName, useLoadingScreen));
    }

    public void LoadScene(int sceneIndex, bool useLoadingScreen = true)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionToScene(SceneManager.GetSceneAt(sceneIndex).name, useLoadingScreen));
    }

    private IEnumerator TransitionToScene(string sceneName, bool useLoadingScreen)
    {
        isTransitioning = true;

        // 淡出效果
        yield return StartCoroutine(FadeOut());

        // 显示加载界面
        if (useLoadingScreen && loadingScreen != null)
        {
            loadingScreen.SetActive(true);

            // 随机显示加载提示
            if (loadingTipText != null && loadingTips.Length > 0)
            {
                loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }
        }

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 更新进度条
        float progress = 0;
        while (!asyncLoad.isDone)
        {
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Unity加载进度到0.9就暂停

            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"{(progress * 100):F0}%";

            // 当加载接近完成时，允许场景激活
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 隐藏加载界面
        if (useLoadingScreen && loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        // 淡入效果
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = fadeCurve.Evaluate(elapsedTime / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1);
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = fadeCurve.Evaluate(1 - (elapsedTime / fadeDuration));
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 0);
        fadeImage.gameObject.SetActive(false);
    }

    // 从对话选项调用场景跳转
    public void LoadSceneFromDialogue(string sceneName)
    {
        LoadScene(sceneName, true);
    }
}
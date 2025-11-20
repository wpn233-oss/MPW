using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance; // 单例，全局调用
    public Camera loadingCamera;        // 专门拍 Quad 的相机
    public Camera mainCamera;           // 主相机
    public Animator loadingAnimator;    // Quad 上的 Animator
    public float animationTime = 1.0f;  // 动画时长（和 Animator Clip 对应）



    private void Awake()
    {
        // 保持全局唯一
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保留
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 对外接口：切换场景
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 1. 禁用所有 UI（Canvas）
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            canvas.enabled = false;
        }

        // 2. 开启 loading 相机，关闭主相机
        loadingCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        // 3. 直接播放动画（不使用参数）
        if (loadingAnimator != null)
        {
            loadingAnimator.Play("TranSence"); // 直接播放名为"TranSence"的动画状态
        }

        yield return new WaitForSeconds(animationTime);

        // 4. 异步加载场景
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;

        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        asyncOp.allowSceneActivation = true;
        // 等待一帧，确保场景加载完
        yield return null;

        // 禁用 loadingCamera，启用新场景主相机
        loadingCamera.gameObject.SetActive(false);

        Camera newMainCamera = Camera.main;
        if (newMainCamera != null)
        {
            newMainCamera.gameObject.SetActive(true);
            Debug.Log("[SceneLoader] 已切换到新场景主相机：" + newMainCamera.name);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] 未找到新场景主相机，请确认相机 Tag 为 MainCamera");
        }

    }


}
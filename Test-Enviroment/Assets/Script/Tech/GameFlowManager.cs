using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("摄像机")]
    public Camera throwingCamera;
    public Camera firingCamera;
    public Camera coloringCamera;

    [Header("UI Canvas")]
    public GameObject throwingUI;
    public GameObject firingUI;
    public GameObject coloringUI;

    [Header("阶段控制脚本")]
    public MonoBehaviour throwingScript;    // 拉坯脚本（Potteryprototype）
    public MonoBehaviour firingScript;      // 烧制脚本（FiringMiniGame）
    public MonoBehaviour coloringScript;    // 上色控制脚本（ColoringManager）

    [Header("位置点")]
    public Transform throwingSpot;
    public Transform firingSpot;
    public Transform coloringSpot;
    public GameObject pottery;

    // 🟢 保存初始比例
    private Vector3 initialScale;

    void Start()
    {
        // 记录初始陶坯比例
        if (pottery != null)
            initialScale = pottery.transform.localScale;

        // 默认进入拉坯阶段
        SwitchToThrowing();
    }

    // 🌀 拉坯阶段
    public void SwitchToThrowing()
    {
        throwingCamera.gameObject.SetActive(true);
        firingCamera.gameObject.SetActive(false);
        coloringCamera.gameObject.SetActive(false);

        throwingUI.SetActive(true);
        firingUI.SetActive(false);
        coloringUI.SetActive(false);

        throwingScript.enabled = true;
        firingScript.enabled = false;
        coloringScript.enabled = false;

        if (pottery != null && throwingSpot != null)
        {
            pottery.transform.position = throwingSpot.position;
            pottery.transform.rotation = throwingSpot.rotation;
            pottery.transform.localScale = initialScale; // 恢复原比例
        }

        Debug.Log("🌀 进入拉坯阶段");
    }

    // 🔥 烧制阶段
    public void SwitchToFiring()
    {
        throwingCamera.gameObject.SetActive(false);
        firingCamera.gameObject.SetActive(true);
        coloringCamera.gameObject.SetActive(false);

        throwingUI.SetActive(false);
        firingUI.SetActive(true);
        coloringUI.SetActive(false);

        throwingScript.enabled = false;
        firingScript.enabled = true;
        coloringScript.enabled = false;

        if (pottery != null && firingSpot != null)
        {
            pottery.transform.position = firingSpot.position;
            pottery.transform.rotation = firingSpot.rotation;
            pottery.transform.localScale = initialScale; // 保持原比例
        }

        Debug.Log("🔥 进入烧制阶段");
    }

    // 🎨 上色阶段
    public void SwitchToColoring()
    {
        throwingCamera.gameObject.SetActive(false);
        firingCamera.gameObject.SetActive(false);
        coloringCamera.gameObject.SetActive(true);

        throwingUI.SetActive(false);
        firingUI.SetActive(false);
        coloringUI.SetActive(true);

        throwingScript.enabled = false;
        firingScript.enabled = false;
        coloringScript.enabled = true;

        if (pottery != null && coloringSpot != null)
        {
            pottery.transform.position = coloringSpot.position;
            pottery.transform.rotation = coloringSpot.rotation;
            pottery.transform.localScale = initialScale * 0.75f; // ✅ 固定缩小比例
        }

        Debug.Log("🎨 进入上色阶段");
    }
}

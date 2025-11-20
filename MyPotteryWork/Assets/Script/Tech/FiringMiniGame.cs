using UnityEngine;
using UnityEngine.UI;

public class FiringMiniGame : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform pointer;          // 指针UI对象
    public RectTransform bar;              // 火力条背景
    public Animator pointerAnimator;       // 控制指针外观（InZone/OutZone）
    public Image successZoneImage;         // 成功区间可视化
    public GameObject floatingTextPrefab;  // 成功提示预制体
    public GameObject missTextPrefab;      // 失败提示预制体

    [Header("Settings")]
    public float pointerSpeed = 2f;        // 指针基础移动速度
    public float moveSpeed = 1f;           // 玩家 A/D 控制速度
    public int maxMisses = 3;              // 碰到边缘最大容错
    public int requiredHits = 5;           // 需要成功命中的次数
    [Range(0f, 1f)] public float successZoneMin = 0.4f;
    [Range(0f, 1f)] public float successZoneMax = 0.6f;

    [Header("Pointer Shake Settings")]
    public float noiseIntensity = 0.5f;    // 抖动幅度
    public float noiseSpeed = 2f;          // 抖动速度

    // 内部状态变量
    private float pointerPosition = 0.5f;  // 指针位置 (0~1)
    private int direction = 1;             // 移动方向 (1=右, -1=左)
    private int missCount = 0;             // 撞边缘累计失败次数
    private int currentHits = 0;           // 成功命中次数
    private bool gameActive = true;        // 游戏是否运行中
    private bool atEdge = false;           // 防止边缘重复触发
    private Vector2 barSize;               // 火力条大小缓存

    [SerializeField] private GameObject firingUI;   // 拖你烧制小游戏的UI进来
    [SerializeField] private GameObject resultPopup; // 等下我们做的弹窗


    void Start()
    {
        // 缓存bar的size，减少开销
        barSize = bar.rect.size;

        // 设置成功区间的可视化宽度和位置
        if (successZoneImage != null)
        {
            float zoneWidth = (successZoneMax - successZoneMin) * barSize.x;
            successZoneImage.rectTransform.sizeDelta = new Vector2(zoneWidth, successZoneImage.rectTransform.sizeDelta.y);
            successZoneImage.rectTransform.anchoredPosition = new Vector2(
                barSize.x * (successZoneMin - 0.5f + (successZoneMax - successZoneMin) / 2f),
                successZoneImage.rectTransform.anchoredPosition.y
            );
        }
    }

    void Update()
    {
        if (!gameActive) return;

        // 🔥 指针移动：基础速度 + 随机扰动
        float noise = (Mathf.PerlinNoise(Time.time * noiseSpeed, 0f) - 0.5f) * 2f * noiseIntensity;
        pointerPosition += direction * (pointerSpeed + noise) * Time.deltaTime;

        // 🎮 玩家输入
        if (Input.GetKey(KeyCode.A)) pointerPosition -= moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) pointerPosition += moveSpeed * Time.deltaTime;

        if (Random.value < 0.01f)
        {
            pointerSpeed = Random.Range(0.1f, 0.5f);
        }

        // 🚧 边缘检测：防止多次触发
        if (pointerPosition >= 1f)
        {
            if (!atEdge)
            {
                pointerPosition = 1f;
                direction = -1;
                missCount++;
                ShowFloatingText(missTextPrefab); // 显示 MISS 提示
                CheckFail();
                atEdge = true;
            }
        }
        else if (pointerPosition <= 0f)
        {
            if (!atEdge)
            {
                pointerPosition = 0f;
                direction = 1;
                missCount++;
                ShowFloatingText(missTextPrefab);
                CheckFail();
                atEdge = true;
            }
        }
        else
        {
            atEdge = false; // 回到中间区域后重置
        }

        // 限制在 0~1
        pointerPosition = Mathf.Clamp01(pointerPosition);

        // 更新UI位置
        pointer.anchoredPosition = new Vector2(barSize.x * (pointerPosition - 0.5f), pointer.anchoredPosition.y);

        // 动画切换（InZone / OutZone）
        bool inZone = pointerPosition >= successZoneMin && pointerPosition <= successZoneMax;
        if (pointerAnimator != null)
            pointerAnimator.SetBool("InZone", inZone);

        // ⌨️ 空格键判定
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inZone)
            {
                currentHits++;
                ShowFloatingText(floatingTextPrefab); // 成功提示
                Debug.Log($"命中成功 {currentHits}/{requiredHits}");

                if (currentHits >= requiredHits)
                {
                    Debug.Log("🔥 烧制成功！");
                    gameActive = false;

                    // 调用游戏流程管理器进入上色环节
                    GameFlowManager flow = FindObjectOfType<GameFlowManager>();
                    if (flow != null)
                    {
                        flow.SwitchToColoring(); // 稍后我们在 GameFlowManager 里加这个函数
                    }
                }

            }
            else
            {
                // 错按惩罚：更难控制
                pointerSpeed += 0.5f;
                pointerPosition += direction * 0.05f;
                ShowFloatingText(missTextPrefab); // 显示 MISS 提示
                Debug.Log("⚠️ 按错了，但进度保留！");
            }
        }
    }

    void CheckFail()
    {
        if (missCount >= maxMisses)
        {
            Debug.Log(" 烧制失败！");
            gameActive = false;
        }
    }

    public void EndFiring(bool success)
    {
        // 1. 关闭烧制小游戏UI
        firingUI.SetActive(false);

        // 2. 打开结果弹窗
        resultPopup.SetActive(true);


    }


    public void ShowFloatingText(GameObject prefab, float yOffset = 50f)
    {
        if (prefab == null || pointer == null) return;

        // 在指针下生成 prefab
        GameObject go = Instantiate(prefab, pointer);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            // 基于指针的局部位置，只加 Y 偏移
            rt.localPosition = new Vector3(0f, yOffset, 0f);
        }

        Animator anim = go.GetComponent<Animator>();
        if (anim != null) anim.Play("FloatingUp", -1, 0f);

        Destroy(go, 1f);
    }
}


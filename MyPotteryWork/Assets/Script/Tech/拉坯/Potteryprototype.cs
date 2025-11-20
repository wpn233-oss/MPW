using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Potteryprototype : MonoBehaviour
{
    [Header("UI控制")]
    public TMP_Text parametersInfoText;
    public Slider 力度滑块;
    public Slider 笔刷大小滑块;
    public Button 重置按钮;
    public Button 烧制按钮;

    [Header("笔刷可视化（UI）")]
    public RectTransform 笔刷UI;   // 在 Inspector 指定 Canvas 下的 Image 的 RectTransform

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Mesh mesh;

    [Header("模型参数")]
    public int 圆周细分 = 40;
    public int 高度层数 = 20;
    public float 层高 = 0.1f;
    public float 外半径 = 1.0f;
    public float 内半径 = 0.9f;

    [Header("交互参数")]
    public float 力度等级 = 5f;
    [Range(1f, 4f)]  // ✅ 上限强制为4
    public float 高度影响范围 = 2f;
    public float 最小半径 = 0.5f;
    public float 最大半径 = 2.0f;
    public float 固定底部高度 = 0.2f;

    private List<Vector3> vertices;
    private List<Vector2> UV;
    private List<int> triangles;
    private List<Vector3> 初始顶点位置;

    private float 每段角度;
    private int 每层顶点数;

    // 鼠标右键调整参数相关
    private Vector3 上一帧鼠标位置;
    private float 实际变形力度 => 0.005f * 力度等级;

    private Plane potteryPlane;

    // ---- 笔刷UI缩放相关 ----
    private Vector2 笔刷当前大小 = Vector2.zero;
    [Header("UI参数")]
    public float 最小笔刷UISize = 30f;
    public float 最大笔刷UISize = 200f;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (mesh == null)
        {
            mesh = new Mesh();
        }

        创建笔刷指示器();

        potteryPlane = new Plane(Vector3.up, transform.position);

        初始化UI();
        生成陶器();

        上一帧鼠标位置 = Input.mousePosition;
        Cursor.visible = true;
    }

    void 创建笔刷指示器()
    {
        if (笔刷UI != null)
        {
            笔刷UI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("未指定笔刷UI（RectTransform），将无法显示笔刷大小");
        }
    }

    void 初始化UI()
    {
        if (力度滑块)
        {
            力度滑块.wholeNumbers = false;
            力度滑块.minValue = 1;
            力度滑块.maxValue = 10;
            力度滑块.value = 力度等级;
            力度滑块.onValueChanged.AddListener(设置力度等级);

            var valueText = 力度滑块.GetComponentInChildren<TMP_Text>();
            if (valueText) valueText.text = 力度等级.ToString();
        }

        if (笔刷大小滑块)
        {
            笔刷大小滑块.minValue = 1f;
            笔刷大小滑块.maxValue = 4f;   // ✅ 限制到4
            笔刷大小滑块.value = 高度影响范围;
            笔刷大小滑块.onValueChanged.AddListener(设置笔刷大小);

            var sizeText = 笔刷大小滑块.GetComponentInChildren<TMP_Text>();
            if (sizeText) sizeText.text = 高度影响范围.ToString("F0");
        }

        if (重置按钮)
        {
            重置按钮.onClick.AddListener(重置陶器);
            var buttonText = 重置按钮.GetComponentInChildren<TMP_Text>();
            if (buttonText) buttonText.text = "重置";
        }

        更新参数显示();
    }

    void 设置笔刷大小(float value)
    {
        高度影响范围 = value;
        if (笔刷大小滑块)
        {
            var sizeText = 笔刷大小滑块.GetComponentInChildren<TMP_Text>();
            if (sizeText) sizeText.text = 高度影响范围.ToString("F0");
        }
        更新参数显示();
        更新笔刷指示器大小();
    }

    void Update()
    {
        处理鼠标变形();
        处理右键调整参数();
        更新笔刷指示器位置();
    }

    void 更新笔刷指示器位置()
    {
        if (笔刷UI == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (笔刷UI.gameObject.activeSelf) 笔刷UI.gameObject.SetActive(false);
            Cursor.visible = true;
            return;
        }

        Ray 射线 = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit 碰撞信息;
        bool 击中陶器 = Physics.Raycast(射线, out 碰撞信息) && 碰撞信息.collider.gameObject == gameObject;

        Vector3 worldBrushPos = Vector3.zero;
        bool valid = false;

        if (!击中陶器)
        {
            float 交点距离;
            if (potteryPlane.Raycast(射线, out 交点距离))
            {
                Vector3 平面交点 = 射线.GetPoint(交点距离);
                Vector3 到陶器中心 = transform.position - 平面交点;
                到陶器中心.y = 0;

                if (到陶器中心.magnitude < 外半径 * 1.5f)
                {
                    Vector3 投影点 = transform.position - 到陶器中心.normalized * 外半径;
                    投影点.y = Mathf.Clamp(平面交点.y, 0, 高度层数 * 层高);
                    worldBrushPos = 投影点 + Vector3.up * 0.01f;
                    valid = true;
                }
            }
        }
        else
        {
            worldBrushPos = 碰撞信息.point + 碰撞信息.normal * 0.01f;
            valid = true;
        }

        if (!valid)
        {
            if (笔刷UI.gameObject.activeSelf) 笔刷UI.gameObject.SetActive(false);
            Cursor.visible = true;
            return;
        }

        if (!笔刷UI.gameObject.activeSelf) 笔刷UI.gameObject.SetActive(true);
        Cursor.visible = false;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldBrushPos);
        if (screenPoint.z <= 0f)
        {
            笔刷UI.gameObject.SetActive(false);
            Cursor.visible = true;
            return;
        }

        Canvas canvas = 笔刷UI.GetComponentInParent<Canvas>();
        Vector2 anchoredPos;
        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        Camera camForCanvas = (canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPoint, camForCanvas, out anchoredPos);
        笔刷UI.anchoredPosition = anchoredPos;

        更新笔刷指示器大小();
    }

    void 更新笔刷指示器大小()
    {
        if (笔刷UI == null) return;

        // 将高度影响范围 (1~4) 线性映射到 UI 像素范围
        float t = Mathf.InverseLerp(1f, 4f, 高度影响范围);
        float targetSize = Mathf.Lerp(最小笔刷UISize, 最大笔刷UISize, t);

        Vector2 target = new Vector2(targetSize, targetSize);

        // 插值过渡，保证平滑
        笔刷当前大小 = Vector2.Lerp(笔刷当前大小, target, 0.15f);
        笔刷UI.sizeDelta = 笔刷当前大小;
    }

    // ✅ 修改后的右键调整逻辑
    void 处理右键调整参数()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 当前鼠标位置 = Input.mousePosition;
            float 鼠标X移动量 = (当前鼠标位置.x - 上一帧鼠标位置.x);
            float 鼠标Y移动量 = (当前鼠标位置.y - 上一帧鼠标位置.y);


            if (Mathf.Abs(鼠标X移动量) > 0.01f)
            {
                float 调整速度 = 0.02f; // 越小越细腻
                float 新力度 = Mathf.Clamp(力度等级 + 鼠标X移动量 * 调整速度, 1f, 10f);

                设置力度等级(新力度);
                if (力度滑块) 力度滑块.value = 新力度;
            }

            // 笔刷大小改成连续值调节
            if (Mathf.Abs(鼠标Y移动量) > 0.01f)
            {
                float 调整速度 = 0.01f; // 越小越细腻
                float 新笔刷大小 = Mathf.Clamp(高度影响范围 + 鼠标Y移动量 * 调整速度, 1f, 4f);

                设置笔刷大小(新笔刷大小);
                if (笔刷大小滑块) 笔刷大小滑块.value = 新笔刷大小;
            }

            上一帧鼠标位置 = 当前鼠标位置;
        }
    }
    void 设置力度等级(float value)
    {
        力度等级 = Mathf.Clamp(value, 1f, 10f);
        if (力度滑块)
        {
            var valueText = 力度滑块.GetComponentInChildren<TMP_Text>();
            if (valueText) valueText.text = 力度等级.ToString("F0");
        }
        更新参数显示();
    }

    void 重置陶器()
    {
        生成陶器();
    }

    void 更新参数显示()
    {
        if (parametersInfoText)
        {
            parametersInfoText.text =
                $"<b>强度:</b> {力度等级:F0}/10          " + $"<b>大小:</b> {高度影响范围:F0}";
        }
    }

    [ContextMenu("生成陶器")]
    void 生成陶器()
    {
        if (mesh == null) mesh = new Mesh();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        UV = new List<Vector2>();

        每段角度 = Mathf.PI * 2 / 圆周细分;
        for (int i = 0; i < 高度层数; i++)
        {
            生成圆环(i);
        }
        封顶();

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = UV.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        初始顶点位置 = new List<Vector3>(vertices);

        potteryPlane = new Plane(Vector3.up, transform.position);

        更新参数显示();
        更新笔刷指示器大小();
    }

    void 生成圆环(int 当前层)
    {
        List<Vector3> 外顶点 = new List<Vector3>();
        List<Vector3> 内顶点 = new List<Vector3>();
        List<Vector2> 外UV = new List<Vector2>();
        List<Vector2> 内UV = new List<Vector2>();

        // 使用更精确的循环控制
        for (int segment = 0; segment <= 圆周细分; segment++)
        {
            float angle = (float)segment / 圆周细分 * Mathf.PI * 2f;

            // 顶点生成保持不变
            Vector3 v1 = new Vector3(外半径 * Mathf.Sin(angle), 当前层 * 层高, 外半径 * Mathf.Cos(angle));
            Vector3 v2 = new Vector3(外半径 * Mathf.Sin(angle), (当前层 + 1) * 层高, 外半径 * Mathf.Cos(angle));
            Vector3 v3 = new Vector3(内半径 * Mathf.Sin(angle), 当前层 * 层高, 内半径 * Mathf.Cos(angle));
            Vector3 v4 = new Vector3(内半径 * Mathf.Sin(angle), (当前层 + 1) * 层高, 内半径 * Mathf.Cos(angle));

            外顶点.Add(v1); 外顶点.Add(v2);
            内顶点.Add(v3); 内顶点.Add(v4);

            // 改进的UV计算 - 参考Potteryprototype_M.cs
            float u = 1f - ((float)segment / 圆周细分);

            // 高度方向的UV均匀分布
            float v_bottom = (float)当前层 / 高度层数;
            float v_top = (float)(当前层 + 1) / 高度层数;

            外UV.Add(new Vector2(u, v_bottom));
            外UV.Add(new Vector2(u, v_top));
            内UV.Add(new Vector2(u, v_bottom));
            内UV.Add(new Vector2(u, v_top));
        }

        vertices.AddRange(外顶点);
        vertices.AddRange(内顶点);
        UV.AddRange(外UV);
        UV.AddRange(内UV);

        每层顶点数 = 外顶点.Count;
        int 起始索引 = vertices.Count - 外顶点.Count - 内顶点.Count;

        // 三角形生成逻辑保持不变...
        for (int i = 起始索引; i < 起始索引 + 外顶点.Count - 2; i += 2)
        {
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 1);
            triangles.Add(i + 2); triangles.Add(i + 3); triangles.Add(i + 1);

            int 内索引 = i + 外顶点.Count;
            triangles.Add(内索引); triangles.Add(内索引 + 1); triangles.Add(内索引 + 2);
            triangles.Add(内索引 + 2); triangles.Add(内索引 + 1); triangles.Add(内索引 + 3);
        }
    }

    void 封顶()
    {
        int 起始索引 = vertices.Count;

        for (int segment = 0; segment <= 圆周细分; segment++)
        {
            float angle = (float)segment / 圆周细分 * Mathf.PI * 2f;

            Vector3 外顶点 = new Vector3(外半径 * Mathf.Sin(angle), 高度层数 * 层高, 外半径 * Mathf.Cos(angle));
            Vector3 内顶点 = new Vector3(内半径 * Mathf.Sin(angle), 高度层数 * 层高, 内半径 * Mathf.Cos(angle));

            vertices.Add(外顶点);
            vertices.Add(内顶点);

            // 改进的顶部UV - 使用圆形展开
            float u = (float)segment / 圆周细分;

            // 外圈顶点使用圆形UV
            Vector2 uv_outer = new Vector2(
                0.5f + 0.5f * Mathf.Cos(angle),
                0.5f + 0.5f * Mathf.Sin(angle)
            );

            // 内圈顶点使用缩小的圆形UV
            Vector2 uv_inner = new Vector2(
                0.5f + 0.25f * Mathf.Cos(angle),
                0.5f + 0.25f * Mathf.Sin(angle)
            );

            UV.Add(uv_outer);
            UV.Add(uv_inner);
        }

        // 三角形生成逻辑保持不变
        for (int i = 起始索引; i < vertices.Count - 2; i += 2)
        {
            triangles.Add(i); triangles.Add(i + 3); triangles.Add(i + 1);
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
        }

        triangles.Add(vertices.Count - 2);
        triangles.Add(起始索引 + 1);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);
        triangles.Add(起始索引);
        triangles.Add(起始索引 + 1);
    }

    void 处理鼠标变形()
    {
        if (!Input.GetMouseButton(0)) return;
        if (mesh == null) return;
        if (初始顶点位置 == null || 初始顶点位置.Count == 0) return;

        Ray 射线 = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit 碰撞信息;
        bool 击中陶器 = Physics.Raycast(射线, out 碰撞信息) && 碰撞信息.collider.gameObject == gameObject;

        Vector3 碰撞点 = Vector3.zero;
        if (!击中陶器)
        {
            float 交点距离;
            if (potteryPlane.Raycast(射线, out 交点距离))
            {
                Vector3 平面交点 = 射线.GetPoint(交点距离);
                Vector3 到陶器中心 = transform.position - 平面交点;
                到陶器中心.y = 0;

                if (到陶器中心.magnitude < 外半径 * 1.5f)
                {
                    碰撞点 = transform.position - 到陶器中心.normalized * 外半径;
                    碰撞点.y = Mathf.Clamp(平面交点.y, 0, 高度层数 * 层高);
                }
                else return;
            }
            else return;
        }
        else
        {
            碰撞点 = 碰撞信息.point;
        }

        Vector3[] 当前顶点 = mesh.vertices;
        float 鼠标X = Input.GetAxis("Mouse X");
        float 鼠标Y = Input.GetAxis("Mouse Y");

        for (int i = 0; i < 当前顶点.Length; i++)
        {
            Vector3 世界坐标 = transform.TransformPoint(当前顶点[i]);
            bool 是底部顶点 = 初始顶点位置[i].y < 固定底部高度;
            float 高度差 = Mathf.Abs(碰撞点.y - 世界坐标.y) / 层高;

            if (高度差 < 高度影响范围)
            {
                float 影响因子 = Mathf.Cos((高度差 / 高度影响范围) * Mathf.PI / 2);
                float 当前半径 = new Vector2(当前顶点[i].x, 当前顶点[i].z).magnitude;

                bool 是顶盖顶点 = i >= 每层顶点数 * 高度层数 * 2;

                float 半径变化 = 鼠标X * 实际变形力度 * 影响因子;
                float 新半径 = Mathf.Clamp(当前半径 + 半径变化, 最小半径, 最大半径);

                float 原始角度 = Mathf.Atan2(当前顶点[i].z, 当前顶点[i].x);
                float 新X = 新半径 * Mathf.Cos(原始角度);
                float 新Z = 新半径 * Mathf.Sin(原始角度);

                当前顶点[i] = new Vector3(新X, 当前顶点[i].y, 新Z);

                if (Mathf.Abs(鼠标Y) > 0.01f && !是顶盖顶点 && !是底部顶点)
                {
                    float 高度变化 = 鼠标Y * 实际变形力度 * 0.5f * 影响因子;
                    当前顶点[i] = new Vector3(
                        当前顶点[i].x,
                        Mathf.Clamp(当前顶点[i].y + 高度变化, 固定底部高度, 高度层数 * 层高),
                        当前顶点[i].z
                    );
                }
            }
        }

        mesh.vertices = 当前顶点;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    void OnDisable()
    {
        Cursor.visible = true;
        if (笔刷UI != null && 笔刷UI.gameObject.activeSelf)
        {
            笔刷UI.gameObject.SetActive(false);
        }
    }
}

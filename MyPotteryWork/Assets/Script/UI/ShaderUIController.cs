using UnityEngine;
using UnityEngine.UI;

public class ShaderUIController : MonoBehaviour
{
    public Image uiImage;   // UI Image
    private Material matInstance;

    [Range(0, 2)]
    public float progress;  // Animator 曲线控制

    void Awake()
    {
        // 克隆材质，确保每个 UI 都有自己的实例
        matInstance = Instantiate(uiImage.materialForRendering);
        uiImage.material = matInstance;
    }

    void Update()
    {
        if (matInstance != null)
        {
            // "_Slider" 要跟 Shader Graph 参数名完全一致
            matInstance.SetFloat("_Slider", progress);
        }
    }
}
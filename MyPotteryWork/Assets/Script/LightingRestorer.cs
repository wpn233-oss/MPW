using UnityEngine;

public class LightingRestorer : MonoBehaviour
{
    void Awake()
    {
        if (RenderSettings.sun == null)
        {
            var sun = FindObjectOfType<Light>();
            if (sun != null && sun.type == LightType.Directional)
            {
                RenderSettings.sun = sun;
                Debug.Log("[LightingRestorer] 恢复主光源：" + sun.name);
            }
            else
            {
                Debug.LogWarning("[LightingRestorer] 未找到 Directional Light，无法设置主光源。");
            }
        }

        // 确保环境光恢复
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;
        DynamicGI.UpdateEnvironment();
    }
}

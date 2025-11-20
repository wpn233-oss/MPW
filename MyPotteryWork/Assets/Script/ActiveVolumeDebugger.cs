using UnityEngine;
using UnityEngine.Rendering;

public class ActiveVolumeDebugger : MonoBehaviour
{
    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[VolumeDebugger] 没有找到主相机。");
            return;
        }

        Debug.Log($"[VolumeDebugger] 当前相机: {cam.name}, LayerMask: {cam.cullingMask}");

        foreach (var vol in FindObjectsOfType<Volume>(true))
        {
            bool isGlobal = vol.isGlobal;
            float weight = vol.weight;
            string scene = vol.gameObject.scene.name ?? "(DontDestroy)";
            Debug.Log($"[VolumeDebugger] Volume: {vol.name}, Scene: {scene}, Global: {isGlobal}, Weight: {weight}, Priority: {vol.priority}");
        }
    }
}

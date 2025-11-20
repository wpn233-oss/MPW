using UnityEngine;

public class ColoringManager : MonoBehaviour
{
    public GameObject pottery;
    public Camera coloringCamera;
    public Transform displaySpot;
    [Range(0.3f, 1f)] public float potteryScale = 0.75f;

    private Vector3 originalScale;
    private bool scaled;

    void Start()
    {
        if (pottery != null)
            originalScale = pottery.transform.localScale;
    }

    void Update()
    {
        if (pottery == null || coloringCamera == null) return;

        bool isColoring = coloringCamera.gameObject.activeInHierarchy;

        // 进入上色阶段：缩放 & 定位
        if (isColoring && !scaled)
        {
            if (displaySpot)
            {
                pottery.transform.position = displaySpot.position;
                pottery.transform.rotation = displaySpot.rotation;
            }
            pottery.transform.localScale = originalScale * potteryScale;
            scaled = true;
        }
        // 离开上色阶段：还原大小
        else if (!isColoring && scaled)
        {
            pottery.transform.localScale = originalScale;
            scaled = false;
        }
    }
}

using UnityEngine;

public class SpriteShake : MonoBehaviour
{
    [Header("晃动控制")]
    public bool enableShake = true;
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 3f;
    public bool useSmoothNoise = true;

    [Header("呼吸/上下浮动")]
    public bool enableBreathing = true;
    public float breathingIntensity = 0.02f;
    public float breathingSpeed = 1.5f;

    private Vector3 initialPos;
    private float noiseOffset;
    private Vector3 velocity;

    void Start()
    {
        initialPos = transform.localPosition;
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!enableShake) return;

        float time = Time.time * shakeSpeed + noiseOffset;

        // 基础 Perlin 噪声
        float nx = Mathf.PerlinNoise(time, 0f) * 2 - 1;
        float ny = Mathf.PerlinNoise(0f, time) * 2 - 1;

        // 呼吸浮动
        float breath = enableBreathing ? Mathf.Sin(Time.time * breathingSpeed) * breathingIntensity : 0f;

        Vector3 targetPos = new Vector3(
            nx * shakeIntensity,
            ny * shakeIntensity + breath,
            0f
        );

        if (useSmoothNoise)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, initialPos + targetPos, ref velocity, 0.1f);
        else
            transform.localPosition = initialPos + targetPos;
    }

    [ContextMenu("测试晃动")]
    void TestShake()
    {
        enableShake = !enableShake;
        if (!enableShake) transform.localPosition = initialPos;
    }
}

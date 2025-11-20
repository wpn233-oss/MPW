using UnityEngine;
using System.Collections;

public class FollowTargetShake : MonoBehaviour
{
    [Header("基础晃动设置")]
    public bool enableShake = true;
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 1f;

    [Header("晃动轴控制")]
    public Vector3 positionStrength = new Vector3(0.8f, 0.5f, 0.3f);
    public Vector3 rotationStrength = new Vector3(0.5f, 0.3f, 0.2f);

    [Header("呼吸效果")]
    public bool enableBreathing = true;
    public float breathingIntensity = 0.05f;
    public float breathingSpeed = 0.8f;

    [Header("高级设置")]
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public bool useSmoothNoise = true;
    public float smoothTime = 0.2f;

    // 私有变量
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private float noiseOffset;

    void Start()
    {
        // 保存初始状态
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // 随机偏移，避免所有对象同步
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!enableShake) return;

        ApplyShakeEffect();
    }

    void ApplyShakeEffect()
    {
        float time = Time.time + noiseOffset;
        float intensity = intensityCurve.Evaluate(time % 1f) * shakeIntensity;

        // 计算目标晃动位置
        Vector3 targetPosition = CalculatePositionShake(time, intensity);
        Vector3 targetRotation = CalculateRotationShake(time, intensity);

        if (useSmoothNoise)
        {
            // 平滑过渡到目标位置
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                initialPosition + targetPosition,
                ref positionVelocity,
                smoothTime
            );

            // 平滑旋转
            Quaternion targetRot = initialRotation * Quaternion.Euler(targetRotation);
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRot,
                Time.deltaTime / smoothTime
            );
        }
        else
        {
            // 直接应用晃动
            transform.localPosition = initialPosition + targetPosition;
            transform.localRotation = initialRotation * Quaternion.Euler(targetRotation);
        }
    }

    Vector3 CalculatePositionShake(float time, float intensity)
    {
        // 使用不同频率的Perlin噪声创建自然的晃动
        float noiseX = Mathf.PerlinNoise(time * shakeSpeed, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0f, time * shakeSpeed) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(time * shakeSpeed, time * shakeSpeed) * 2f - 1f;

        // 呼吸效果 - 缓慢的上下浮动
        float breathing = enableBreathing ?
            Mathf.Sin(time * breathingSpeed) * breathingIntensity : 0f;

        return new Vector3(
            noiseX * intensity * positionStrength.x,
            (noiseY * intensity * positionStrength.y) + breathing,
            noiseZ * intensity * positionStrength.z
        );
    }

    Vector3 CalculateRotationShake(float time, float intensity)
    {
        // 旋转晃动使用不同的噪声频率
        float rotX = Mathf.PerlinNoise(time * (shakeSpeed * 0.7f), 100f) * 2f - 1f;
        float rotY = Mathf.PerlinNoise(200f, time * (shakeSpeed * 0.9f)) * 2f - 1f;
        float rotZ = Mathf.PerlinNoise(time * (shakeSpeed * 1.1f), 300f) * 2f - 1f;

        return new Vector3(
            rotX * intensity * rotationStrength.x,
            rotY * intensity * rotationStrength.y,
            rotZ * intensity * rotationStrength.z
        );
    }

    #region 公共控制方法

    /// <summary>
    /// 设置晃动强度
    /// </summary>
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }

    /// <summary>
    /// 启用或禁用晃动
    /// </summary>
    public void EnableShake(bool enable)
    {
        enableShake = enable;

        if (!enable)
        {
            // 禁用时平滑回到初始位置
            StopAllCoroutines();
            StartCoroutine(SmoothReset());
        }
    }

    /// <summary>
    /// 设置晃动速度
    /// </summary>
    public void SetShakeSpeed(float speed)
    {
        shakeSpeed = speed;
    }

    /// <summary>
    /// 立即重置到初始位置
    /// </summary>
    public void ResetImmediately()
    {
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
    }

    /// <summary>
    /// 触发一次强烈的晃动冲击
    /// </summary>
    public void AddImpulse(float impulseStrength, float duration = 0.5f)
    {
        StartCoroutine(ImpulseShake(impulseStrength, duration));
    }

    #endregion

    #region 协程方法

    private IEnumerator SmoothReset()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsed < smoothTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / smoothTime;

            transform.localPosition = Vector3.Lerp(startPos, initialPosition, t);
            transform.localRotation = Quaternion.Slerp(startRot, initialRotation, t);

            yield return null;
        }

        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
    }

    private IEnumerator ImpulseShake(float strength, float duration)
    {
        float originalIntensity = shakeIntensity;
        float elapsed = 0f;

        // 突然增加强度
        shakeIntensity = originalIntensity + strength;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 逐渐减弱冲击效果
            shakeIntensity = Mathf.Lerp(originalIntensity + strength, originalIntensity, elapsed / duration);
            yield return null;
        }

        shakeIntensity = originalIntensity;
    }

    #endregion

    #region 编辑器调试方法

    [ContextMenu("测试晃动效果")]
    private void TestShake()
    {
        if (!enableShake)
        {
            EnableShake(true);
        }
        AddImpulse(0.2f, 1f);
    }

    [ContextMenu("重置位置")]
    private void EditorReset()
    {
        ResetImmediately();
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        // 在Scene视图中显示晃动范围
        if (enableShake)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shakeIntensity * 0.5f);
        }
    }
}
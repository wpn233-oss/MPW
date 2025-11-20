using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light)), DisallowMultipleComponent]
public class FlickerLight : MonoBehaviour
{
    [Tooltip("This is the minimal intensity that will be possible for this light.")]
    [Range(0.1f, 1f)] public float MinIntensity = 0.5f;
    [Tooltip("This is the maximum intensity that will be possible for this light.")]
    [Range(1.1f, 3f)] public float MaxIntensity = 2.3f;
    [Tooltip("Influence how fast the light will change.")]
    [Range(0.05f, 0.5f)] public float Speed = 0.15f;

    private Light _light;

    private float _time = 0;
    private float _targetIntensity;
    private float _previousIntensity;


    private void Awake() {
        _light = GetComponent<Light>();
    }

    private void FixedUpdate() {
        _time += Time.fixedDeltaTime;
        float t = _time / Speed;
        _light.intensity = Mathf.Lerp(_previousIntensity, _targetIntensity, t);

        if (Mathf.Approximately(_light.intensity, _targetIntensity)) {
            _previousIntensity = _light.intensity;
            _targetIntensity = Random.Range(MinIntensity, MaxIntensity);
            _time = 0;
        }
    }
}
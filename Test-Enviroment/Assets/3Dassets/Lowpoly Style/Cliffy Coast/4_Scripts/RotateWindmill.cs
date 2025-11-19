using UnityEngine;

public class RotateWindmill : MonoBehaviour
{
    [Tooltip("Whether the windmill will rotate when WindStrength is greater than 0.")]
    public bool Active = false;
    [Tooltip("Randomizes start rotation, so that multiple windmills look more natural.")]
    public bool RandomStartRotation = true;
    [Tooltip("Wind strength. Highter strength will make the arms rotate faster.")]
    [Range(0, 1)]
    public float WindStrength = .5f;

    void Start() {
        if (RandomStartRotation) {
            transform.Rotate(0f, 0f, Random.value * 360f);
        }
    }

    void Update() {
        if (Active) {
            transform.Rotate(0f, 0f, 120f * WindStrength * Time.deltaTime);
        }
    }
}

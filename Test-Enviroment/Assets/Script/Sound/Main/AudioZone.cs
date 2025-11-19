using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioZone : MonoBehaviour
{
    [Header("区域音频设置")]
    public AudioClip clip;
    public float minDistance = 5f;
    public float maxDistance = 25f;
    public bool loop = true;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 1f; // 完全3D声音
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
    }

    void Start()
    {
        if (clip != null)
        {
            source.volume = AudioManager.Instance != null
                ? AudioManager.Instance.GetVolume(AudioType.BGM)
                : 1f;
            source.Play();
        }
    }

    void Update()
    {
        // 与全局BGM音量保持同步
        if (AudioManager.Instance != null)
        {
            source.volume = AudioManager.Instance.GetVolume(AudioType.BGM);
        }
    }
}

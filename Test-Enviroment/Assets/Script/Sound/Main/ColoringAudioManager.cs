using UnityEngine;

public class ColoringAudioManager : MonoBehaviour
{
    public static ColoringAudioManager Instance { get; private set; }

    [Header("上色场景音效音源")]
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 从全局 AudioManager 获取当前 SFX 音量
        if (AudioManager.Instance != null)
            sfxSource.volume = AudioManager.Instance.defaultUIVolume;
    }

    void Update()
    {
        // 实时同步全局音量变化
        if (AudioManager.Instance != null)
            sfxSource.volume = AudioManager.Instance.defaultUIVolume;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxSource.volume);
    }
}

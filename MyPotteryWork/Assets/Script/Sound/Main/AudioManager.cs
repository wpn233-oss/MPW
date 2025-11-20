using UnityEngine;
using System.Collections.Generic;

public enum AudioType
{
    BGM,
    UI
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音源通道")]
    public AudioSource bgmSource; // 背景+环境音
    public AudioSource uiSource;  // UI音效

    [Header("默认音量 (0-1)")]
    [Range(0f, 1f)] public float defaultBGMVolume = 0.8f;
    [Range(0f, 1f)] public float defaultUIVolume = 1f;

    private Dictionary<AudioType, float> volumes = new Dictionary<AudioType, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitAudioSources();
        LoadVolumeSettings();
    }

    private void InitAudioSources()
    {
        if (bgmSource == null)
            bgmSource = CreateChildSource("BGM Source", true);

        if (uiSource == null)
            uiSource = CreateChildSource("UI Source", false);
    }

    private AudioSource CreateChildSource(string name, bool loop)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform);
        var src = child.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        return src;
    }

    // 播放背景音乐（主菜单或非区域音乐）
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.volume = GetVolume(AudioType.BGM);
        bgmSource.Play();
    }

    // 播放UI音效
    public void PlayUI(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        uiSource.PlayOneShot(clip, GetVolume(AudioType.UI) * volumeScale);
    }

    // 调整音量
    public void SetVolume(AudioType type, float value)
    {
        value = Mathf.Clamp01(value);
        volumes[type] = value;
        SaveVolumeSettings();

        switch (type)
        {
            case AudioType.BGM:
                bgmSource.volume = value;
                break;
            case AudioType.UI:
                uiSource.volume = value;
                break;
        }
    }

    public float GetVolume(AudioType type)
    {
        return volumes.ContainsKey(type) ? volumes[type] : 1f;
    }

    private void LoadVolumeSettings()
    {
        volumes[AudioType.BGM] = PlayerPrefs.GetFloat("Volume_BGM", defaultBGMVolume);
        volumes[AudioType.UI] = PlayerPrefs.GetFloat("Volume_UI", defaultUIVolume);

        bgmSource.volume = volumes[AudioType.BGM];
        uiSource.volume = volumes[AudioType.UI];
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("Volume_BGM", volumes[AudioType.BGM]);
        PlayerPrefs.SetFloat("Volume_UI", volumes[AudioType.UI]);
        PlayerPrefs.Save();
    }
}

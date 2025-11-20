using UnityEngine;

[CreateAssetMenu(fileName = "SceneAudioProfile", menuName = "Audio/Scene Profile")]
public class SceneAudioProfile : ScriptableObject
{
    [Header(" 场景背景音乐（可为空）")]
    public AudioClip bgmClip;

    [Header(" 场景环境音（可为空）")]
    public AudioClip ambienceClip;

    [Header(" 自动播放选项")]
    public bool playOnStart = true;
}

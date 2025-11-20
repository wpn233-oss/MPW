using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("音频源")]
    public AudioSource backgroundSource;
    public AudioSource playerSource;
    public AudioSource uiSource;

    [Header("玩家音效")]
    public AudioClip[] grassFootsteps;
    public AudioClip[] stoneFootsteps;
    public AudioClip[] waterFootsteps;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip sprintSound;

    [Header("地形音效")]
    public AudioClip[] dirtFootsteps;
    public AudioClip[] sandFootsteps;

    [Header("UI音效")]
    public AudioClip uiClick;
    public AudioClip uiHover;
    public AudioClip dialogueOpen;
    public AudioClip dialogueNext;

    private Dictionary<string, AudioClip[]> footstepMap = new Dictionary<string, AudioClip[]>();

    void Awake()
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

        // 初始化脚步声映射
        footstepMap.Add("Grass", grassFootsteps);
        footstepMap.Add("Stone", stoneFootsteps);
        footstepMap.Add("Water", waterFootsteps);
        footstepMap.Add("Dirt", dirtFootsteps);
        footstepMap.Add("Sand", sandFootsteps);
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (backgroundSource.isPlaying) backgroundSource.Stop();
        backgroundSource.clip = clip;
        backgroundSource.loop = true;
        backgroundSource.Play();
    }

    public void PlayFootstep(string textureName, float volume = 1f)
    {
        if (footstepMap.ContainsKey(textureName))
        {
            AudioClip[] clips = footstepMap[textureName];
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            playerSource.PlayOneShot(clip, volume);
        }
        else
        {
            if (grassFootsteps.Length > 0)
            {
                AudioClip clip = grassFootsteps[Random.Range(0, grassFootsteps.Length)];
                playerSource.PlayOneShot(clip, volume);
            }
        }
    }

    public void PlayJumpSound() => playerSource.PlayOneShot(jumpSound);
    public void PlayLandSound() => playerSource.PlayOneShot(landSound);
    public void PlaySprintSound() => playerSource.PlayOneShot(sprintSound);

    public void PlayUIClick() => uiSource.PlayOneShot(uiClick);
    public void PlayUIHover() => uiSource.PlayOneShot(uiHover);
    public void PlayDialogueOpen() => uiSource.PlayOneShot(dialogueOpen);
    public void PlayDialogueNext() => uiSource.PlayOneShot(dialogueNext);

    public void SetMasterVolume(float volume) => AudioListener.volume = volume;
}